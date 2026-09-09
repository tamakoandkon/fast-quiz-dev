<template>
  <CommonPageLayout v-if="loaded">
    <div class="sticky top-0 z-10 bg-white">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex items-center h-18">
          <!-- 左侧：查看答题卡按钮 -->
          <div class="flex-shrink-0">
            <UButton
              @click="
                () => {
                  showAnswerSheet = !showAnswerSheet;
                }
              "
              :class="[
                'transition-all duration-200',
                showAnswerSheet
                  ? 'bg-blue-600 hover:bg-blue-700 text-white'
                  : 'bg-gray-100 hover:bg-gray-200 text-gray-700',
              ]"
              class="px-4 py-2 rounded-lg font-medium"
            >
              {{ showAnswerSheet ? "关闭答题卡" : "查看答题卡" }}
            </UButton>
          </div>

          <div class="flex-shrink-0 ml-4">
            <USwitch label="查看全部题目" v-model="showAllQuestion" />
          </div>

          <!-- 中间：倒计时 -->
          <div class="flex-1 flex justify-center">
            <CommonCountDown
              :minutes="template.durationMinutes ?? 0"
              :start-time="attempt.startTime ?? ''"
              @end="onCountdownEnd"
              class="text-lg font-semibold text-gray-800"
            />
          </div>

          <!-- 右侧占位保持对称 -->
          <div class="w-28 flex-shrink-0"></div>
        </div>
      </div>
      <!-- 分割线 -->
      <hr class="border-gray-200" />
    </div>

    <QuestionRecordAnswerSheetDrawer
      v-model="showAnswerSheet"
      @click="goToQuestion"
      :ids="ids"
    ></QuestionRecordAnswerSheetDrawer>

    <div class="max-w-6xl mx-auto">
      <div class="bg-white rounded-lg shadow p-6 mb-6">
        <h1 class="text-3xl font-bold text-gray-800 text-center">
          {{ template.name }}
        </h1>
      </div>

      <!-- 全题视图 -->
      <div v-if="showAllQuestion" class="space-y-4">
        <QuestionRecordAnswerCard
          v-for="(question, questionIndex) in questionRecords"
          :key="question.id || questionIndex"
          :question="question"
          :index="questionIndex + 1"
          v-model="selectedAnswers[question.id!]"
          @answer="updateAnswer"
          :id="question.id"
        />

        <!-- 空状态 -->
        <div
          v-if="questionRecords.length === 0"
          class="bg-white rounded-lg shadow p-12 text-center text-gray-500"
        >
          暂无题目
        </div>
      </div>

      <!-- 单题视图 -->
      <div v-else>
        <div v-if="currentQuestion" class="space-y-4">
          <!-- 进度信息 -->
          <div
            class="flex justify-between items-center bg-white rounded-lg shadow p-4"
          >
            <span class="text-sm text-gray-600">
              第 {{ currentIndex + 1 }} / {{ questionRecords.length }} 题
            </span>
            <span class="text-sm text-gray-500">
              {{ getAnsweredCount() }} / {{ questionRecords.length }} 已答
            </span>
          </div>

          <!-- 当前题目 -->
          <QuestionRecordAnswerCard
            :question="currentQuestion"
            :index="currentIndex + 1"
            v-model="selectedAnswers[currentQuestion.id!]"
            @answer="updateAnswer"
            :id="currentQuestion.id"
          />

          <!-- 导航按钮 -->
          <div
            class="flex justify-between items-center bg-white rounded-lg shadow p-4"
          >
            <UButton
              @click="goToPreviousQuestion"
              :disabled="currentIndex === 0"
              color="info"
              size="lg"
            >
              <Icon name="i-heroicons-chevron-left" class="mr-1" />
              上一题
            </UButton>

            <UButton
              @click="goToNextQuestion"
              :disabled="currentIndex === questionRecords.length - 1"
              color="info"
              size="lg"
            >
              下一题
              <Icon name="i-heroicons-chevron-right" class="ml-1" />
            </UButton>
          </div>
        </div>

        <!-- 空状态 -->
        <div
          v-if="questionRecords.length === 0"
          class="bg-white rounded-lg shadow p-12 text-center text-gray-500"
        >
          暂无题目
        </div>
      </div>
    </div>

    <div class="flex justify-center my-4">
      <UButton
        @click="submitQuizAttempt"
        size="xl"
        class="text-lg px-8 py-3"
        id="submit-btn"
      >
        交卷
      </UButton>
    </div>
  </CommonPageLayout>
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

onMounted(async () => {
  attemptId.value = route.params.attemptId as string;
  tenantId.value = route.params.tenantId as string;

  const result = await $api.tenant.quizTemplateQuizAttemptDetail(
    tenantId.value,
    attemptId.value,
  );
  attempt.value = result.data;

  const templateResult = await $api.tenant.quizTemplateDetail(
    tenantId.value,
    attempt.value.quizTemplateId ?? "",
  );
  template.value = templateResult.data;

  await fetchQuestions();

  loaded.value = true;
});

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

    if (unansweredQuestions.length > 0) {
      const confirmSubmit = await new Promise((resolve) => {
        // 使用 Nuxt UI 的弹窗或 confirm
        if (
          window.confirm(
            `还有 ${unansweredQuestions.length} 道题未作答，确定交卷吗？`,
          )
        ) {
          resolve(true);
        } else {
          resolve(false);
        }
      });

      if (!confirmSubmit) {
        return;
      }
    }

    await $api.tenant.quizTemplateQuizAttemptUpdate(
      tenantId.value,
      attemptId.value,
      {
        status: "Submitted",
      },
    );
    toast.add({ title: "交卷成功", color: "success" });
    router.push(`/tenant/${tenantId.value}/my-quiz-attempt`);
  } catch (error) {
    toast.add({ title: "交卷失败", color: "error" });
  }
};

const onCountdownEnd = async () => {
  toast.add({ title: "倒计时结束" });

  await $api.tenant.quizTemplateQuizAttemptUpdate(
    tenantId.value,
    attemptId.value,
    {
      status: "Submitted",
    },
  );
  toast.add({ title: "交卷成功", color: "success" });
  router.push(`/tenant/${tenantId.value}/my-quiz-attempt`);
};

const goToQuestion = (id: string) => {
  // router.push({ hash: `#${id}` });
  currentQuestionId.value = id;
  window.location.hash = `#${id}`;
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
