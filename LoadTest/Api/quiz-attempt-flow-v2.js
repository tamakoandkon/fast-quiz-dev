/**
 * FastQuiz 答题接口压力测试
 * ============================================================================
 * 被测目标：**作答接口**（写入路径）
 *
 *   PUT /tenant/{tenantId}/quiz-template/quiz-attempt/question/{questionId}
 *
 * ---------------------------------------------------------------------------
 * 两个阶段
 * ---------------------------------------------------------------------------
 * 【准备阶段】setup() —— 只跑一次，串行建好全部前置数据：
 *    ① 登录（管理员）
 *    ② 创建测试题库        压力测试题库 + 随机数
 *    ③ 创建 10 道单选题     压力测试题目 + 随机数
 *    ④ 创建考试模板        压力测试考试 + 随机数（无限开放时间 / 答题 120 分钟）
 *    ⑤ 创建抽题规则         抽 10 道 SingleChoice，level 与题库对齐
 *    ⑥ 铺试卷池             count=N
 *    ⑦ 校验快照池
 *    ⑧ 创建 1 份考试快照
 *    ⑨ 发布考试
 *    ⑩ 开始考试             创建答题记录(Attempt)，同时拿到试卷
 *    ⑪ 置为 InProgress      取题的前置条件
 *    ⑫ 获取无答案题目列表     含选项内容，供压测本地随机挑
 *    ⑬ （不建 VU 账号）
 *
 * 【压测阶段】default() —— 所有 VU 共享①的 token、⑩的 attempt、⑫的题目：
 *    每个 VU 每轮：从 10 道题里随机挑 1 道 → 随机挑 1 个选项 → PUT 提交。
 *    每轮只发 1 个请求，全部打在作答接口上。
 *
 * ---------------------------------------------------------------------------
 * 为什么能共享一个 attempt（不用每个 VU 建账号和答题记录）
 * ---------------------------------------------------------------------------
 * 读源码确认（QuizAttemptService.AnswerQuestionAsync）：
 *   该方法只按 questionRecordId 反查 attempt，**全程不校验 userId**，
 *   也不检查 attempt 是否属于调用者。唯一硬性检查是：
 *     - attempt 必须处于 InProgress
 *     - 当前时间 <= StartTime + DurationMinutes
 * 所以任意账号都能往同一个 attempt 上作答，无需建用户、无需建 attempt。
 *
 * 同理，取题目列表（GetAllQuestionWithoutAnswerDtoAsync）只在 attempt
 * **不是** InProgress 时才要求 QuizAttempt 权限；先置 InProgress 即可。
 *
 * ⚠️⚠️ 两个必须知道的约束 ⚠️⚠️
 *
 * 1) 压测期间**不能收尾**。一旦把 attempt 推到 Submitted/Graded，
 *    所有 VU 的作答会立刻 500「试卷不在进行中」。压完即止。
 *
 * 2) 这个 attempt 有 **120 分钟寿命**（TEMPLATE_DURATION_MINUTES）。
 *    计时从 setup 里置 InProgress 那一刻开始，超时后作答全部 500。
 *    反复跑压测要重跑 setup；需要更长窗口就调大 durationMinutes
 *    （验证器上限 2880 分钟 = 48 小时）。
 *
 * ---------------------------------------------------------------------------
 * 运行方式
 * ---------------------------------------------------------------------------
 *   # 冒烟：1 个 VU 跑 10 秒
 *   k6 run -e VUS=1 -e HOLD=10s LoadTest/Api/quiz-attempt-flow-v2.js
 *
 *   # 正式压测
 *   k6 run -e VUS=50 -e HOLD=2m LoadTest/Api/quiz-attempt-flow-v2.js
 *
 *   # 换服务地址（默认 https://localhost:7040）
 *   k6 run -e PORT=5026 -e SCHEME=http LoadTest/Api/quiz-attempt-flow-v2.js
 *   k6 run -e HOST=192.168.1.50 LoadTest/Api/quiz-attempt-flow-v2.js
 *   k6 run -e BASE_URL=https://quiz.example.com LoadTest/Api/quiz-attempt-flow-v2.js
 *
 * ---------------------------------------------------------------------------
 * 配置项一览（全部可用 -e 覆盖）
 * ---------------------------------------------------------------------------
 *   BASE_URL          完整地址，优先级最高；给了它就忽略 HOST/PORT/SCHEME
 *   HOST              主机，默认 localhost
 *   PORT              端口，默认 7040
 *   SCHEME            协议 http/https，默认 https
 *   INSECURE_TLS      是否跳过证书校验，默认 true（本地自签证书必须跳过）
 *   TENANT_ID         租户 id，默认 system_tenant
 *   ADMIN_USERNAME    管理员用户名，默认 defaultadmin
 *   ADMIN_PASSWORD    管理员密码，默认 fastquiz
 *
 *   QUESTION_POOL     题库题数，默认 10
 *   QUESTIONS_PER_RULE 抽题规则抽取数，默认 10
 *   RULE_COUNT        抽题规则条数，默认 1
 *   QUESTION_SCORE    每题分值，默认 5
 *   QUESTION_LEVEL    题目难度，默认 1（规则必须与之一致）
 *   TEMPLATE_DURATION_MINUTES  答题时长(分钟)，默认 120
 *   SNAPSHOT_POOL     试卷池数量，默认 max(VUS*2, 20)
 *   PUBLISH           是否发布考试，默认 true
 *
 *   VUS               并发虚拟用户数，默认 20
 *   RAMP_UP           爬坡时长，默认 15s
 *   HOLD              满负载保持时长，默认 1m
 *   RAMP_DOWN         降坡时长，默认 10s
 *   CYCLES_PER_ITER   每个 iteration 串行跑几轮作答，默认 20
 */

