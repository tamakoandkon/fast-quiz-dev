<template>
  <CommonPageLayout>
    <div class="max-w-6xl mx-auto">
      <div class="bg-white rounded-lg shadow p-6 mb-6">
        <h1 class="text-3xl font-bold text-gray-800 text-center">
          {{ template.name }}
        </h1>
      </div>

      <UButton @click="completedReview">完成阅卷</UButton>

      <!-- 题目列表 -->
      <div class="space-y-4">
        <div v-for="(question, questionIndex) in questionRecords">
          <QuestionRecordResultCard
            :key="question.id || questionIndex"
            :question="question"
            :index="questionIndex + 1"
            :tenant-id="tenantId"
          />
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
  </CommonPageLayout>
</template>

<script setup lang="ts">
const toast = useToast();

const { $api } = useNuxtApp();
const route = useRoute();

const attemptId = ref("");
const attempt = ref<QuizAttempDto>({});
const questionRecords = ref<QuestionRecordWithAnswerDto[]>([]);
const template = ref<QuizTemplateDto>({});

const tenantId = ref("");

onMounted(async () => {
  tenantId.value = route.params.tenantId as string;
  attemptId.value = route.params.attemptId as string;

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
});

const fetchQuestions = async () => {
  const result =
    await $api.tenant.quizTemplateQuizAttemptQuestionWithAnswerList(
      tenantId.value,
      attemptId.value,
      { Cursor: undefined, Count: 100 },
    );
  questionRecords.value = result.data;
};

const completedReview = async () => {
  await $api.tenant.quizTemplateQuizAttemptUpdate(
    tenantId.value,
    attemptId.value,
    {
      status: QuizAttemptStatus.Graded,
    },
  );
};

// 👇 添加这个方法
const handleReview = (payload: { questionId: string; isCorrect: boolean }) => {
  console.log("📨 父组件收到事件", payload.questionId, payload.isCorrect);
  setCorrectStatusAsync(payload.questionId, payload.isCorrect);
};

const setCorrectStatusAsync = async (recordId: string, isCorrect: boolean) => {
  await $api.tenant.quizTemplateQuizAttemptQuestionStatusUpdate(
    tenantId.value,
    recordId,
    isCorrect,
  );

  // 更新本地数据
  const question = questionRecords.value.find((q) => q.id === recordId);
  if (question) {
    question.isCorrect = isCorrect;
  }

  toast.add({ title: "成功阅卷" });
};
</script>

<style scoped></style>
