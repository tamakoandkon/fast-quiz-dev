<template>
  <CommonPageLayout>
    <template #breadcrumb>
      <UBreadcrumb :items="links" class="py-2" />
    </template>

    <!-- 阅卷头部：试卷信息 + 批改状态 -->
    <div class="bg-white rounded-lg shadow p-5 mb-5">
      <div class="flex flex-wrap items-center justify-between gap-3">
        <div class="min-w-0">
          <h1 class="text-2xl font-bold text-gray-800 truncate">
            {{ template.name || "未命名试卷" }}
          </h1>
          <p class="text-sm text-gray-500 mt-1 flex items-center gap-2">
            <span>作答 ID：{{ attemptId }}</span>
            <UBadge :color="getReviewBadgeColor(attempt.status)" size="sm">
              {{ getAttemptStatusText(attempt.status) }}
            </UBadge>
          </p>
        </div>

        <div class="flex items-center gap-2">
          <UButton
            :to="reviewListTo"
            color="neutral"
            variant="outline"
            icon="i-lucide-arrow-left"
          >
            返回批改列表
          </UButton>
        </div>
      </div>

      <!-- 状态提示 -->
      <div
        v-if="!isReviewable"
        class="mt-4 rounded-lg px-4 py-3 text-sm"
        :class="
          attempt.status === QuizAttemptStatus.Graded
            ? 'bg-green-50 text-green-700 border border-green-200'
            : 'bg-amber-50 text-amber-700 border border-amber-200'
        "
      >
        <template v-if="attempt.status === QuizAttemptStatus.Graded">
          本卷已完成批改（Graded），当前为只读查看。
        </template>
        <template v-else-if="attempt.status === QuizAttemptStatus.Submitted">
          考试尚未结束，暂不能开始批改。请到考试结束后再回来阅卷（#2）。
        </template>
        <template v-else>
          该作答当前状态为「{{ getAttemptStatusText(attempt.status) }}」，
          尚不符合可批改条件（仅 ended 后的已提交卷可阅）。
        </template>
      </div>
    </div>

    <div v-if="loaded" class="mb-4 flex items-center justify-between">
      <div class="text-sm text-gray-600">
        全卷共 <b>{{ totalQuestionCount }}</b> 题，其中
        <b class="text-primary-600">{{ manualQuestions.length }}</b> 题需人工判定
        （其余客观题由系统按标准答案判定，无需人工阅卷）。
      </div>
      <div
        v-if="manualQuestions.length > 0"
        class="text-sm text-gray-500 flex items-center gap-2"
      >
        <span>本卷已判定</span>
        <b :class="judgedCount === manualQuestions.length ? 'text-green-600' : 'text-gray-700'">
          {{ judgedCount }}
        </b>
        <span>/ {{ manualQuestions.length }} 题</span>
      </div>
    </div>

    <!-- 需要人工判定的题目（#1 / #4：仅展示主观（Manual）题） -->
    <div v-if="loaded" class="space-y-4">
      <!-- 阅卷操作区（仅当本卷可批改时可用） -->
      <div
        v-if="isReviewable"
        class="sticky top-0 z-10 flex flex-col sm:flex-row items-stretch sm:items-center justify-between gap-2 bg-gray-50/90 backdrop-blur px-4 py-3 rounded-lg shadow-sm border"
      >
        <div class="text-sm text-gray-600 flex items-center gap-1">
          <span>判定方式：</span>
          <span class="text-gray-500">点击每题右侧 ✔️（对）/ ❌（错）切换（#5 手动判定）。</span>
        </div>
        <UButton
          color="success"
          size="md"
          icon="i-lucide-check-check"
          :disabled="!manualQuestions.length"
          @click="completedAndNext"
        >
          {{
            manualQuestions.length
              ? "完成阅卷并自动进行下一份（#6）"
              : "本卷无人工题，直接标记完成"
          }}
        </UButton>
      </div>

      <QuestionRecordReviewCard
        v-for="(question, questionIndex) in manualQuestions"
        :key="question.id || questionIndex"
        :question="question"
        :index="questionIndex + 1"
        :tenant-id="tenantId"
        :readonly="!isReviewable"
        @judged="handleJudged"
      />

      <!-- 空状态：没有需要人工判定的主观题 -->
      <div
        v-if="manualQuestions.length === 0"
        class="bg-white rounded-lg shadow p-12 text-center text-gray-500"
      >
        本卷全部为客观题（选择 / 自动匹配填空），无需人工批改。
        <div v-if="isReviewable" class="mt-4">
          <UButton color="success" icon="i-lucide-check-check" @click="completedAndNext">
            标记为已批阅并进入下一份
          </UButton>
        </div>
      </div>
    </div>
  </CommonPageLayout>
</template>

<script setup lang="ts">
import type { BreadcrumbItem } from "@nuxt/ui";

const { $api } = useNuxtApp();
const toast = useToast();
const router = useRouter();
const route = useRoute();

const tenantId = ref("");
const templateId = ref("");
const attemptId = ref("");

const attempt = ref<QuizAttempDto>({});
const template = ref<QuizTemplateDto>({});
const allQuestionRecords = ref<QuestionRecordWithAnswerDto[]>([]);

const loaded = ref(false);
const links = ref<BreadcrumbItem[]>([]);

const reviewListTo = computed(() => {
  if (!templateId.value) return "#";
  return `/tenant/${tenantId.value}/dash/quiz-template/${templateId.value}/review`;
});

/** 需要人工判定的题（主观 / Manual） */
const manualQuestions = computed<QuestionRecordWithAnswerDto[]>(() =>
  allQuestionRecords.value.filter((q) => needsManualMarking(q)),
);

const totalQuestionCount = computed(() => allQuestionRecords.value.length);