import http from "k6/http";
import { check, fail, group, sleep } from "k6";
import { Counter, Rate, Trend } from "k6/metrics";

// ===========================================================================
// SECTION 1. 配置
// ===========================================================================

// ===========================================================================
// SECTION 1a. 服务地址（可配置，默认 localhost:7040）
// ===========================================================================
//
// 三种配法，按你的习惯选一种：
//
//   ① 只改端口（最常用，本机换 profile 时）：
//        k6 run -e PORT=5026 LoadTest/Api/quiz-attempt-flow-v2.js
//
//   ② 只改主机（同一台机器上跑 k6，打到别的机器）：
//        k6 run -e HOST=192.168.1.50 LoadTest/Api/quiz-attempt-flow-v2.js
//
//   ③ 直接给完整地址（一并覆盖 HOST/PORT/SCHEME）：
//        k6 run -e BASE_URL=https://quiz.example.com LoadTest/Api/quiz-attempt-flow-v2.js
//
// 优先级：BASE_URL 最高，给了它就忽略 HOST/PORT/SCHEME。
//
const HOST = __ENV.HOST || "localhost";
const PORT = __ENV.PORT || "7040";
// https 还是 http：默认 https。7040 是 launchSettings 里的 https profile，
// 换成 http profile 的 5026 端口时记得一起改 -e SCHEME=http
const SCHEME = __ENV.SCHEME || "https";

const BASE_URL = stripTrailingSlash(
  __ENV.BASE_URL || `${SCHEME}://${HOST}:${PORT}`,
);

// 是否跳过 TLS 证书校验。
// 本地默认自签证书，必须跳过，否则所有请求直接 TLS 失败。
// 打到正式环境（有真证书）时建议关掉：-e INSECURE_TLS=false
const INSECURE_TLS = bool(__ENV.INSECURE_TLS, true);

// ---- 默认管理员 / 默认租户 -------------------------------------------------
// 这三个值与源码常量一一对应，改之前请对着源码确认：
//   FastQuiz.Auth/Domain/Entities/User.cs
//       DefaultAdminId       = "default_admin"
//       DefaultAdminUsername = "defaultadmin"
//       DefaultAdminPassword = "fastquiz"
//   FastQuiz.Auth/Domain/Entities/Tenant.cs
//       DefaultTenantId      = "system_tenant"
const TENANT_ID = __ENV.TENANT_ID || "system_tenant";
const ADMIN_USERNAME = __ENV.ADMIN_USERNAME || "defaultadmin";
const ADMIN_PASSWORD = __ENV.ADMIN_PASSWORD || "fastquiz";

// ---- 数据规模 --------------------------------------------------------------
// 题库建 10 道题（全部单选题）
const QUESTION_POOL = num(__ENV.QUESTION_POOL, 10);
// 抽题规则从题库抽 10 道题 —— 与题库数量持平（一份试卷抽满整个题库）
const QUESTIONS_PER_RULE = num(__ENV.QUESTIONS_PER_RULE, 10);
const RULE_COUNT = num(__ENV.RULE_COUNT, 1);
const QUESTION_SCORE = num(__ENV.QUESTION_SCORE, 5);
const QUESTION_LEVEL = num(__ENV.QUESTION_LEVEL, 1);

// ---- 命名规范 --------------------------------------------------------------
// 题库名称 = 压力测试题库 + 随机数
// 题目内容 = 压力测试题目 + 随机数
// 考试模板 名称 & 详情 = 压力测试考试 + 随机数（两者相同）
const GROUP_NAME_PREFIX = __ENV.GROUP_NAME_PREFIX || "压力测试题库";
const QUESTION_NAME_PREFIX = __ENV.QUESTION_NAME_PREFIX || "压力测试题目";
const TEMPLATE_NAME_PREFIX = __ENV.TEMPLATE_NAME_PREFIX || "压力测试考试";

// ---- 考试模板 --------------------------------------------------------------
// 答题时长(分钟)。注意这不是"考试开放时间"——开放时间由 isUnlimitedTime 控制。
// ⚠️ 这个值同时决定压测窗口：attempt 置 InProgress 后，
//    UtcNow 超过 StartTime + 这个分钟数，作答就开始 500。
const TEMPLATE_DURATION_MINUTES = num(__ENV.TEMPLATE_DURATION_MINUTES, 120);

// ---- 压测规模 --------------------------------------------------------------
const VUS = num(__ENV.VUS, 50000);
const RAMP_UP = __ENV.RAMP_UP || "15s";
const HOLD = __ENV.HOLD || "1m";
const RAMP_DOWN = __ENV.RAMP_DOWN || "10s";

const SNAPSHOT_POOL = num(1);
const PUBLISH = bool(__ENV.PUBLISH, true);

// ---- 单轮答题行为 ----------------------------------------------------------
// 每个 iteration 内串行跑多少轮「随机挑题 + 随机挑选项 + 提交」。
// 每轮 = 1 次 PUT 作答请求，这就是被压的核心写路径。
const CYCLES_PER_ITER = num(__ENV.CYCLES_PER_ITER, 20);

// ===========================================================================
// SECTION 2. options
// ===========================================================================

