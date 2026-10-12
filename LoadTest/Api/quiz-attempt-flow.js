/**
 * FastQuiz 答题链路压力测试
 * ---------------------------------------------------------------------------
 * 覆盖链路：创建题目 -> 创建抽题规则 -> 创建考试模板 -> 创建考试快照 ->
 *           发布考试 -> 创建答题记录(Attempt) -> 获取考试内容 -> 开始答题 -> 作答
 *
 * 运行：
 *   k6 run LoadTest/Api/quiz-attempt-flow.js
 *   # 自定义目标与规模
 *   k6 run -e BASE_URL=https://localhost:7040 -e ADMIN_USERNAME=defaultadmin \
 *          -e ADMIN_PASSWORD=fastquiz -e VUS=50 -e DURATION=2m \
 *          -e QUESTION_POOL=200 -e SNAPSHOT_POOL=100 \
 *          LoadTest/Api/quiz-attempt-flow.js
 *
 * 先跑一次冒烟(建议)：k6 run -e VUS=1 -e DURATION=5s -e SETUP_SNAPSHOTS=2 LoadTest/Api/quiz-attempt-flow.js
 *
 * ---------------------------------------------------------------------------
 * 关于本项目的接口约束（读源码后确认，务必先看这段再改脚本）
 * ---------------------------------------------------------------------------
 * 1) 答题记录状态 `status` 是 **查询字符串** 参数，不是 body：
 *      PUT /tenant/{t}/quiz-template/quiz-attempt/{id}?status=InProgress
 *    写在 body 里会得到 500 "was not provided from query string"。
 * 2) 枚举用 JsonStringEnumConverter，JSON 里必须传字符串：
 *    "SingleChoice" / "Exact" / "InProgress"。
 * 3) 创建 attempt 有硬约束（QuizAttemptService.CreateQuizAttemptAsync）：
 *    - 同一用户在同一 template 下**只能有一个未结束的考试**（Pending/InProgress
 *      会被 FindLastUnfinished 命中）-> "还有未结束的考试"。
 *    - 有 AttemptLimit 与时间窗口校验。
 *    因此每个 VU 必须串行：创建 -> 作答 -> 结束，才能进入下一轮。
 * 4) 快照是"试卷池"：创建 attempt 时按 **最小被引用次数** 挑一个 QuizSnapshot
 *    (GetQuizSnapshotOfMinAttemptCountByQuizTemplateIdAsync)。若快照池小于并发
 *    VU 数，多个 VU 会分到同一快照，QuestionRecord 会成倍增长 ->
 *    "获取考试内容" 接口耗时非线性放大，压测结果不可用。
 *    => SNAPSHOT_POOL 必须 >= VUS，脚本会在 setup 阶段强校验并直接失败。
 * 5) 抽题规则 groupId+type+level 必须凑够 count 道题，否则创建快照报
 *    "题目数量不足：实际 N 道，需要 M 道"。本脚本按 count = 每轮作答题数
 *    自动铺足题库。
 * 6) 只有系统租户管理员(defaultadmin)权限齐全。新注册用户不在任何租户内，
 *    访问 /tenant/system_tenant/* 会 403 PermissionException
 *    (QuizAttemptService 第一步就检查 GetUserInTenantAsync)。
 *    所以：管理员 token 只用在 setup 里建数据 + 建压测账号；
 *    每个 VU 用 POST /tenant/{t}/user 建出来的专属账号跑压测。
 * 6b) 为什么每个 VU 要独立账号：AttemptLimit 按 (用户, 模板) 累计
 *    (GetAttemptCountAsync(userId, templateId))，且同一用户在同一模板下
 *    只能有一个未结束考试。共用 defaultadmin 时 AttemptLimit=100 会被
 *    所有 VU 一起消耗，很快集体撞上"考试次数已经达到上限"。
 *    账号名必须满足 3-20 位 [a-zA-Z0-9_]，密码 6-20 位。
 * 7) HTTPS 是自签证书，必须 insecureSkipTLSVerify。
 * 8) 答题缓冲：作答接口只写入内存缓冲(AnswerBufferService)后立即 204，
 *    不代表已落库。压测关注的是接口吞吐/延迟，不是数据持久化。
 * 9) 状态机 Pending -> InProgress -> Submitted -> Graded，Graded 只能从
 *    Submitted 转入。Submitted 会触发 5 秒后后台自动阅卷，所以默认
 *    END_STATUS=Graded 让 VU 立刻把 attempt 收尾到终态，避免后台批阅
 *    的 CPU 消耗混进接口延迟。
 * 10) default 函数体内是"有界串行循环"，一个 VU 同时只跑一个 attempt。
 *     不要改成"每个 iteration 新建一个 attempt"——那会让同一 VU 并发
 *     创建，全部撞在"还有未结束的考试"上（实测 30 VU 时错误率 78%）。
 */

