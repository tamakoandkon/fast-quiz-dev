<template>
  <CommonPageLayout v-if="loaded">
    <div class="attempt-shell">
      <header class="attempt-header">
        <div class="attempt-title-row">
          <h1>{{ template.name }}</h1>
          <span class="attempt-progress">已答 {{ getAnsweredCount() }} / {{ questionRecords.length }}</span>
        </div>
        <div class="attempt-toolbar">
          <div class="attempt-timing">
            <CommonCountDown
              v-if="!template.isUnlimitedTime && (template.durationMinutes ?? 0) > 0"
              :minutes="template.durationMinutes ?? 0"
              :start-time="attempt.startTime ?? ''"
              @end="onCountdownEnd"
            />
            <span v-else>不限时</span>
          </div>
          <div class="attempt-toolbar-actions">
            <UButton color="neutral" variant="ghost" @click="showAnswerSheet = true">答题卡</UButton>
            <UButton id="submit-btn" color="primary" :disabled="submitting" :loading="submitting" @click="submitQuizAttempt">交卷</UButton>
          </div>
        </div>
      </header>

      <QuestionRecordAnswerSheetDrawer
        v-model="showAnswerSheet"
        :ids="ids"
        :answered-ids="answeredIds"
        @click="goToQuestion"
      />

      <div class="attempt-view-switch" role="group" aria-label="答题视图">
        <button type="button" :aria-pressed="!showAllQuestion" :class="{ 'is-active': !showAllQuestion }" @click="showAllQuestion = false">逐题</button>
        <button type="button" :aria-pressed="showAllQuestion" :class="{ 'is-active': showAllQuestion }" @click="showAllQuestion = true">全部题目</button>
      </div>

      <div v-if="showAllQuestion" class="attempt-questions">
        <QuestionRecordAnswerCard
          v-for="(question, questionIndex) in questionRecords"
          :key="question.id || questionIndex"
          :question="question"
          :index="questionIndex + 1"
          v-model="selectedAnswers[question.id!]"
          @answer="updateAnswer"
          :id="question.id"
        />
      </div>
      <div v-else class="attempt-questions">
        <template v-if="currentQuestion">
          <QuestionRecordAnswerCard
            :question="currentQuestion"
            :index="currentIndex + 1"
            v-model="selectedAnswers[currentQuestion.id!]"
            @answer="updateAnswer"
            :id="currentQuestion.id"
          />
          <nav class="attempt-pagination" aria-label="题目导航">
            <UButton color="neutral" variant="ghost" :disabled="currentIndex === 0" @click="goToPreviousQuestion">上一题</UButton>
            <span>{{ currentIndex + 1 }} / {{ questionRecords.length }}</span>
            <UButton color="neutral" variant="ghost" :disabled="currentIndex === questionRecords.length - 1" @click="goToNextQuestion">下一题</UButton>
          </nav>
        </template>
      </div>
      <div v-if="questionRecords.length === 0" class="exam-empty">暂无题目</div>
    </div>
  </CommonPageLayout>
  <CommonPageLayout v-else-if="loadError">
    <div class="exam-empty">
      <p>试卷暂时无法加载。</p>
      <UButton color="neutral" variant="outline" @click="loadAttempt">重试</UButton>
    </div>
  </CommonPageLayout>
  <CommonPageLayout v-else><div class="exam-empty" role="status">正在加载试卷…</div></CommonPageLayout>
</template>

<script setup lang="ts">
const { $api } = useNuxtApp();
const route = useRoute();
const router = useRouter();
const toast = useToast();

const attemptId = ref("");
const attempt = ref<QuizAttempDto>({});
const questionRecords = ref<QuestionRecordWithoutAnswerDto[]>([]);
const template = ref<QuizTemplateDto>({});

const ids = computed<string[]>(() => {
  return questionRecords.value
    .map((item) => item.id)
    .filter((id): id is string => id !== null && id !== undefined && id !== "");
});

const showAnswerSheet = ref(false);

const showAllQuestion = ref(false);

const currentQuestionId = ref("");

// 存储每个题目的答案：以 questionId 为 key，值为 string[]
const selectedAnswers = ref<Record<string, string[]>>({});

const tenantId = ref("");

// 计算当前题目
const currentIndex = computed(() => {
  if (!currentQuestionId.value) return 0;
  const index = questionRecords.value.findIndex(
    (q) => q.id === currentQuestionId.value,
  );
  return index >= 0 ? index : 0;
});

const currentQuestion = computed(() => {
  if (questionRecords.value.length === 0) return null;
  return questionRecords.value[currentIndex.value] || questionRecords.value[0];
});

const loaded = ref(false);
const loadError = ref(false);
const submitting = ref(false);
const answeredIds = computed(() => ids.value.filter((id) => {
  const answers = selectedAnswers.value[id] || [];
  return answers.some((answer) => Boolean(answer?.trim()));
}));

