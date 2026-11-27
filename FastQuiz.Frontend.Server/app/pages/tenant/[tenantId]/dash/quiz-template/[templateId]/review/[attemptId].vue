<template>
  <CommonPageLayout :description="[template.name, attempt.assignUsername].filter(Boolean).join(' · ')">
    <template #actions>
      <UBadge v-if="loaded" :color="getReviewBadgeColor(attempt.status)" variant="soft">
        {{ getAttemptStatusText(attempt.status) }}
      </UBadge>
      <UButton :to="reviewListTo" color="neutral" variant="outline" icon="i-lucide-arrow-left">
        返回批改列表
      </UButton>
    </template>

    <div v-if="loaded && !isReviewable">
      <!-- 状态提示 -->
      <div
        class="rounded-xl px-4 py-3 text-sm"
        :class="
          attempt.status === QuizAttemptStatus.Graded
            ? 'bg-green-50 text-green-700 border border-green-200'
            : 'bg-amber-50 text-amber-700 border border-amber-200'
        "
      >
        <template v-if="attempt.status === QuizAttemptStatus.Graded">
          本卷已批改完成，可以查看阅卷结果。
        </template>
        <template v-else-if="attempt.status === QuizAttemptStatus.Submitted">
          本场考试结束后开放阅卷。
        </template>
        <template v-else>
          该作答当前状态为「{{ getAttemptStatusText(attempt.status) }}」，
          交卷且开放阅卷后即可批改。
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
          <span class="text-gray-500">用每题右侧的按钮标记正确或错误。</span>
        </div>
        <UButton
          color="success"
          size="md"
          icon="i-lucide-check-check"
          :disabled="judgedCount !== manualQuestions.length"
          @click="completedAndNext"
        >
          {{
            manualQuestions.length
              ? "完成阅卷并打开下一份"
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
      />

      <!-- 空状态：没有需要人工判定的主观题 -->
      <div
        v-if="manualQuestions.length === 0"
        class="workspace-panel p-10 text-center text-muted"
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
      return "success";
    case QuizAttemptStatus.Submitted:
      return "warning";
    case QuizAttemptStatus.InProgress:
      return "info";
    case QuizAttemptStatus.Pending:
      return "neutral";
    default:
      return "neutral";
  }
};

/** 本卷此刻是否允许批改：限时卷需考试结束；且作答需处于已提交。 */
const isReviewable = computed(
  () =>
    attempt.value.status === QuizAttemptStatus.Submitted &&
    canStartReviewing(template.value),
);

/** 从持久化判定计算进度，刷新或续阅时保留已判错和已判对的题目。 */
const judgedCount = computed(() =>
  manualQuestions.value.filter((q) => typeof q.isCorrect === "boolean").length,
);

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
    cursor = page[page.length - 1]?.id ?? undefined;
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
    cursor = page[page.length - 1]?.id ?? undefined;
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
  return eligible[0]?.id ?? undefined;
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

});
</script>