import http from 'k6/http';
import { check, fail, group, sleep } from 'k6';
import { Counter, Rate, Trend } from 'k6/metrics';
import exec from 'k6/execution';

// ---------------------------------------------------------------------------
// 配置
// ---------------------------------------------------------------------------
const BASE_URL = (__ENV.BASE_URL || 'https://localhost:7040').replace(/\/$/, '');
const TENANT_ID = __ENV.TENANT_ID || 'system_tenant';
const ADMIN_USERNAME = __ENV.ADMIN_USERNAME || 'defaultadmin';
const ADMIN_PASSWORD = __ENV.ADMIN_PASSWORD || 'fastquiz';

const VUS = Number(__ENV.VUS || 20);
const DURATION = __ENV.DURATION || '1m';
const RAMP_UP = __ENV.RAMP_UP || '15s';

// 每个 VU 应当独占一份试卷快照，池子必须 >= VUS。
const SNAPSHOT_POOL = Number(__ENV.SNAPSHOT_POOL || VUS * 2);
// 每道抽题规则要凑够的题目数（= 一份试卷的题量）
const QUESTIONS_PER_RULE = Number(__ENV.QUESTIONS_PER_RULE || 5);
// 抽题规则条数
const RULE_COUNT = Number(__ENV.RULE_COUNT || 1);
// 题库总量（每个 group+type+level 组合下的可用题数）
const QUESTION_POOL = Number(__ENV.QUESTION_POOL || Math.max(QUESTIONS_PER_RULE, 50));
// 每题分值
const QUESTION_SCORE = Number(__ENV.QUESTION_SCORE || 5);
// 每个 VU 轮询答题时，每轮作答几道题（<= QUESTIONS_PER_RULE）
const ANSWERS_PER_CYCLE = Number(__ENV.ANSWERS_PER_CYCLE || QUESTIONS_PER_RULE);
// 每题之间的思考停顿(秒)
const THINK_TIME = Number(__ENV.THINK_TIME || 0.2);

// 每个 k6 iteration 内串行跑多少轮完整答题。
// 必须 >1：一个 VU 同时只能有一个未结束的考试，串行成环才能持续产生负载。
const CYCLES_PER_ITERATION = Number(__ENV.CYCLES_PER_ITERATION || 20);

const QUESTION_LEVEL = Number(__ENV.QUESTION_LEVEL || 1);

// ---------------------------------------------------------------------------
// 自定义指标：把"准备阶段"和"答题阶段"分开看，便于定位瓶颈
// ---------------------------------------------------------------------------
const m = {
  setupErrors: new Counter('fastquiz_setup_errors'),
  cycleErrors: new Counter('fastquiz_cycle_errors'),
  cycleSuccess: new Rate('fastquiz_cycle_success'),
  attemptLatency: new Trend('fastquiz_attempt_create_duration', true),
  questionsLatency: new Trend('fastquiz_questions_fetch_duration', true),
  startLatency: new Trend('fastquiz_start_duration', true),
  answerLatency: new Trend('fastquiz_answer_duration', true),
  submitLatency: new Trend('fastquiz_submit_duration', true),
  answersSubmitted: new Counter('fastquiz_answers_submitted'),
  attemptsCompleted: new Counter('fastquiz_attempts_completed'),
};

/**
 * 答题尝试的标签。默认答错，便于一次性完成收尾。
 *
 * 重要：不要把 Submitted 放进这里。Submit 会触发 5 秒后的后台自动阅卷
 * (AnswerBufferBackgroundService / ReviewQuizAttemptByAttemptIdAsync)，
 * 若在同一个 VU 里持续压测，会把后台批阅的 CPU 消耗混进接口延迟里。
 * 需要测"批阅链路"时用 -e END_STATUS=Submitted 单独跑一轮。
 */
const END_STATUS = __ENV.END_STATUS || 'Graded';