export const options = {
  // 本地自签证书必须跳过校验，否则所有请求直接 TLS 失败。
  // 由 -e INSECURE_TLS=false 关闭（打正式环境时用）。
  insecureSkipTLSVerify: INSECURE_TLS,
  scenarios: {
    answering: {
      executor: "ramping-vus",
      startVUs: 0,
      stages: [
        { duration: RAMP_UP, target: VUS },
        { duration: HOLD, target: VUS },
        { duration: RAMP_DOWN, target: 0 },
      ],
      gracefulRampDown: "30s",
      gracefulStop: "60s",
    },
  },
  thresholds: {
    http_req_failed: ["rate<0.01"],
    // 作答是本脚本唯一被压的接口，这里就是核心 SLA
    fastquiz_http_answer: ["p(95)<300", "p(99)<800"],
    fastquiz_cycle_ok: ["rate>0.98"],
  },
  summaryTrendStats: ["avg", "min", "med", "max", "p(90)", "p(95)", "p(99)"],
};

// import { textSummary } from 'https://jslib.k6.io/k6-summary/0.0.2/index.js';

// ===========================================================================
// SECTION 2b. 每秒请求数 / 链路流量（网络层）
// ===========================================================================
//
// 这两个指标是 k6 内置的，handleSummary 里直接用：
//   http_reqs    —— 每秒请求数 RPS（Counter + rate）
//   data_sent    —— 发送字节数（Counter，含请求头+请求体）
//   data_received—— 接收字节数（Counter，含响应头+响应体）
//
// 注意：它们统计的是**全部** HTTP 请求，包括 setup 阶段那十几次准备请求，
// 所以数值里含 setup 的固定开销（量级很小，可忽略）。
//
// 换算成链路吞吐（MB/s）：总字节 / 整体耗时（秒）。
// 整体耗时用 data.state.testRunDurationMs —— k6 v2.2.0 实测 data.state 只暴露
// isStdOutTTY / isStdErrTTY / testRunDurationMs 三个字段，这是唯一可靠的
// 「整个测试窗口」时长来源。
// ⚠️ 不要用 iteration_duration.values.max 代替：实测该 Trend 的 values
//    全为 0（含 max），拿它当除数会直接算出 0 秒、把吞吐炸成天文数字。
function bytes(b) {
  if (!isFinite(b) || b <= 0) return "0 B";
  if (b >= 1024 * 1024 * 1024)
    return (b / 1024 / 1024 / 1024).toFixed(2) + " GB";
  if (b >= 1024 * 1024) return (b / 1024 / 1024).toFixed(2) + " MB";
  if (b >= 1024) return (b / 1024).toFixed(2) + " kB";
  return b.toFixed(0) + " B";
}

function counterCount(data, name) {
  const m = data.metrics[name];
  if (!m || !m.values || m.values.count === undefined) return 0;
  return m.values.count;
}

/** 用 Counter 的派生 rate（每秒速率）来算，比除总时长更准 */
function perSec(data, name) {
  const m = data.metrics[name];
  if (!m || !m.values || m.values.rate === undefined) return 0;
  return m.values.rate;
}

/** 整个测试窗口的秒数（用于把总字节换算成 MB/s） */
function testDurationSeconds(data) {
  const d = data.state && data.state.testRunDurationMs;
  if (isFinite(d) && d > 0) return d / 1000;
  return 0;
}

// export function handleSummary(data) {
//   // 取出 Counter 指标的值
//   const sent = data.metrics.data_sent?.values?.count ?? 0;
//   const received = data.metrics.data_received?.values?.count ?? 0;

//   // 格式化字节数
//   const fmt = (bytes) => {
//     if (bytes >= 1024 * 1024) return (bytes / 1024 / 1024).toFixed(2) + ' MB';
//     if (bytes >= 1024) return (bytes / 1024).toFixed(2) + ' kB';
//     return bytes + ' B';
//   };

//   // 拼到默认输出后面
//   const extra = [
//     '',
//     `data_sent......................: ${fmt(sent)}`,
//     `data_received..................: ${fmt(received)}`,
//   ].join('\n');

//   return {
//     stdout: textSummary(data, { indent: ' ', enableColors: true }) + extra + '\n',
//   };
// }

// ===========================================================================
// SECTION 3. 自定义指标
// 把每一个接口单独打点，这样报告里能直接看出是哪一步慢
// ===========================================================================

const metrics = {
  // 准备阶段（只跑一次，用来确认准备流程本身有没有问题）
  setupErrors: new Counter("fastquiz_setup_errors"),

  // 每轮随机作答（压测主体）
  cycleOk: new Rate("fastquiz_cycle_ok"),
  cycleErrors: new Counter("fastquiz_cycle_errors"),
  answersSubmitted: new Counter("fastquiz_answers_submitted"),

  // 各接口耗时
  httpAnswer: new Trend("fastquiz_http_answer", true),
};

// ===========================================================================
// SECTION 4. 小工具
// ===========================================================================

function stripTrailingSlash(s) {
  return s.replace(/\/+$/, "");
}

function num(v, fallback) {
  const n = Number(v);
  return v === undefined || v === "" || isNaN(n) ? fallback : n;
}

function bool(v, fallback) {
  if (v === undefined || v === "") return fallback;
  return String(v).toLowerCase() === "true" || String(v) === "1";
}

/** 拼接绝对地址 */
function url(path) {
  return `${BASE_URL}${path}`;
}

/** 拼租户前缀，几乎所有业务接口都在 /tenant/{tenantId}/ 下面 */
function t(path) {
  return `/tenant/${TENANT_ID}${path}`;
}

function jsonHeaders(token) {
  const h = { "Content-Type": "application/json" };
  if (token) h["Authorization"] = `Bearer ${token}`;
  return h;
}

/** 安全解析 JSON：一个脏响应不应该把整个 VU 打挂 */
function asJson(res) {
  try {
    return res.json();
  } catch (e) {
    return null;
  }
}