const getAttemptStatusText = (status?: QuizAttemptStatus): string => {
  switch (status) {
    case QuizAttemptStatus.Pending:
      return "未开始";
    case QuizAttemptStatus.InProgress:
      return "进行中";
    case QuizAttemptStatus.Submitted:
      return "待批阅";
    case QuizAttemptStatus.Graded:
      return "已批阅";
    default:
      return "未知";
  }
};

const getReviewBadgeColor = (status?: QuizAttemptStatus) => {
  switch (status) {
    case QuizAttemptStatus.Graded:
      return "green";
    case QuizAttemptStatus.Submitted:
      return "yellow";
    case QuizAttemptStatus.InProgress:
      return "blue";
    case QuizAttemptStatus.Pending:
      return "gray";
    default:
      return "gray";
  }
};

/** 本卷此刻是否允许批改：限时卷需考试结束；且作答需处于已提交。 */
const isReviewable = computed(
  () =>
    attempt.value.status === QuizAttemptStatus.Submitted &&
    canStartReviewing(template.value),
);

/** 本次会话中已人工判定过的题目 id 集合。 */
const judgedIds = ref<Set<string>>(new Set());
const judgedCount = computed(() => judgedIds.value.size);

const handleJudged = (questionId: string) => {
  judgedIds.value = new Set(judgedIds.value).add(questionId);
};

/** 拉取一份作答的完整题目列表（with-answer），含分页。 */
const fetchAllQuestionRecords = async () => {
  const records: QuestionRecordWithAnswerDto[] = [];
  let cursor: string | undefined = undefined;
  const pageSize = 100;
  let guard = 0;

  while (guard++ < 50) {
    const res = await $api.tenant.quizTemplateQuizAttemptQuestionWithAnswerList(
      tenantId.value,
      attemptId.value,
      { Cursor: cursor, Count: pageSize },
    );
    const page = res.data ?? [];
    records.push(...page);
    if (page.length < pageSize) break;
    cursor = page[page.length - 1].id ?? undefined;
    if (!cursor) break;
  }
  return records;
};

/** 拉取某模板下全部作答（用于构建“待阅队列”）。 */
const fetchAllAttempts = async (): Promise<QuizAttempDto[]> => {
  const attempts: QuizAttempDto[] = [];
  let cursor: string | undefined = undefined;
  const pageSize = 100;
  let guard = 0;

  while (guard++ < 50) {
    const res = await $api.tenant.quizTemplateQuizAttemptList(
      tenantId.value,
      templateId.value,
      { Cursor: cursor, Count: pageSize, IsPrevious: false },
    );
    const page = res.data ?? [];
    attempts.push(...page);
    if (page.length < pageSize) break;
    cursor = page[page.length - 1].id ?? undefined;
    if (!cursor) break;
  }
  return attempts;
};

/** 待阅队列里“下一份”卷：按交卷时间先后，取仍待批的最早一份（排除本卷自身）。 */
const resolveNextAttemptId = (queue: QuizAttempDto[]): string | undefined => {
  const eligible = queue
    .filter((a) => isAttemptNeedsReview(a, template.value))
    .filter((a) => a.id !== attemptId.value)
    .sort((a, b) => (a.submitTime ?? "").localeCompare(b.submitTime ?? ""));
  return eligible[0]?.id;
};

/** 阅完一份 → 标记 Graded，并自动切到队列里下一份待阅卷（#6）。 */
const completedAndNext = async () => {
  if (!isReviewable.value) {
    toast.add({ title: "当前状态不可阅卷", color: "error" });
    return;
  }

  try {
    await $api.tenant.quizTemplateQuizAttemptUpdate(tenantId.value, attemptId.value, {
      status: QuizAttemptStatus.Graded,
    });
  } catch (e: any) {
    const title = e?.response?.data?.title ?? "标记失败";
    toast.add({ title: `完成阅卷失败：${title}`, color: "error" });
    return;
  }
  toast.add({ title: "本卷批阅完成", color: "success" });

  // 完成后构建“仍待批”队列，自动取最早一份切换（#6）
  const allAttempts = await fetchAllAttempts();
  const nextId = resolveNextAttemptId(allAttempts);
  if (nextId) {
    router.push(
      `/tenant/${tenantId.value}/dash/quiz-template/${templateId.value}/review/${nextId}`,
    );
  } else {
    toast.add({ title: "没有更多待阅试卷，返回列表", color: "info" });
    router.push(reviewListTo.value);
  }
};

onMounted(async () => {
  tenantId.value = route.params.tenantId as string;
  attemptId.value = route.params.attemptId as string;

  try {
    const result = await $api.tenant.quizTemplateQuizAttemptDetail(
      tenantId.value,
      attemptId.value,
    );
    attempt.value = result.data;

    templateId.value = attempt.value.quizTemplateId ?? "";

    if (templateId.value) {
      const templateResult = await $api.tenant.quizTemplateDetail(
        tenantId.value,
        templateId.value,
      );
      template.value = templateResult.data;
    }

    allQuestionRecords.value = await fetchAllQuestionRecords();
  } catch (e: any) {
    const title = e?.response?.data?.title ?? "加载失败";
    toast.add({ title: `阅卷页加载失败：${title}`, color: "error" });
  } finally {
    loaded.value = true;
  }

  links.value.push({
    label: "试卷管理",
    icon: "i-lucide-book-open",
    to: `/tenant/${tenantId.value}/dash/quiz-template`,
  });
  links.value.push({
    label: template.value.name ?? templateId.value,
    to: `/tenant/${tenantId.value}/dash/quiz-template/${templateId.value}`,
  });
  links.value.push({
    label: "批改试卷",
    to: reviewListTo.value,
  });
  links.value.push({
    label: attemptId.value.slice(0, 8),
  });
});
</script>