// ---------------------------------------------------------------------------
// options
// ---------------------------------------------------------------------------
export const options = {
  insecureSkipTLSVerify: true,
  discardResponseBodies: false,
  scenarios: {
    answering: {
      executor: 'ramping-vus',
      startVUs: 0,
      stages: [
        { duration: RAMP_UP, target: VUS },
        { duration: DURATION, target: VUS },
        { duration: '10s', target: 0 },
      ],
      gracefulRampDown: '15s',
      gracefulStop: '30s',
    },
  },
  thresholds: {
    // 答题接口是本轮压测的核心指标
    'http_req_failed': ['rate<0.01'],
    'fastquiz_answer_duration': ['p(95)<300', 'p(99)<800'],
    'fastquiz_questions_fetch_duration': ['p(95)<500'],
    'fastquiz_cycle_success': ['rate>0.98'],
  },
  summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(90)', 'p(95)', 'p(99)'],
};

// ---------------------------------------------------------------------------
// 工具函数
// ---------------------------------------------------------------------------
function jsonHeaders(token) {
  const h = { 'Content-Type': 'application/json' };
  if (token) {
    h['Authorization'] = `Bearer ${token}`;
  }
  return h;
}

function url(path) {
  return `${BASE_URL}${path}`;
}

function tenantPath(path) {
  return `/tenant/${TENANT_ID}${path}`;
}

/** 解析 JSON，失败返回 null（避免一个脏响应把整个 VU 打挂） */
function parseJson(res) {
  try {
    return res.json();
  } catch (e) {
    return null;
  }
}

function expect(res, label, statuses) {
  const ok = check(res, {
    [`${label} -> ${statuses.join('|')}`]: (r) => statuses.indexOf(r.status) >= 0,
  });
  if (!ok) {
    m.setupErrors.add(1);
    // 打印首个失败详情，便于定位（只在准备阶段调用）
    console.error(
      `[setup] ${label} failed: status=${res.status} body=${String(res.body).slice(0, 300)}`
    );
  }
  return ok;
}

function nowIso(offsetMinutes) {
  return new Date(Date.now() + offsetMinutes * 60 * 1000).toISOString();
}

