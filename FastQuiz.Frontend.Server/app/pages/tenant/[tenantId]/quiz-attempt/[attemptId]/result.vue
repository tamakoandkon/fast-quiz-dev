<template>
  <CommonPageLayout :title="template.name || '考试结果'" :description="template.name ? '考试结果' : undefined">
    <template #actions>
      <UButton
        v-if="ids.length"
        color="neutral"
        variant="outline"
        icon="i-lucide-list-checks"
        @click="showAnswerSheet = true"
      >查看答题卡</UButton>
    </template>
    <QuestionRecordAnswerSheetDrawer
      v-model="showAnswerSheet"
      :ids="ids"
      :answered-ids="answeredIds"
      @click="showQuestion"
    />
    <div class="space-y-4">
      <div v-if="attempt.score != null || attempt.status === QuizAttemptStatus.Submitted" class="workspace-panel workspace-toolbar">
        <p v-if="attempt.score != null" class="text-sm font-semibold">成绩 {{ attempt.score }} 分</p>
        <p v-else class="text-sm text-muted">已交卷，正在阅卷</p>
      </div>
      <div v-if="loading" class="workspace-panel p-8 text-center text-gray-600" role="status">
        正在加载答卷…
      </div>
      <div v-else-if="loadError" class="workspace-panel p-8 text-center">
        <p class="text-gray-700" role="alert">{{ loadError }}</p>
        <UButton class="mt-4" color="neutral" variant="outline" @click="loadResult">重新加载</UButton>
      </div>
      <div v-else class="space-y-4">
        <p v-if="answerNotice" class="workspace-panel px-4 py-3 text-sm text-gray-700" role="status">
          {{ answerNotice }}
        </p>
        <QuestionRecordResultCard
          v-for="(question, questionIndex) in questionRecords"
          :id="question.id ? 'result-question-' + question.id : undefined"
          :key="question.id || questionIndex"
          :question="question"
          :index="questionIndex + 1"
          :tenant-id="tenantId"
        />
        <div v-if="!questionRecords.length" class="workspace-panel p-8 text-center text-gray-600">
          这份答卷没有题目记录。
        </div>
      </div>
    </div>
  </CommonPageLayout>
</template>

<script setup lang="ts">
import { isAxiosError } from "axios";

const { $api } = useNuxtApp();
const route = useRoute();
const tenantId = route.params.tenantId as string;
const attemptId = route.params.attemptId as string;
const attempt = ref<QuizAttempDto>({});
const template = ref<QuizTemplateDto>({});
const questionRecords = ref<QuestionRecordWithAnswerDto[]>([]);
const loading = ref(true);
const loadError = ref("");
const answerNotice = ref("");
const showAnswerSheet = ref(false);

const ids = computed(() =>
  questionRecords.value.flatMap(record => record.id ? [record.id] : []),
);
const answeredIds = computed(() =>
  questionRecords.value.flatMap(record => record.id && record.answerRecords?.length ? [record.id] : []),
);

function showQuestion(id: string) {
  showAnswerSheet.value = false;
  document.getElementById("result-question-" + id)?.scrollIntoView({ behavior: "smooth", block: "center" });
}

async function readQuestions(includeAnswerKey: boolean) {
  const read = includeAnswerKey
    ? $api.tenant.quizTemplateQuizAttemptQuestionWithAnswerList
    : $api.tenant.quizTemplateQuizAttemptQuestionList;
  const records: QuestionRecordWithAnswerDto[] = [];
  const pageSize = 100;
  let cursor: string | undefined;
  while (true) {
    const page = (await read(tenantId, attemptId, { Cursor: cursor, Count: pageSize })).data;
    records.push(...page);
    if (page.length < pageSize) return records;
    const next = page.at(-1)?.id;
    if (!next || cursor === next) throw new Error("无法加载后续题目");
    cursor = next;
  }
}

async function loadResult() {
  loading.value = true;
  loadError.value = "";
  answerNotice.value = "";
  showAnswerSheet.value = false;
  questionRecords.value = [];
  try {
    attempt.value = (await $api.tenant.quizTemplateQuizAttemptDetail(tenantId, attemptId)).data;
    if (attempt.value.quizTemplateId) {
      template.value = (await $api.tenant.quizTemplateDetail(tenantId, attempt.value.quizTemplateId)).data;
    }
    try {
      questionRecords.value = await readQuestions(true);
    } catch (error) {
      const response = isAxiosError<{ detail?: string }>(error) ? error.response : undefined;
      if (response?.status === 401) throw error;
      questionRecords.value = await readQuestions(false);
      const reason = response?.data?.detail;
      if (reason === "仍有剩余考试次数时不可查看标准答案" || reason === "考试结束前不可查看标准答案") {
        answerNotice.value = reason + "。你可以查看自己的作答。";
      } else if (attempt.value.status === QuizAttemptStatus.Submitted) {
        answerNotice.value = "阅卷期间可以先查看自己的作答。";
      } else {
        answerNotice.value = "标准答案暂不可查看。你可以查看自己的作答。";
      }
    }
  } catch {
    loadError.value = "暂时无法加载答卷，请重试。";
  } finally {
    loading.value = false;
  }
}

onMounted(loadResult);
</script>