const loadAttempt = async () => {
  loadError.value = false;
  attemptId.value = route.params.attemptId as string;
  tenantId.value = route.params.tenantId as string;
  try {
    const result = await $api.tenant.quizTemplateQuizAttemptDetail(tenantId.value, attemptId.value);
    attempt.value = result.data;
    const templateResult = await $api.tenant.quizTemplateDetail(tenantId.value, attempt.value.quizTemplateId ?? "");
    template.value = templateResult.data;
    await fetchQuestions();
    loaded.value = true;
  } catch {
    loadError.value = true;
  }
};

onMounted(loadAttempt);

const fetchQuestions = async () => {
  const result = await $api.tenant.quizTemplateQuizAttemptQuestionList(
    tenantId.value,
    attemptId.value,
    { Cursor: undefined, Count: 100 },
  );
  questionRecords.value = result.data;

  // 从 answerRecords 同步选中状态
  syncSelectedAnswers();
};

// 从 answerRecords 同步选中状态
const syncSelectedAnswers = () => {
  questionRecords.value.forEach((question) => {
    const questionId = question.id || "";
    if (!questionId) return;

    if (question.answerRecords && question.answerRecords.length > 0) {
      // 提取所有答案值
      const answerValues = question.answerRecords
        .map((record) => record.value)
        .filter(
          (value): value is string => value !== undefined && value !== null,
        );

      if (answerValues.length > 0) {
        selectedAnswers.value[questionId] = answerValues;
      } else {
        selectedAnswers.value[questionId] = [];
      }
    } else {
      // 初始化空答案
      selectedAnswers.value[questionId] = [];
    }
  });
};

// 更新答案（由 InteractiveCard 触发）
const updateAnswer = async (
  questionId: string,
  answers: SetAnswerRecordCommand[],
) => {
  if (!questionId) return;

  try {
    await $api.tenant.quizTemplateQuizAttemptQuestionUpdate(
      tenantId.value,
      questionId,
      answers,
    );
  } catch (error) {
    console.error("更新答案失败:", error);
    // 回退到之前的答案
    // 从 question.answerRecords 恢复
    const question = questionRecords.value.find((q) => q.id === questionId);
    if (question?.answerRecords) {
      const restoredValues = question.answerRecords
        .map((record) => record.value)
        .filter(
          (value): value is string => value !== undefined && value !== null,
        );
      selectedAnswers.value[questionId] = restoredValues;
    } else {
      selectedAnswers.value[questionId] = [];
    }
    toast.add({
      title: "更新答案失败",
      color: "error",
    });
  }
};

// 交卷
const submitQuizAttempt = async () => {
  if (submitting.value) return;
  try {
    // 检查是否有未完成的题目
    const unansweredQuestions = questionRecords.value.filter((q) => {
      const answers = selectedAnswers.value[q.id || ""];
      return (
        !answers ||
        answers.length === 0 ||
        answers.every((a) => !a || !a.trim())
      );
    });

    const message = unansweredQuestions.length > 0
      ? `还有 ${unansweredQuestions.length} 道题未作答，确定交卷吗？`
      : "确定交卷吗？";
    if (!window.confirm(message)) return;

    submitting.value = true;
    await $api.tenant.quizTemplateQuizAttemptUpdate(
      tenantId.value,
      attemptId.value,
      {
        status: QuizAttemptStatus.Submitted,
      },
    );
    toast.add({ title: "交卷成功", color: "success" });
    router.push(`/tenant/${tenantId.value}/my-quiz-attempt`);
  } catch (error) {
    toast.add({ title: "交卷失败", color: "error" });
  } finally {
    submitting.value = false;
  }
};

const onCountdownEnd = async () => {
  toast.add({ title: "倒计时结束" });

  await $api.tenant.quizTemplateQuizAttemptUpdate(
    tenantId.value,
    attemptId.value,
    {
      status: QuizAttemptStatus.Submitted,
    },
  );
  toast.add({ title: "交卷成功", color: "success" });
  router.push(`/tenant/${tenantId.value}/my-quiz-attempt`);
};

const goToQuestion = (id: string) => {
  currentQuestionId.value = id;
  showAnswerSheet.value = false;
  if (showAllQuestion.value) {
    nextTick(() => document.getElementById(id)?.scrollIntoView({ behavior: "smooth", block: "center" }));
  }
};

// 跳转到上一题
const goToPreviousQuestion = () => {
  if (currentIndex.value > 0) {
    const prevQuestion = questionRecords.value[currentIndex.value - 1];
    if (prevQuestion?.id) {
      goToQuestion(prevQuestion.id);
    }
  }
};

// 跳转到下一题
const goToNextQuestion = () => {
  if (currentIndex.value < questionRecords.value.length - 1) {
    const nextQuestion = questionRecords.value[currentIndex.value + 1];
    if (nextQuestion?.id) {
      goToQuestion(nextQuestion.id);
    }
  }
};

// 计算已答题数
const getAnsweredCount = () => {
  return questionRecords.value.filter((q) => {
    const answers = selectedAnswers.value[q.id || ""];
    return answers && answers.length > 0 && answers.some((a) => a && a.trim());
  }).length;
};
</script>

<style scoped></style>