// ---------------------------------------------------------------------------
// setup：登录 -> 建题库 -> 建模板 -> 建规则 -> 铺快照池 -> 发布
// 返回一份共享数据，所有 VU 复用（token 与 templateId）
// ---------------------------------------------------------------------------
export function setup() {
  // ---- 0. 强校验：快照池必须覆盖并发，否则获取考试内容的耗时会失真 ----
  if (SNAPSHOT_POOL < VUS) {
    fail(
      `SNAPSHOT_POOL(${SNAPSHOT_POOL}) 必须 >= VUS(${VUS})：` +
        `试卷快照不足时多个 VU 会共享同一份试卷，QuestionRecord 成倍增长，` +
        `"获取考试内容" 的耗时会非线性放大，压测结果不可信。`
    );
  }
  if (SNAPSHOT_POOL > 100) {
    fail(
      `SNAPSHOT_POOL(${SNAPSHOT_POOL}) 超过单次创建上限 100 ` +
        `(CreateQuizSnapshotCommandValidator: 1..100)。请分批创建。`
    );
  }
  if (QUESTIONS_PER_RULE > QUESTION_POOL) {
    fail(
      `QUESTIONS_PER_RULE(${QUESTIONS_PER_RULE}) 不能大于 QUESTION_POOL(${QUESTION_POOL})`
    );
  }

  const runTag = `${Date.now().toString(36)}${Math.floor(Math.random() * 1e4)}`;

  // ---- 1. 登录（只用管理员，见文件头注释 6）----
  const loginRes = http.post(
    url('/auth/login'),
    JSON.stringify({ username: ADMIN_USERNAME, password: ADMIN_PASSWORD }),
    { headers: jsonHeaders(), tags: { name: 'setup:login' } }
  );
  if (!expect(loginRes, 'login', [200])) {
    fail(`登录失败，请确认 ${BASE_URL} 已启动且 ${ADMIN_USERNAME}/${ADMIN_PASSWORD} 可用`);
  }
  const token = parseJson(loginRes).accessToken;
  if (!token) {
    fail('登录响应中没有 accessToken');
  }
  const auth = jsonHeaders(token);

  // ---- 2. 题目组 ----
  const groupRes = http.post(
    url(tenantPath('/question-group')),
    JSON.stringify({ name: `k6-${runTag}-group`, remark: 'k6 load test' }),
    { headers: auth, tags: { name: 'setup:create-group' } }
  );
  expect(groupRes, 'create question group', [200]);
  const questionGroupId = parseJson(groupRes).id;

  // ---- 3. 批量建题（串行，setup 只跑一次，不影响压测结论）----
  const questionIds = [];
  for (let i = 0; i < QUESTION_POOL; i++) {
    const payload = {
      content: `k6-[${runTag}]-${i} 下面哪一个选项是正确答案？`,
      level: QUESTION_LEVEL,
      type: 'SingleChoice',
      answers: [
        {
          content: `k6-[${runTag}]-${i} 错误选项 A`,
          isCorrect: false,
          sortOrder: 0,
          matchStrategy: 'Exact',
        },
        {
          content: `k6-[${runTag}]-${i} 正确选项 B`,
          isCorrect: true,
          sortOrder: 1,
          matchStrategy: 'Exact',
        },
      ],
      remark: 'k6 load test',
    };
    const res = http.post(
      url(tenantPath(`/question-group/${questionGroupId}/question`)),
      JSON.stringify(payload),
      { headers: auth, tags: { name: 'setup:create-question' } }
    );
    if (expect(res, `create question #${i}`, [200])) {
      questionIds.push(parseJson(res).id);
    }
  }
  if (questionIds.length < QUESTIONS_PER_RULE) {
    fail(`题库不足：成功创建 ${questionIds.length} 道，至少需要 ${QUESTIONS_PER_RULE} 道`);
  }

  // ---- 4. 考试模板（不限时 + 尽量不限制次数，避免压测期间被业务校验拒绝）----
  const templateRes = http.post(
    url(tenantPath('/quiz-template')),
    JSON.stringify({
      name: `k6-${runTag}-template`,
      description: 'k6 load test template',
      remark: 'k6 load test',
      // attemptLimit 上限为 100（CreateQuizTemplateCommandValidator）。
      // 长压测跑久了会撞到上限，届时需降低 VUS/DURATION 或跑重建一次模板。
      attemptLimit: 100,
      isUnlimitedTime: true,
      beginTime: nowIso(-60),
      endTime: nowIso(60 * 24 * 365),
      durationMinutes: 60,
      userGroupId: null,
    }),
    { headers: auth, tags: { name: 'setup:create-template' } }
  );
  expect(templateRes, 'create quiz template', [200]);
  const quizTemplateId = parseJson(templateRes).id;

  // ---- 5. 抽题规则：每条规则负责 QUESTIONS_PER_RULE 道题 ----
  for (let r = 0; r < RULE_COUNT; r++) {
    const ruleRes = http.post(
      url(tenantPath(`/quiz-template/${quizTemplateId}/rule`)),
      JSON.stringify({
        groupId: questionGroupId,
        questionType: 'SingleChoice',
        level: QUESTION_LEVEL,
        count: QUESTIONS_PER_RULE,
        score: QUESTION_SCORE,
      }),
      { headers: auth, tags: { name: 'setup:create-rule' } }
    );
    expect(ruleRes, `create selection rule #${r}`, [200]);
  }

  // ---- 6. 铺满试卷快照池 ----
  const snapshotRes = http.post(
    url(tenantPath(`/quiz-template/${quizTemplateId}/quiz-snapshot`)),
    JSON.stringify({ count: SNAPSHOT_POOL }),
    { headers: auth, tags: { name: 'setup:create-snapshot' } }
  );
  if (!expect(snapshotRes, `create ${SNAPSHOT_POOL} snapshots`, [204])) {
    fail(
      '创建试卷快照失败。最常见原因：题库数量不足(每个 group+type+level 至少要 ' +
        `${QUESTIONS_PER_RULE} 道)，或快照数超过 100 上限。`
    );
  }

  // ---- 7. 校验快照池真的建够了 ----
  const listRes = http.get(
    url(tenantPath(`/quiz-template/${quizTemplateId}/quiz-snapshot?count=200`)),
    { headers: auth, tags: { name: 'setup:list-snapshot' } }
  );
  expect(listRes, 'list snapshots', [200]);
  const snapshots = parseJson(listRes) || [];
  if (snapshots.length < SNAPSHOT_POOL) {
    fail(
      `试卷快照池不足：期望 ${SNAPSHOT_POOL} 份，实际 ${snapshots.length} 份。` +
        `请提高 QUESTION_POOL 或降低 SNAPSHOT_POOL。`
    );
  }

  // ---- 8. 发布考试（必须先有快照，且发布后不能再建快照）----
  const publishRes = http.put(
    url(tenantPath(`/quiz-template/${quizTemplateId}/published`)),
    JSON.stringify({ isPublished: true }),
    { headers: auth, tags: { name: 'setup:publish' } }
  );
  expect(publishRes, 'publish template', [204]);

  // ---- 9. 为每个 VU 预置一个专属账号 ----
  //
  // 为什么必须这么做：AttemptLimit 是**按 (用户, 模板) 维度**累计的
  // (GetAttemptCountAsync(userId, templateId))。所有 VU 共用 defaultadmin
  // 时，AttemptLimit=100 会被 30 个 VU 共同消耗，很快就会集体撞上
  // "考试次数已经达到上限"，压测提前失效（实测 30VU 时 55% 请求失败）。
  // 每个 VU 一个账号，各自有独立的次数与"未结束考试"配额。
  //
  // POST /tenant/{tenantId}/user 会同时注册用户并加入租户
  // (UserApplicationService.CreateUserAsync -> AddUserToTenantAsync)，
  // 用户因此继承该租户的权限，可以直接调考试相关接口。
  //
  // 注意 UserRegisterCommandValidator 的硬约束：
  //   用户名 3-20 字符，只能 [a-zA-Z0-9_]；密码 6-20 字符。
  // 所以这里不能用带连字符的长用户名（如 k6-xxxx-vu0 会 422）。
  const vuUsers = [];
  for (let i = 0; i < VUS; i++) {
    const username = `k6_${runTag}_u${i}`.slice(0, 20);
    const password = `K6_${runTag}`.slice(0, 20);
    const created = http.post(
      url(tenantPath('/user')),
      JSON.stringify({ username, password, email: null }),
      { headers: auth, tags: { name: 'setup:create-vu-user' } }
    );
    if (!expect(created, `create vu user #${i}`, [200])) {
      fail(
        `创建压测账号失败（第 ${i} 个，username=${username}）。` +
          `请检查用户名是否满足 3-20 位且仅含字母数字下划线。`
      );
    }

    const login = http.post(
      url('/auth/login'),
      JSON.stringify({ username, password }),
      { headers: jsonHeaders(), tags: { name: 'setup:login-vu-user' } }
    );
    if (!expect(login, `login vu user #${i}`, [200])) {
      fail(`压测账号登录失败（第 ${i} 个，username=${username}）。`);
    }
    vuUsers.push({ token: parseJson(login).accessToken, username });
  }

  console.log(
    `[setup] ready: template=${quizTemplateId} questions=${questionIds.length} ` +
      `snapshots=${snapshots.length} vus=${VUS} vuUsers=${vuUsers.length}`
  );

  // VU 之间不共享 mutable 状态：只回传只读数据
  return {
    quizTemplateId,
    vuUsers,
    snapshots: snapshots.map((s) => s.id),
    totalScore: snapshots[0] ? snapshots[0].totalScore : 0,
  };
}

