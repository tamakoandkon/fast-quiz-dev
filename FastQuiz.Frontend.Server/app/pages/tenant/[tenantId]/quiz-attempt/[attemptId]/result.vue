<template>
  <CommonPageLayout>
    <QuestionRecordAnswerSheetDrawer
      v-model="showAnswerSheet"
      :ids="ids"
    ></QuestionRecordAnswerSheetDrawer>
    <UButton
      @click="
        () => {
          showAnswerSheet = !showAnswerSheet;
        }
      "
      >查看答题卡</UButton
    >
    <div class="max-w-6xl mx-auto">
      <div class="bg-white rounded-lg shadow p-6 mb-6">
        <h1 class="text-3xl font-bold text-gray-800 text-center">
          {{ template.name }}
        </h1>
      </div>

      <!-- 题目列表 -->
      <div class="space-y-4">
        <QuestionRecordResultCard
          v-for="(question, questionIndex) in questionRecords"
          :key="question.id || questionIndex"
          :question="question"
          :index="questionIndex + 1"
          :tenant-id="tenantId"
        />

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
const { $api } = useNuxtApp();
const route = useRoute();

const attemptId = ref("");
const attempt = ref<QuizAttempDto>({});
const questionRecords = ref<QuestionRecordWithAnswerDto[]>([]);
const template = ref<QuizTemplateDto>({});

const ids = computed<string[]>(() => {
  return questionRecords.value
    .map((item) => item.id)
    .filter((id): id is string => id !== null && id !== undefined && id !== "");
});

const showAnswerSheet = ref(false);

const tenantId = ref("");

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
</script>

<style scoped></style>
