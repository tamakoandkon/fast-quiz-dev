<template>
  <article class="answer-card">
    <div class="answer-card-heading">
      <span class="answer-number">{{ index }}</span>
      <span v-if="question.type" class="answer-type">{{ getQuestionTypeName(question.type) }}</span>
    </div>
    <div class="answer-prompt">
      <CommonSafeMarkup v-if="question.markUpContent" :content="question.markUpContent" />
      <span v-else>题目内容暂无</span>
    </div>
    <div v-if="question.type === QuestionType.FillIn" class="answer-options">
      <UInput
        :model-value="modelValue[0] || ''"
        :aria-label="`第 ${index} 题答案`"
        @update:model-value="handleFillInput"
        placeholder="输入答案"
        class="answer-fill"
      />
    </div>
    <div v-else-if="question.answers && question.answers.length > 0" class="answer-options" :role="question.type === QuestionType.SingleChoice ? 'radiogroup' : 'group'" :aria-label="`第 ${index} 题选项`">
      <button
        v-for="(answer, answerIndex) in question.answers"
        :key="answer.id || answerIndex"
        type="button"
        class="answer-option"
        :class="{ 'is-selected': isSelected(answer.id || '') }"
        :role="question.type === QuestionType.SingleChoice ? 'radio' : 'checkbox'"
        :aria-checked="isSelected(answer.id || '')"
        @click="toggleAnswer(answer.id || '')"
      >
        <span class="answer-letter">{{ String.fromCharCode(65 + answerIndex) }}</span>
        <CommonSafeMarkup :content="answer.markUpContent || '无内容'" />
        <Icon v-if="isSelected(answer.id || '')" name="i-heroicons-check" class="answer-check" aria-hidden="true" />
      </button>
    </div>
    <div v-else class="answer-options">暂无答案选项</div>
  </article>
</template>

<script setup lang="ts">
const props = withDefaults(
  defineProps<{
    question: QuestionRecordWithoutAnswerDto;
    index: number;
    modelValue?: string[];
  }>(),
  { modelValue: () => [] },
);

const emit = defineEmits<{
  (e: "update:modelValue", value: string[]): void;
  (e: "answer", questionId: string, data: { value: string }[]): void;
}>();

const isSelected = (answerId: string) => {
  return props.modelValue.includes(answerId);
};

const toggleAnswer = (answerId: string) => {
  if (!answerId) return;

  const current = props.modelValue;
  let newValue: string[];

  if (props.question.type === QuestionType.SingleChoice) {
    newValue = current.includes(answerId) ? [] : [answerId];
  } else {
    newValue = current.includes(answerId)
      ? current.filter((id) => id !== answerId)
      : [...current, answerId];
  }

  emit("update:modelValue", newValue);

  submitAnswer(newValue);
};

const handleFillInput = (value: string | number) => {
  const strValue = String(value);
  const newValue = [strValue];
  emit("update:modelValue", newValue);
  submitAnswer(newValue);
};

const submitAnswer = (value: string[]) => {
  const validValues = value.filter((v) => v && v.trim() !== "");
  if (validValues.length === 0) {
    emit("answer", props.question.id || "", []);
    return;
  }

  const commands = validValues.map((v) => ({ value: v }));
  emit("answer", props.question.id || "", commands);
};

</script>
