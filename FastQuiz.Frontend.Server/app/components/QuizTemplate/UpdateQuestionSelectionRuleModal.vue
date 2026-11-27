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

const props = defineProps<{
  id: string;
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
const count = ref<number>(1);
const score = ref<number>(1);

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

// 监听props.id变化，加载规则详情
watch(
  () => props.id,
  async (newId) => {
    if (newId) {
      await fetchQuestionSelectionRuleAsync(newId);
    }
  },
  { immediate: true },
);

const fetchQuestionSelectionRuleAsync = async (id: string) => {
  if (!id || id.trim() === "") {
    return;
  }

  let result = await $api.tenant.quizTemplateRuleDetail(
    props.tenantId,
    id,
  );

  groupId.value = result.data.groupId ?? "";
  questionType.value = result.data.questionType ?? QuestionType.SingleChoice;
  level.value = result.data.level ?? 0;
  count.value = result.data.count ?? 1;
  score.value = result.data.score ?? 1;
};
</script>
