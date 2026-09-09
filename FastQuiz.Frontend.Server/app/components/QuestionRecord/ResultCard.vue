<!-- components/quiz/ResultCard.vue -->
<template>
  <UCard
    class="hover:shadow-md transition-shadow duration-200"
    :class="{
      'border-green-500 border-2 bg-green-50': question.isCorrect === true,
      'border-red-500 border-2 bg-red-50': question.isCorrect === false,
    }"
  >
    <div class="flex items-start gap-4">
      <div class="flex flex-col items-center gap-2 flex-shrink-0">
        <span
          class="w-8 h-8 bg-blue-100 text-blue-700 rounded-full flex items-center justify-center font-semibold text-sm"
        >
          {{ index }}
        </span>
      </div>

      <div class="flex-1">
        <!-- 题目内容 -->
        <div class="text-sm text-gray-700 leading-relaxed">
          <div class="flex items-start gap-2">
            <div class="flex-1">
              <div
                v-if="question.markUpContent"
                v-html="question.markUpContent"
              ></div>
              <div v-else class="text-gray-400 italic">题目内容暂无</div>
            </div>
            <UBadge
              v-if="question.type"
              color="info"
              size="sm"
              variant="subtle"
              class="flex-shrink-0 mt-0.5"
            >
              {{ getQuestionTypeName(question.type) }}
            </UBadge>
          </div>
        </div>

        <!-- 填空题 -->
        <template v-if="question.type === QuestionType.FillIn">
          <div
            class="mt-3 space-y-2 p-3 bg-gray-50 rounded-lg border border-gray-200"
          >
            <div class="flex items-center gap-2 text-sm">
              <span class="font-medium text-gray-600">你的答案：</span>
              <span
                :class="{
                  'text-green-600': question.isCorrect === true,
                  'text-red-600': question.isCorrect === false,
                  'text-gray-500':
                    question.isCorrect === undefined ||
                    question.isCorrect === null,
                }"
              >
                {{ getUserFillAnswer() || "未作答" }}
              </span>
            </div>

            <!-- 循环展示所有正确答案及其匹配策略 -->
            <div
              v-if="question.answers && question.answers.length > 0"
              class="space-y-1"
            >
              <div
                v-for="(answer, idx) in question.answers"
                :key="answer.id || idx"
                class="flex items-center gap-2 text-sm"
              >
                <span class="font-medium text-gray-600"> 正确答案：</span>
                <span class="text-green-600 flex-1">
                  {{ answer.markUpContent || "未设置" }}
                </span>
                <UBadge
                  v-if="answer.matchStrategy"
                  color="secondary"
                  size="sm"
                  variant="subtle"
                >
                  {{ getMatchStrategyLabel(answer.matchStrategy) }}
                </UBadge>
              </div>
            </div>
          </div>
        </template>

        <!-- 选择题（单选/多选） - 只读查看模式 -->
        <template v-else>
          <div
            v-if="question.answers && question.answers.length > 0"
            class="mt-3 space-y-2"
          >
            <div
              v-for="(answer, answerIndex) in question.answers"
              :key="answer.id || answerIndex"
              class="flex items-start gap-2 text-sm p-2 rounded-lg border"
              :class="getAnswerClass(question, answer, answerIndex)"
            >
              <span class="text-gray-400 flex-shrink-0">
                {{ String.fromCharCode(65 + answerIndex) }}.
              </span>
              <span
                class="text-gray-700 flex-1"
                v-html="answer.markUpContent || '无内容'"
              ></span>
              <!-- 标签显示：正确答案 + 你的答案 -->
              <div class="flex-shrink-0 flex gap-2 flex-wrap items-center">
                <!-- 正确答案标签 -->
                <span
                  v-if="isCorrectAnswer(answer)"
                  class="inline-flex items-center font-semibold text-green-600"
                >
                  <UIcon name="iconoir:check" class="size-5"></UIcon>正确答案
                </span>
                <!-- 你的答案标签 -->
                <span
                  v-if="isUserSelected(answer.id as string)"
                  class="font-semibold inline-flex items-center"
                  :class="
                    isCorrectAnswer(answer) ? 'text-green-600' : 'text-red-600'
                  "
                >
                  <UIcon
                    v-if="isCorrectAnswer(answer)"
                    name="iconoir:check"
                    class="size-5"
                    color="success"
                  ></UIcon>
                  <UIcon
                    v-else
                    color="error"
                    name="iconoir:xmark"
                    class="size-5"
                  ></UIcon
                  >(你的答案)
                </span>
              </div>
            </div>
          </div>
          <div v-else class="text-sm text-gray-400 italic mt-2">无答案</div>
        </template>

        <!-- 难度等级 -->
        <div class="text-xs text-orange-400 mt-3">
          难度等级：{{ question.level }}
        </div>
      </div>
    </div>
  </UCard>
</template>

<script setup lang="ts">
const props = defineProps<{
  question: QuestionRecordWithAnswerDto;
  index: number;
  tenantId: string;
}>();

const getMatchStrategyLabel = (strategy: MatchStrategy) => {
  const map: Record<string, string> = {
    [MatchStrategy.Exact]: "精确匹配",
    [MatchStrategy.Contains]: "包含匹配",
    [MatchStrategy.Manual]: "手动匹配",
    [MatchStrategy.Regex]: "正则匹配",
  };
  return map[strategy] || strategy;
};

// 获取用户选中的答案 ID 列表
const getUserSelectedIds = (): string[] => {
  if (!props.question.answerRecords) return [];
  return props.question.answerRecords
    .map((record) => record.value)
    .filter((value): value is string => value !== undefined && value !== null);
};

// 判断用户是否选择了这个答案
const isUserSelected = (answerId?: string): boolean => {
  if (!answerId) return false;
  const selectedIds = getUserSelectedIds();
  return selectedIds.includes(answerId);
};

// 判断这个答案是否正确
const isCorrectAnswer = (answer: any): boolean => {
  return answer.isCorrect === true;
};

// 填空题相关 - 获取用户答案
const getUserFillAnswer = (): string => {
  if (props.question.type !== QuestionType.FillIn) return "";

  if (props.question.answerRecords && props.question.answerRecords.length > 0) {
    const values = props.question.answerRecords
      .map((record) => record.value)
      .filter(
        (value): value is string =>
          value !== undefined && value !== null && value.trim() !== "",
      );

    return values.join("、");
  }

  return "";
};

// 获取答案的样式类
const getAnswerClass = (
  question: any,
  answer: any,
  answerIndex: number,
): string => {
  const selected = isUserSelected(answer.id);
  const correct = isCorrectAnswer(answer);
  const isMultiple = question.type === "multiple";

  if (correct) {
    if (isMultiple && !selected) {
      return "border-green-300 bg-green-50/50";
    }
    return "border-green-300 bg-green-50";
  }

  if (selected && !correct) {
    return "border-red-300 bg-red-50";
  }

  if (isMultiple && !selected && !correct) {
    return "border-gray-200 bg-gray-50";
  }

  return "border-gray-200 bg-white";
};
</script>