/** 准备阶段断言：失败就打印详情并计数 */
function expect(res, label, statuses) {
  const ok = statuses.indexOf(res.status) >= 0;
  if (!ok) {
    metrics.setupErrors.add(1);
    console.error(
      `[setup] ${label} 失败: status=${res.status} body=${String(res.body || "").slice(0, 400)}`,
    );
  }
  return ok;
}

/** 相对当前时间偏移 minutes 分钟的 ISO 字符串 */
function isoOffset(minutes) {
  return new Date(Date.now() + minutes * 60 * 1000).toISOString();
}

/**
 * 随机数后缀，用于题库名/题目名。
 * 格式：时间戳(36进制) + 4 位随机，例如「压力测试题库m8k3x2f1045」
 * 保证多次运行不重名，也保证同一批里的 10 道题互不相同。
 */
function randomSuffix() {
  return `${Date.now().toString(36)}${Math.floor(Math.random() * 10000)}`;
}

// ===========================================================================
// SECTION 5. 准备阶段
// ===========================================================================

/** 2. 登录，返回管理员 token */
function setupLogin() {
  const res = http.post(
    url("/auth/login"),
    JSON.stringify({ username: ADMIN_USERNAME, password: ADMIN_PASSWORD }),
    { headers: jsonHeaders(), tags: { name: "setup/login" } },
  );
  if (!expect(res, "POST /auth/login", [200])) {
    fail(
      `管理员登录失败。请确认：\n` +
        `  1. 服务已启动在 ${BASE_URL}\n` +
        `  2. 账号 ${ADMIN_USERNAME} / ${ADMIN_PASSWORD} 正确（源码常量见文件头 SECTION 1）`,
    );
  }
  const body = asJson(res);
  if (!body || !body.accessToken) fail("登录响应里没有 accessToken");
  return body.accessToken;
}

/**
 * 3. 创建测试题库（题目组）
 *
 * 命名规范（按需求定死）：
 *   题库名称 = 「压力测试题库」+ 随机数
 * 用管理员的 token 创建。
 */
function setupCreateQuestionGroup(token) {
  const name = `${GROUP_NAME_PREFIX}${randomSuffix()}`;

  const res = http.post(
    url(t("/question-group")),
    JSON.stringify({ name: name, remark: "k6 压力测试" }),
    { headers: jsonHeaders(token), tags: { name: "setup/create-group" } },
  );
  if (!expect(res, "POST /question-group", [200])) {
    fail(`创建题库失败：name=${name}`);
  }
  const body = asJson(res);
  if (!body || !body.id) fail("创建题库响应里没有 id");

  console.log(`[setup] 题库已创建: ${name} (id=${body.id})`);
  return { id: body.id, name: name };
}

/**
 * 4. 批量创建题目（全部单选题）
 *
 * 命名规范（按需求定死）：
 *   题目内容 = 「压力测试题目」+ 随机数
 *
 * 全部 type = 'SingleChoice'（单选题）。
 * 注意：抽题规则按 (groupId, questionType, level) 三要素过滤，
 *       所以题目类型和难度必须与规则完全一致，否则规则抽不到题。
 */
function setupCreateQuestions(token, groupId, count) {
  const ids = [];
  const names = [];

  for (let i = 0; i < count; i++) {
    const title = `${QUESTION_NAME_PREFIX}${randomSuffix()}`;

    const payload = {
      content: title,
      level: QUESTION_LEVEL,
      type: "SingleChoice", // 全部单选题
      answers: [
        {
          content: `${title} - 选项A`,
          isCorrect: false,
          sortOrder: 0,
          matchStrategy: "Exact",
        },
        {
          content: `${title} - 选项B（正确答案）`,
          isCorrect: true,
          sortOrder: 1,
          matchStrategy: "Exact",
        },
      ],
      remark: "k6 压力测试",
    };

    const res = http.post(
      url(t(`/question-group/${groupId}/question`)),
      JSON.stringify(payload),
      { headers: jsonHeaders(token), tags: { name: "setup/create-question" } },
    );
    if (expect(res, `POST /question-group/{id}/question #${i}`, [200])) {
      const body = asJson(res);
      if (body && body.id) {
        ids.push(body.id);
        names.push(title);
      }
    }
  }

  if (ids.length < count) {
    fail(`题目创建不完整：期望 ${count} 道，实际成功 ${ids.length} 道`);
  }

  console.log(`[setup] 题目已创建 ${ids.length} 道（全部单选题）`);
  return { ids: ids, names: names };
}

/**
 * 5. 创建考试模板
 *
 * 两个「时间」概念是独立的，不冲突：
 *   IsUnlimitedTime  = 考试**开放时间**无限制（不是答题时长）
 *                      为 true 时 BeginTime/EndTime 不参与校验（不生效）
 *   DurationMinutes  = 单次**答题时长**（分钟），本题固定 120
 */
function setupCreateTemplate(token) {
  const name = `${TEMPLATE_NAME_PREFIX}${randomSuffix()}`;

  const payload = {
    // 名称与详情相同
    name: name,
    description: name,
    remark: "",
    // 上限 100（CreateQuizTemplateCommandValidator: 1..100）
    attemptLimit: 100,
    // 考试开放时间无限制（≠ 答题时长）
    isUnlimitedTime: true,
    // 无限开放时间时这两个字段不生效，给个占位值即可
    beginTime: isoOffset(-60),
    endTime: isoOffset(60 * 24 * 365),
    // 答题时长：120 分钟（决定压测窗口，见文件头约束 2）
    durationMinutes: TEMPLATE_DURATION_MINUTES,
    // 不指定用户组
    userGroupId: null,
  };

  const res = http.post(url(t("/quiz-template")), JSON.stringify(payload), {
    headers: jsonHeaders(token),
    tags: { name: "setup/create-template" },
  });
  if (!expect(res, "POST /quiz-template", [200])) {
    fail(`创建考试模板失败：name=${name}`);
  }
  const body = asJson(res);
  if (!body || !body.id) fail("创建考试模板响应里没有 id");

  console.log(`[setup] 考试模板已创建: ${body.name} (id=${body.id})`);
  return body.id;
}

