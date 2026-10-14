<template>
  <UModal title="修改抽题规则" v-model:open="show">
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
            label="name"
            v-model="groupId"
            class="w-full"
          />
        </UFormField>
        <CommonFormNumberInput v-model="level" label="难度" />
        <CommonFormNumberInput v-model="count" label="数量" />
        <CommonFormNumberInput v-model="score" label="分数" />

        <UButton
          label="确认"
          color="primary"
          variant="subtle"
          @click="updateQuestionSelectionRule"
          class="w-full justify-center"
        />
      </CommonFormContainer>
    </template>
  </UModal>
</template>

<script setup lang="ts">
const { $api } = useNuxtApp();

const props = defineProps({
  id: {
    type: String,
    default: "",
  },
  tenantId: {
    type: String,
    default: "",
  },
});



// 计算属性：将groups转换为USelect需要的格式
const groupOptions = computed(() => {
  return groups.value.map((group) => ({
    id: group.id,
    label: group.name ?? "未知题库", // 确保这里使用正确的字段名，可能是group.name或group.title等
  }));
});

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
const count = ref<number>(1);
const score = ref<number>(1);

const { groups } = useQuestionGroups(() => props.tenantId, () => !!show.value);

const emit = defineEmits<{
  (e: "update", payLoad: UpdateQuestionSelectionRuleCommand): void;
}>();

const updateQuestionSelectionRule = function () {
  emit("update", {
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

watch([() => props.tenantId, () => props.id, show], async ([tenant, id, open], _, onCleanup) => {
  let current = true;
  onCleanup(() => { current = false; });
  groupId.value = '';
  if (!open || !tenant?.trim() || !id?.trim()) return;
  try {
    const result = await $api.tenant.quizTemplateRuleDetail(tenant, id);
    if (!current) return;
    groupId.value = result.data.groupId ?? '';
    questionType.value = result.data.questionType ?? QuestionType.SingleChoice;
    level.value = result.data.level ?? 0;
    count.value = result.data.count ?? 1;
    score.value = result.data.score ?? 1;
  } catch (error) {
    if (current) console.error('加载抽题规则失败:', error);
  }
}, { immediate: true });
</script>
