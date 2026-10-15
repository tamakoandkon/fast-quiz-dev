<!-- components/quiz/InteractiveCard.vue -->
<template>
  <UCard class="hover:shadow-md transition-shadow duration-200">
    <div class="flex items-start gap-4">
      <!-- 题号 - 圆形背景 -->
      <span
        class="flex-shrink-0 w-8 h-8 bg-blue-100 text-blue-700 rounded-full flex items-center justify-center font-semibold text-sm"
      >
        {{ index }}
      </span>

      <div class="flex-1">
        <div class="text-sm text-gray-700 leading-relaxed">
          <div class="flex items-start gap-2">
            <div class="flex-1">
              <CommonSafeMarkup
                v-if="question.markUpContent"
                :content="question.markUpContent"
               />
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

        <!-- 根据题型渲染不同内容 -->
        <!-- 填空题 -->
        <template v-if="question.type === QuestionType.FillIn">
          <div class="ml-4">
            <UInput
              :model-value="modelValue[0] || ''"
              @update:model-value="handleFillInput"
              placeholder="请输入答案..."
              class="max-w-md"
            />
            <div
              v-if="!modelValue[0]?.trim()"
              class="text-xs text-gray-400 mt-1"
            >
              请输入你的答案
            </div>
          </div>
        </template>

        <!-- 选择题（单选/多选） -->
        <template v-else>
          <div
            v-if="question.answers && question.answers.length > 0"
            class="ml-4 space-y-2"
          >
            <div
              v-for="(answer, answerIndex) in question.answers"
              :key="answer.id || answerIndex"
              @click="toggleAnswer(answer.id || '')"
              class="flex items-start gap-2 text-sm p-2 rounded-lg cursor-pointer transition-colors"
              :class="[
                isSelected(answer.id || '')
                  ? 'bg-blue-50 ring-1 ring-blue-300'
                  : 'hover:bg-gray-50 ring-1 ring-transparent',
              ]"
            >
              <span class="text-gray-400 flex-shrink-0">
                {{ String.fromCharCode(65 + answerIndex) }}.
              </span>
              <CommonSafeMarkup
                class="text-gray-700"
                :content="answer.markUpContent || '无内容'"
               />

              <span
                v-if="isSelected(answer.id || '')"
                class="ml-auto text-blue-600 flex-shrink-0"
              >
                ✓
              </span>
            </div>
          </div>
          <div v-else class="ml-4 text-sm text-gray-400">暂无答案选项</div>
        </template>
      </div>
    </div>
  </UCard>
</template>

<script setup lang="ts">
const props = withDefaults(
  defineProps<{
    question: QuestionRecordWithoutAnswerDto;
    index: number;
    modelValue?: string[]; // 统一为 string[]，未选择时默认空数组
  }>(),
  { modelValue: () => [] },
);

const emit = defineEmits<{
  (e: "update:modelValue", value: string[]): void;
  (e: "answer", questionId: string, data: { value: string }[]): void;
}>();

// 判断某选项是否被选中
const isSelected = (answerId: string) => {
  return props.modelValue.includes(answerId);
};

// 切换选项（单选/多选共用）
const toggleAnswer = (answerId: string) => {
  if (!answerId) return;

  const current = props.modelValue;
  let newValue: string[];

  if (props.question.type === QuestionType.SingleChoice) {
    // 单选：如果已选中则取消，否则替换
    newValue = current.includes(answerId) ? [] : [answerId];
  } else {
    // 多选（默认）：切换
    newValue = current.includes(answerId)
      ? current.filter((id) => id !== answerId)
      : [...current, answerId];
  }

  emit("update:modelValue", newValue);

  // 提交答案
  submitAnswer(newValue);
};

// 处理填空题输入
const handleFillInput = (value: string | number) => {
  const strValue = String(value);
  const newValue = [strValue];
  emit("update:modelValue", newValue);
  submitAnswer(newValue);
};

// 提交答案的通用方法
const submitAnswer = (value: string[]) => {
  // 过滤掉空值
  const validValues = value.filter((v) => v && v.trim() !== "");
  if (validValues.length === 0) {
    // 如果答案为 null 或空，也提交，表示清空答案
    emit("answer", props.question.id || "", []);
    return;
  }

  const commands = validValues.map((v) => ({ value: v }));
  emit("answer", props.question.id || "", commands);
};

// 未提供作答数组时，由 prop 默认值提供空数组。
</script>