/**
 * 6. 创建抽题规则：从指定题库里抽 10 道单选题
 *
 * level 必须与题库里题目的 level **精确对齐**！
 * 抽题是按 (groupId, questionType, level) 三要素过滤的，不是范围匹配。
 * 实测：题目 level=1 而规则 level=0 时，规则能建（验证器允许 0-5），
 *       但建快照会 400「试题数量不符合需求，生成试卷已中断」——
 *       因为验证器注释里写的「0表示全部」并没有在抽题逻辑里实现。
 */
function setupCreateRules(token, templateId, groupId) {
  const ruleIds = [];
  for (let r = 0; r < RULE_COUNT; r++) {
    const payload = {
      groupId: groupId,
      questionType: "SingleChoice", // 单选题
      level: QUESTION_LEVEL, // 与题库题目难度对齐（默认 1）
      count: QUESTIONS_PER_RULE, // 抽 10 道
      score: QUESTION_SCORE,
    };
    const res = http.post(
      url(t(`/quiz-template/${templateId}/rule`)),
      JSON.stringify(payload),
      { headers: jsonHeaders(token), tags: { name: "setup/create-rule" } },
    );
    if (!expect(res, `POST /quiz-template/{id}/rule #${r}`, [200])) {
      fail(
        `创建抽题规则失败。请确认规则与题库的 (类型, 难度) 对齐：` +
          `questionType=SingleChoice level=${QUESTION_LEVEL}`,
      );
    }
    const body = asJson(res);
    if (body && body.id) ruleIds.push(body.id);
    console.log(
      `[setup] 抽题规则已创建: 抽 ${QUESTIONS_PER_RULE} 道 ` +
        `SingleChoice level=${QUESTION_LEVEL} (id=${body ? body.id : "?"})`,
    );
  }
  return ruleIds;
}

/** 7. 创建试卷快照 */
function setupCreateSnapshots(token, templateId) {
  const res = http.post(
    url(t(`/quiz-template/${templateId}/quiz-snapshot`)),
    JSON.stringify({ count: SNAPSHOT_POOL }),
    { headers: jsonHeaders(token), tags: { name: "setup/create-snapshot" } },
  );
  if (!expect(res, "POST /quiz-template/{id}/quiz-snapshot", [204])) {
    fail(
      `创建试卷快照失败（期望 204）。最常见原因：\n` +
        `  1. 题库题数不够：规则要 ${QUESTIONS_PER_RULE} 道，实际少于这个数\n` +
        `  2. count 超过上限 100（CreateQuizSnapshotCommandValidator: 1..100）`,
    );
  }
}

/** 8. 读回快照列表，确认池子真的建够了 */
function setupListSnapshots(token, templateId) {
  const res = http.get(
    url(t(`/quiz-template/${templateId}/quiz-snapshot?count=200`)),
    { headers: jsonHeaders(token), tags: { name: "setup/list-snapshot" } },
  );
  if (!expect(res, "GET /quiz-template/{id}/quiz-snapshot", [200])) {
    fail("读取试卷快照列表失败");
  }
  const list = asJson(res) || [];
  if (list.length < SNAPSHOT_POOL) {
    fail(
      `试卷快照池不足：期望 ${SNAPSHOT_POOL} 份，实际 ${list.length} 份。\n` +
        `请提高 QUESTION_POOL 或降低 SNAPSHOT_POOL。`,
    );
  }
  return list;
}

/**
 * 10. 创建一份考试快照并返回它
 *
 * 与步骤 ⑦ 的区别：⑦ 是批量铺「试卷池」(count=N)，这里只建 1 份。
 * 发布之后不能再建快照，所以本步骤必须排在发布**之前**。
 */
function setupCreateOneSnapshot(token, templateId) {
  const before = setupListSnapshotsRaw(token, templateId).length;

  const res = http.post(
    url(t(`/quiz-template/${templateId}/quiz-snapshot`)),
    JSON.stringify({ count: 1 }),
    {
      headers: jsonHeaders(token),
      tags: { name: "setup/create-one-snapshot" },
    },
  );
  if (!expect(res, "POST /quiz-template/{id}/quiz-snapshot (count=1)", [204])) {
    fail("创建考试快照失败");
  }

  const after = setupListSnapshotsRaw(token, templateId);
  if (after.length <= before) {
    fail(`创建快照后数量未增加：before=${before} after=${after.length}`);
  }
  const snapshot = after[0]; // 最新一份
  console.log(
    `[setup] 考试快照已创建: id=${snapshot.id} totalScore=${snapshot.totalScore}`,
  );
  return snapshot;
}

/** 读快照列表（不做数量断言，供探测用） */
function setupListSnapshotsRaw(token, templateId) {
  const res = http.get(
    url(t(`/quiz-template/${templateId}/quiz-snapshot?count=200`)),
    { headers: jsonHeaders(token), tags: { name: "setup/list-snapshot" } },
  );
  if (!expect(res, "GET /quiz-template/{id}/quiz-snapshot", [200])) {
    fail("读取试卷快照列表失败");
  }
  return asJson(res) || [];
}