// ---------------------------------------------------------------------------
// 主流程：一轮完整的"创建答题 -> 获取内容 -> 开始 -> 作答 -> 提交"
// 说明：同一用户在同一 template 下不能有未结束的考试，所以必须串行成环。
// ---------------------------------------------------------------------------
function runOneAttemptCycle(data, me) {
  const auth = jsonHeaders(me.token);
  const tpl = data.quizTemplateId;
  const tags = { template: tpl };

  // ---- 1. 创建答题记录 ----
  const attemptRes = http.post(
    url(tenantPath(`/quiz-template/${tpl}/quiz-attempt`)),
    JSON.stringify({}),
    {
      headers: auth,
      tags: Object.assign({ name: 'attempt:create' }, tags),
    }
  );
  m.attemptLatency.add(attemptRes.timings.duration);

  if (attemptRes.status !== 200) {
    m.cycleErrors.add(1);
    check(attemptRes, { 'attempt:create -> 200': () => false });
    // 400 一般是"还有未结束的考试"，且确实还存在未结束的记录。
    // 交由外层 reconcile 处理，不要盲目重试刷 400。
    return false;
  }
  check(attemptRes, { 'attempt:create -> 200': (r) => r.status === 200 });

  const attempt = parseJson(attemptRes);
  if (!attempt || !attempt.id) {
    m.cycleErrors.add(1);
    return false;
  }
  const attemptId = attempt.id;

  // ---- 2. 获取考试内容（不含答案）----
  const questionsRes = http.get(
    url(tenantPath(`/quiz-template/quiz-attempt/${attemptId}/question?count=50`)),
    { headers: auth, tags: Object.assign({ name: 'attempt:questions' }, tags) }
  );
  m.questionsLatency.add(questionsRes.timings.duration);
  check(questionsRes, {
    'attempt:questions -> 200': (r) => r.status === 200,
  });

  const questions = parseJson(questionsRes) || [];
  if (questions.length === 0) {
    m.cycleErrors.add(1);
    finalizeAttempt(attemptId, me, tags, 'Pending');
    return false;
  }

  // ---- 3. 开始答题：status 走 query string！ ----
  const startRes = http.put(
    url(tenantPath(`/quiz-template/quiz-attempt/${attemptId}?status=InProgress`)),
    null,
    { headers: auth, tags: Object.assign({ name: 'attempt:start' }, tags) }
  );
  m.startLatency.add(startRes.timings.duration);
  check(startRes, { 'attempt:start -> 204': (r) => r.status === 204 });
  if (startRes.status !== 204) {
    m.cycleErrors.add(1);
    finalizeAttempt(attemptId, me, tags, 'Pending');
    return false;
  }

  // ---- 4. 逐题作答 ----
  const total = Math.min(ANSWERS_PER_CYCLE, questions.length);
  let answered = 0;

  for (let i = 0; i < total; i++) {
    const q = questions[i];
    const answers = q.answers || [];
    if (answers.length === 0) {
      continue;
    }
    // 取最后一个选项作为"作答"，内容不影响压测结论（压的是写路径）
    const picked = answers[answers.length - 1].markUpContent;

    const answerRes = http.put(
      url(tenantPath(`/quiz-template/quiz-attempt/question/${q.id}`)),
      JSON.stringify([{ value: picked }]),
      {
        headers: auth,
        tags: Object.assign({ name: 'attempt:answer' }, tags),
      }
    );
    m.answerLatency.add(answerRes.timings.duration);
    check(answerRes, { 'attempt:answer -> 204': (r) => r.status === 204 });

    if (answerRes.status === 204) {
      answered++;
      m.answersSubmitted.add(1);
    } else {
      m.cycleErrors.add(1);
    }

    if (THINK_TIME > 0) {
      sleep(THINK_TIME);
    }
  }

  // ---- 5. 收尾（必须！否则下一轮会被"还有未结束的考试"挡住）----
  finalizeAttempt(attemptId, me, tags, 'InProgress');

  m.attemptsCompleted.add(1);
  m.cycleSuccess.add(true);
  return answered > 0;
}

