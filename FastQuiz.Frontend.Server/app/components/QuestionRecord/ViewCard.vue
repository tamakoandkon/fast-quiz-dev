<template>
  <UCard class="hover:shadow-md transition-shadow duration-200">
    <div class="flex items-start gap-4">
      <span
        class="flex-shrink-0 w-8 h-8 bg-blue-100 text-blue-700 rounded-full flex items-center justify-center font-semibold text-sm"
      >
        {{ index }}
      </span>

      <div class="flex-1">
        <!-- 题目内容 -->
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

        <template v-if="question.type === QuestionType.FillIn">
          <div
            v-if="question.answers && question.answers.length"
            class="mt-3 space-y-2"
          >
            <div
              v-for="(answer, aIndex) in question.answers"
              :key="answer.id || aIndex"
              class="flex items-start gap-2 p-2 rounded border border-gray-200"
            >
              <div class="flex-shrink-0 w-6 text-center">
                <span class="text-xs font-medium text-gray-500">
                  {{ aIndex + 1 }}.
                </span>
              </div>
              <CommonSafeMarkup
                class="flex-1 text-sm text-gray-700"
                :content="answer.markUpContent || '答案内容暂无'"
               />
              <div class="flex-shrink-0">
                <UBadge
                  v-if="answer.matchStrategy"
                  color="secondary"
                  size="sm"
                  variant="subtle"
                >
                  {{ getMatchStrategy(answer.matchStrategy) }}
                </UBadge>
              </div>
            </div>
          </div>
          <div v-else class="text-xs text-gray-400 italic mt-2">无答案</div>
        </template>

        <!-- 选择题（单选/多选） -->
        <template v-else>
          <div
            v-if="question.answers && question.answers.length"
            class="mt-3 space-y-2"
          >
            <div
              v-for="(answer, aIndex) in question.answers"
              :key="answer.id || aIndex"
              class="flex items-start gap-2 p-2 rounded transition-colors border border-gray-200"
              :class="{
                'bg-green-50 ring-1 ring-green-300': answer.isCorrect,
                'bg-gray-50': !answer.isCorrect,
              }"
            >
              <div class="flex-shrink-0 w-6 text-center">
                <span class="text-xs font-medium text-gray-500">
                  {{ String.fromCharCode(65 + aIndex) }}.
                </span>
              </div>
              <CommonSafeMarkup
                class="flex-1 text-sm text-gray-700"
                :content="answer.markUpContent || '选项内容暂无'"
               />
              <div class="flex-shrink-0 flex gap-1 flex-wrap items-center">
                <UBadge
                  v-if="answer.isCorrect"
                  color="success"
                  size="sm"
                  variant="subtle"
                >
                  正确答案
                </UBadge>
              </div>
            </div>
          </div>
          <div v-else class="text-xs text-gray-400 italic mt-2">无选项</div>
        </template>

        <!-- 难度等级 -->
        <div v-if="question.level" class="text-xs text-orange-400 mt-3">
          难度等级：{{ question.level }}
        </div>
      </div>
    </div>
  </UCard>
</template>

<script setup lang="ts">
const props = defineProps<{
  question: QuestionSnapshotDto;
  index: number;
}>();
</script>