/**
 * 11. 开始考试（创建答题记录 Attempt）
 *
 * 这一步的返回里就带了试卷信息（quizSampleId），即"获取到试卷"。
 * POST body 传 {} 即可（接口签名里没有 body 参数）。
 */
function setupStartQuizAttempt(token, templateId) {
  const res = http.post(
    url(t(`/quiz-template/${templateId}/quiz-attempt`)),
    JSON.stringify({}),
    { headers: jsonHeaders(token), tags: { name: "setup/start-attempt" } },
  );
  if (!expect(res, "POST /quiz-template/{id}/quiz-attempt", [200])) {
    fail(
      `开始考试失败。常见原因：\n` +
        `  1. 考试未发布（无法参加未发布的考试）\n` +
        `  2. 还有未结束的考试（同一用户+同一模板只允许一个）\n` +
        `  3. 次数已达上限（attemptLimit）\n` +
        `  4. 不在可参加的时间范围内`,
    );
  }
  const attempt = asJson(res);
  if (!attempt || !attempt.id) fail("开始考试响应里没有 id");

  console.log(
    `[setup] 考试已开始: attemptId=${attempt.id} 状态=${attempt.status} ` +
      `试卷快照=${attempt.quizSampleId} 得分字段=${attempt.score}`,
  );
  return attempt;
}

/**
 * 12. 获取无答案的题目列表
 *
 * ⚠️ 关键约束（QuizAttemptService.GetAllQuestionWithoutAnswerDtoAsync）：
 *    如果答题记录**不是 InProgress 状态**，就必须拥有 Permissions.QuizAttempt
 *    权限，否则抛 PermissionException -> 403。
 *
 *    所以取题必须满足其一：
 *      (a) 先把 attempt 置为 InProgress 再取题；或
 *      (b) 调用者拥有 QuizAttempt 权限（管理员有，普通考生账号没有）
 *
 *    这正是之前用 VU 专属账号取题 403 的原因：账号无此权限，且当时
 *    attempt 还是 Pending。按 (a) 先置 InProgress，普通账号也能取题。
 */
function setupFetchQuestions(token, attemptId, status) {
  const res = http.get(
    url(t(`/quiz-template/quiz-attempt/${attemptId}/question?count=50`)),
    { headers: jsonHeaders(token), tags: { name: "setup/fetch-questions" } },
  );
  if (!expect(res, "GET quiz-attempt/{id}/question", [200])) {
    fail(
      `获取无答案题目列表失败（当前 attempt 状态=${status}）。\n` +
        `若为 403：attempt 不是 InProgress 且调用者没有 QuizAttempt 权限。\n` +
        `若为其他错误：检查 attemptId 是否正确。`,
    );
  }
  const questions = asJson(res) || [];
  if (questions.length === 0) {
    fail("无答案题目列表为空，试卷快照可能没有题目");
  }

  console.log(`[setup] 已获取无答案题目 ${questions.length} 道:`);
  for (let i = 0; i < questions.length; i++) {
    const q = questions[i];
    const ans = q.answers || [];
    console.log(
      `  ${i + 1}. [${q.type}] level=${q.level} 选项=${ans.length}个 "${q.markUpContent}"`,
    );
  }
  return questions;
}

/** 把答题记录状态推进到指定值（状态机不允许跳级） */
function setupSetAttemptStatus(token, attemptId, status) {
  const res = http.put(
    url(t(`/quiz-template/quiz-attempt/${attemptId}?status=${status}`)),
    null,
    { headers: jsonHeaders(token), tags: { name: "setup/set-attempt-status" } },
  );
  if (!expect(res, `PUT quiz-attempt/{id}?status=${status}`, [204])) {
    fail(
      `设置答题记录状态失败：-> ${status}\n` +
        `状态机是 Pending -> InProgress -> Submitted -> Graded，不能跳级。`,
    );
  }
  console.log(`[setup] 答题记录状态已置为 ${status}`);
}

/**
 * 9. 发布考试
 * 必须放在创建快照之后：发布后就不能再建快照了。
 */
function setupPublish(token, templateId) {
  const res = http.put(
    url(t(`/quiz-template/${templateId}/published`)),
    JSON.stringify({ isPublished: true }),
    { headers: jsonHeaders(token), tags: { name: "setup/publish" } },
  );
  if (!expect(res, "PUT /quiz-template/{id}/published", [204])) {
    fail("发布考试失败");
  }
  console.log("[setup] 考试已发布 (isPublished=true)");
}

/**
 * setup()：整个准备流程，只跑一次。
 * 返回的 data 会被所有 VU 共享，所以里面只能放只读数据。
 */