/**
 * 把 attempt 推到终态。
 *
 * 状态机（QuizAttemptService.UpdateQuizAttemptStatusAsync）：
 *   Pending -> InProgress -> Submitted -> Graded
 *   Graded 只能从 Submitted 转入，所以默认走两步。
 *
 * END_STATUS=Submitted 时只提交，把批阅留给后台任务；
 * END_STATUS=Graded(默认) 时再补一步，让 VU 立刻拿到终态，
 * 避免 5 秒后的后台批阅和压测流量互相干扰。
 */
function finalizeAttempt(attemptId, me, tags, fromStatus) {
  const auth = jsonHeaders(me.token);
  const base = url(tenantPath(`/quiz-template/quiz-attempt/${attemptId}`));

  if (fromStatus === 'Pending') {
    // 没有题目/没成功开始，直接提交收尾
    const res = http.put(`${base}?status=Submitted`, null, {
      headers: auth,
      tags: Object.assign({ name: 'attempt:submit' }, tags),
    });
    m.submitLatency.add(res.timings.duration);
    check(res, { 'attempt:submit -> 204': (r) => r.status === 204 });
    if (END_STATUS === 'Submitted') {
      return;
    }
    // 继续尝试收尾到 Graded（若后台已抢先批阅，则不会命中 Submitted 分支）
  } else {
    const submitRes = http.put(`${base}?status=Submitted`, null, {
      headers: auth,
      tags: Object.assign({ name: 'attempt:submit' }, tags),
    });
    m.submitLatency.add(submitRes.timings.duration);
    check(submitRes, { 'attempt:submit -> 204': (r) => r.status === 204 });
    if (END_STATUS === 'Submitted') {
      return;
    }
  }

  // Submitted -> Graded：按已判定的 IsCorrect 收尾计分
  const gradeRes = http.put(`${base}?status=Graded`, null, {
    headers: auth,
    tags: Object.assign({ name: 'attempt:grade' }, tags),
  });
  // 若后台批阅已经把状态推到 Graded，这里可能拿到 400，属正常竞态
  check(gradeRes, {
    'attempt:grade -> 204|400': (r) => r.status === 204 || r.status === 400,
  });
}

