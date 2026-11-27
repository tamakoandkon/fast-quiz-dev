<template>
  <UModal title="新增抽题规则" v-model:open="show">
    <UButton label="新增抽题规则" color="primary" variant="solid" />

    <template #body>
      <CommonFormContainer>
        <UFormField label="题型" class="w-full">
          <USelect
            :items="items"
            value-key="id"
            label="题型"
            v-model="questionType"
            class="w-full"
          />
        </UFormField>
        <UFormField label="题库" class="w-full">
          <USelect
            :items="groupOptions"
            value-key="id"
            label="题库"
            v-model="groupId"
            class="w-full"
          />
        </UFormField>
        <CommonFormNumberInput v-model="level" label="难度" />
        <CommonFormNumberInput
          v-model="count"
          label="数量"
          :min="0"
          :max="currentCount"
        />
        <CommonFormNumberInput v-model="score" label="分数" />

        <!-- 显示当前符合条件的题目数量 -->
        <div v-if="currentCount > 0" class="text-sm text-gray-500 px-1">
          当前共有 {{ currentCount }} 道符合条件的题目
        </div>
        <div
          v-else-if="groupId && questionType"
          class="text-sm text-yellow-600 px-1"
        >
          没有符合条件的题目
        </div>

        <UButton
          label="确认"
          color="primary"
          variant="subtle"
          @click="createQuestionSelectionRule"
          class="w-full justify-center"
        />
      </CommonFormContainer>
    </template>
  </UModal>
</template>
<script setup lang="ts">
const { $api } = useNuxtApp();

const props = defineProps<{
  tenantId: string;
  groups: QuestionGroupDto[];
}>();

const groupOptions = computed(() => props.groups.flatMap(group =>
  group.id ? [{ id: group.id, label: group.name || "未命名题库" }] : [],
));

const items = ref([
  {
    label: "单选",
    id: QuestionType.SingleChoice,
  },
  {
    label: "多选",
    id: QuestionType.MultipleChoice,
  },
  {
    label: "填空",
    id: QuestionType.FillIn,
  },
]);

const show = defineModel({ required: true, default: true });
const groupId = ref<string>("");
const questionType = ref<QuestionType>(QuestionType.SingleChoice);
const level = ref<number>(0);
const count = ref<number>(0);
const score = ref<number>(1);

const emit = defineEmits<{
  (e: "create", payLoad: CreateQuestionSelectionRuleCommand): void;
}>();

const createQuestionSelectionRule = function () {
  emit("create", {
    groupId: groupId.value,
    questionType: questionType.value,
    level: level.value,
    count: count.value,
    score: score.value,
  });
  groupId.value = "";
  questionType.value = QuestionType.SingleChoice;
  level.value = 0;
  count.value = 1;
  score.value = 1;
};

// 监听 groupId 变化
watch(groupId, () => {
  fetchQuestionCountAsync();
});

// 监听 questionType 变化
watch(questionType, () => {
  fetchQuestionCountAsync();
});

// 监听 level 变化
watch(level, () => {
  fetchQuestionCountAsync();
});

const currentCount = ref(0);
const fetchQuestionCountAsync = async () => {
  if (!groupId.value) {
    currentCount.value = 0;
    return;
  }
  const result = await $api.tenant.questionGroupCountList(
    props.tenantId,
    groupId.value,
    { Type: questionType.value, Level: level.value },
  );

  count.value = 0;
  currentCount.value = result.data;
};
</script>