export function setup() {
  // ---- 启动前的参数自检，早失败早报错，别等压到一半才发现 ----
  // 共享 attempt 模型下，试卷池不再决定并发能力（所有 VU 共用同一份试卷），
  // 保留这个校验只是防止参数组合错误。
  if (SNAPSHOT_POOL > 100) {
    fail(`SNAPSHOT_POOL(${SNAPSHOT_POOL}) 超过单次创建上限 100，请分批创建`);
  }
  if (QUESTIONS_PER_RULE > QUESTION_POOL) {
    fail(
      `QUESTIONS_PER_RULE(${QUESTIONS_PER_RULE}) 不能大于 QUESTION_POOL(${QUESTION_POOL})`,
    );
  }
  if (CYCLES_PER_ITER < 1) {
    fail(
      `CYCLES_PER_ITER(${CYCLES_PER_ITER}) 必须 >= 1，否则压测不产生任何请求`,
    );
  }

  const data = {};

  group("setup: 准备数据", function () {
    console.log("[setup] 1/13 登录管理员");
    const adminToken = setupLogin();
    data.adminToken = adminToken;

    console.log(`[setup] 2/13 创建测试题库（${GROUP_NAME_PREFIX}xxx）`);
    const groupInfo = setupCreateQuestionGroup(adminToken);
    const groupId = groupInfo.id;
    data.questionGroupId = groupId;
    data.questionGroupName = groupInfo.name;

    console.log(
      `[setup] 3/13 创建题目 x${QUESTION_POOL}（${QUESTION_NAME_PREFIX}xxx，全部单选题）`,
    );
    const questions = setupCreateQuestions(adminToken, groupId, QUESTION_POOL);
    data.questionIds = questions.ids;
    data.questionNames = questions.names;
    data.questionCount = questions.ids.length;

    console.log("[setup] 4/13 创建考试模板");
    const templateId = setupCreateTemplate(adminToken);
    data.quizTemplateId = templateId;

    console.log(
      `[setup] 5/13 创建抽题规则 x${RULE_COUNT}（每条抽 ${QUESTIONS_PER_RULE} 题，共 ${QUESTION_POOL} 道题库）`,
    );
    const ruleIds = setupCreateRules(adminToken, templateId, groupId);
    data.ruleIds = ruleIds;

    console.log(`[setup] 6/13 铺试卷池 x${SNAPSHOT_POOL}`);
    setupCreateSnapshots(adminToken, templateId);

    console.log("[setup] 7/13 校验快照池");
    const snapshots = setupListSnapshots(adminToken, templateId);
    data.snapshots = snapshots.map((s) => s.id);
    data.totalScore = snapshots.length > 0 ? snapshots[0].totalScore : 0;

    console.log(`[setup] 8/13 创建 1 份考试快照`);
    const snapshot = setupCreateOneSnapshot(adminToken, templateId);
    data.snapshotId = snapshot.id;

    if (PUBLISH) {
      console.log("[setup] 9/13 发布考试");
      setupPublish(adminToken, templateId);
    } else {
      console.log("[setup] 9/13 跳过发布（PUBLISH=false）");
    }

    // ---- 准备流程收尾三连：开始考试 -> 取试卷 -> 取无答案题目列表 ----
    console.log("[setup] 10/13 开始考试（创建答题记录，同时获取试卷）");
    const attempt = setupStartQuizAttempt(adminToken, templateId);
    data.attemptId = attempt.id;
    data.quizSampleId = attempt.quizSampleId;
    data.attemptStatus = attempt.status;

    console.log("[setup] 11/13 置为 InProgress（取题的前置条件）");
    setupSetAttemptStatus(adminToken, attempt.id, "InProgress");

    console.log(
      "[setup] 12/13 获取无答案题目列表（含选项内容，供压测随机作答）",
    );
    const fetched = setupFetchQuestions(adminToken, attempt.id, "InProgress");
    data.fetchedQuestionCount = fetched.length;
    data.quizAttemptQuestionIds = fetched.map((q) => q.id);
    // VU 直接复用这份数据本地随机挑题挑选项，不用再请求接口
    data.questions = fetched;

    console.log("[setup] 13/13 压测阶段共用这一个 attempt，不建 VU 账号");
    data.vuUsers = null;
  });

  console.log(
    `[setup] 完成: 题库=${data.questionGroupName} 题目=${data.questionCount}道 ` +
      `考试=${data.quizTemplateId} 快照=${data.snapshots.length} ` +
      `VU账号=${data.vuUsers ? data.vuUsers.length : "共用管理员"}`,
  );

  return data;
}

// ===========================================================================
// SECTION 6. 压测阶段
// ===========================================================================
//
// 设计要点（共享尝试模型）：
//   - 不新建 attempt、不新建用户。所有 VU 共用 setup 阶段建好的那**一个** attempt。
//   - 该 attempt 已在 setup 里置为 InProgress，所以任何账号都能作答（见下）。
//   - 每个 VU 每轮：从 10 道题里**随机挑 1 道**，再**随机挑 1 个选项**提交。
//     内容对错不影响结论 —— 压的是写入路径。
//
// 为什么共享 attempt 可行（已读源码确认）：
//   QuizAttemptService.AnswerQuestionAsync 只按 questionRecordId 反查 attempt，
//   全程**不对 userId 做任何校验**，也不检查 attempt 归属。
//   唯一硬性检查是 attempt 必须处于 InProgress 且在答题时限内。
//
// ⚠️ 因此本压测**不能收尾**：
//   一旦把 attempt 推到 Submitted/Graded，所有 VU 的作答会立刻
//   500「试卷不在进行中」。压完即止，收尾留给人工。
//
// ⚠️ 答题时限：AnswerQuestionAsync 会校验
//   UtcNow <= StartTime + DurationMinutes(120)。
//   StartTime = setup 里置 InProgress 的时刻，所以这个 attempt 只有
//   120 分钟寿命。超过后所有作答会 500，需要重跑 setup。
// ---------------------------------------------------------------------------

/**
 * 生成作答内容。
 *
 * 每道题的选项内容在 setup 阶段就随题目一起取回来了（data.questions），
 * 所以这里不需要再请求接口 —— 直接本地随机挑，把请求全部集中到写路径上。
 */
function pickRandomAnswer(question) {
  const answers = question.answers || [];
  if (answers.length === 0) return null;
  return answers[Math.floor(Math.random() * answers.length)].markUpContent;
}

/** 从题目池里随机挑一道 */
function pickRandomQuestion(questions) {
  return questions[Math.floor(Math.random() * questions.length)];
}