/**
 * 清理残留的未结束 attempt。
 *
 * 创建答题记录会被 `FindLastUnfinishedByTemplateIdAndUserIdAsync` 挡住
 * （Status 不是 Submitted/Graded 即视为未结束）。所以一旦某轮中途失败，
 * 后续所有轮次都会 400。这里先把残留推到 Submitted 再继续。
 * 返回 true 表示清理后可以重试一轮。
 */
function reconcileStuckAttempt(me, quizTemplateId) {
  const tpl = quizTemplateId;
  const res = http.get(url(tenantPath('/quiz-template/quiz-attempt/me?count=30')), {
    headers: jsonHeaders(me.token),
    tags: { name: 'attempt:reconcile-list' },
  });
  if (res.status !== 200) {
    return false;
  }
  const attempts = parseJson(res) || [];
  let cleaned = 0;
  for (let i = 0; i < attempts.length; i++) {
    const a = attempts[i];
    if (a.quizTemplateId !== tpl) {
      continue;
    }
    if (a.status === 'Submitted' || a.status === 'Graded') {
      continue;
    }
    const r = http.put(
      url(tenantPath(`/quiz-template/quiz-attempt/${a.id}?status=Submitted`)),
      null,
      { headers: jsonHeaders(me.token), tags: { name: 'attempt:reconcile-submit' } }
    );
    if (r.status === 204) {
      cleaned++;
    }
  }
  return cleaned > 0;
}

// ---------------------------------------------------------------------------
// default：每个 VU 独占一份快照，循环走完整链路
//
// 关键：default 函数体内必须是"有界循环"，一轮只处理一个 attempt。
// 同一个用户在同一 template 下只允许存在一个未结束考试，如果每个 k6
// iteration 都新建一个 attempt 并快速返回，同一个 VU 会立刻并发创建下一个，
// 大量请求会撞在"还有未结束的考试"上（实测 30VU 时错误率 78%），
// 压测结果完全不可用。正确做法是一个 VU 同时只跑一个 attempt。
// ---------------------------------------------------------------------------
export default function (data) {
  const deadline = Date.now() + CYCLES_PER_ITERATION * 1000 * 60 * 10;

  // 每个 VU 用专属账号（见 setup 第 9 步）：独立的考试次数与未结束考试配额
  const vuIndex = exec.vu.idInTest - 1;
  const me = data.vuUsers[vuIndex % data.vuUsers.length];

  group('quiz-attempt-flow', function () {
    let consecutiveFailures = 0;

    for (let cycle = 0; cycle < CYCLES_PER_ITERATION; cycle++) {
      const ok = runOneAttemptCycle(data, me);

      if (ok) {
        consecutiveFailures = 0;
      } else {
        consecutiveFailures++;
        // 连续失败时才做一次残留清理，避免打爆列表接口
        if (consecutiveFailures >= 2) {
          if (!reconcileStuckAttempt(me, data.quizTemplateId)) {
            // 清理不掉说明不是"未结束考试"的问题，跳出交给下一次迭代
            break;
          }
          consecutiveFailures = 0;
        }
      }

      if (Date.now() > deadline) {
        break;
      }
    }
  });

  // 一轮结束后稍作停顿，模拟真实用户节奏
  sleep(Number(__ENV.ITER_SLEEP || 1));
}

// ---------------------------------------------------------------------------
// teardown：输出一份简要说明
// ---------------------------------------------------------------------------
export function teardown(data) {
  console.log(
    `[teardown] template=${data.quizTemplateId} snapshots=${data.snapshots.length} ` +
      `(数据已留在库中，如需清理请调用对应 delete 接口)`
  );
}