/**
 * 跑一轮：随机挑 1 道题 -> 随机挑 1 个选项 -> 提交。
 *
 * 这是压测的核心，也是**唯一**会发生的业务请求。
 * 没有创建 attempt、没有取题、没有状态流转 —— 全是开胃菜之外的纯写压测。
 */
function runOneCycle(data, identity) {
  const questions = data.questions;
  if (!questions || questions.length === 0) {
    metrics.cycleErrors.add(1);
    metrics.cycleOk.add(false);
    return false;
  }

  const question = pickRandomQuestion(questions);
  const value = pickRandomAnswer(question);
  if (value === null) {
    metrics.cycleErrors.add(1);
    metrics.cycleOk.add(false);
    return false;
  }

  const res = http.put(
    url(t(`/quiz-template/quiz-attempt/question/${question.id}`)),
    JSON.stringify([{ value: value }]),
    {
      headers: jsonHeaders(identity.token),
      tags: { name: "cycle/answer" },
    },
  );
  metrics.httpAnswer.add(res.timings.duration);

  const ok = check(res, { "cycle/answer -> 204": (r) => r.status === 204 });
  if (ok) {
    metrics.answersSubmitted.add(1);
    metrics.cycleOk.add(true);
  } else {
    metrics.cycleErrors.add(1);
    metrics.cycleOk.add(false);
  }
  return ok;
}

/**
 * default()：每个 VU 的入口。
 *
 * 无状态循环：每轮独立随机挑题作答，VU 之间、轮次之间互不影响，
 * 因为大家共用同一个 attempt 和同一份 questionRecord 列表。
 */
export default function (data) {
  const identity = { token: data.adminToken, username: ADMIN_USERNAME };

  group("压测: 随机作答", function () {
    for (let c = 0; c < CYCLES_PER_ITER; c++) {
      runOneCycle(data, identity);
      sleep(1);
    }
  });
}

// ===========================================================================
// SECTION 7. 报告
// ===========================================================================

export function handleSummary(data) {
  const line = "=".repeat(72);
  const parts = [];
  parts.push("");
  parts.push(line);
  parts.push("FastQuiz 答题链路压测结果");
  parts.push(line);
  parts.push(`  目标地址 BASE_URL            ${BASE_URL}`);
  parts.push(`  租户     TENANT_ID           ${TENANT_ID}`);
  parts.push(`  并发     VUS                 ${VUS}`);
  parts.push(
    `  题库     ${GROUP_NAME_PREFIX} + 随机数   题量 ${QUESTION_POOL} 道（单选题）`,
  );

  function metric(name) {
    const m = data.metrics[name];
    if (!m) return "(无数据)";
    const v = m.values;
    const out = [];

    // k6 的 Counter 也会带一个派生 rate（每秒速率），这里只显示 count，
    // 避免把"每秒次数"误读成百分比。
    if (v.count !== undefined) out.push(`count=${v.count}`);
    if (v.rate !== undefined) out.push(`rate=${(v.rate * 100).toFixed(2)}%`);
    if (v.avg !== undefined) out.push(`avg=${v.avg.toFixed(2)}ms`);
    if (v["p(90)"] !== undefined) out.push(`p90=${v["p(90)"].toFixed(2)}ms`);
    if (v["p(95)"] !== undefined) out.push(`p95=${v["p(95)"].toFixed(2)}ms`);
    if (v["p(99)"] !== undefined) out.push(`p99=${v["p(99)"].toFixed(2)}ms`);
    if (v.max !== undefined) out.push(`max=${v.max.toFixed(2)}ms`);
    return out.join("  ");
  }

  parts.push("");
  parts.push("【整体】");
  parts.push(`  http_req_duration           ${metric("http_req_duration")}`);
  parts.push(`  http_req_failed             ${metric("http_req_failed")}`);
  parts.push(`  fastquiz_cycle_ok           ${metric("fastquiz_cycle_ok")}`);

  parts.push("");
  parts.push("【作答吞吐】");
  parts.push(
    `  成功作答数 answers_submitted  ${metric("fastquiz_answers_submitted")}`,
  );
  parts.push(
    `  失败轮次 cycle_errors         ${metric("fastquiz_cycle_errors")}`,
  );

  parts.push("");
  parts.push("【核心接口耗时】");
  parts.push(
    `  作答 (压测目标)               ${metric("fastquiz_http_answer")}`,
  );

  // ---- 网络层：请求数 + 链路流量 ----------------------------------------
  const reqs = counterCount(data, "http_reqs");
  const rps = perSec(data, "http_reqs");
  const sent = counterCount(data, "data_sent");
  const received = counterCount(data, "data_received");
  const secs = testDurationSeconds(data);

  parts.push("");
  parts.push("【请求数 / 链路流量】");
  parts.push(
    `  请求数     http_reqs          count=${reqs}  RPS=${rps.toFixed(2)}/s`,
  );
  if (rps > 0) {
    parts.push(
      `  发送       data_sent          ${bytes(sent)}  (${bytes(sent / secs)}/s)`,
    );
    parts.push(
      `  接收       data_received      ${bytes(received)}  (${bytes(received / secs)}/s)`,
    );
    parts.push(
      `  双向合计                      ${bytes(sent + received)}  (${bytes((sent + received) / secs)}/s)`,
    );
    parts.push(
      `  单请求平均 发送/接收           ${bytes(sent / reqs)} / ${bytes(received / reqs)}`,
    );
    parts.push(`  测试窗口时长                   ${secs.toFixed(1)}s`);
  }

  parts.push("");
  parts.push(line);
  parts.push("");

  return {
    stdout: parts.join("\n"),
  };
}
