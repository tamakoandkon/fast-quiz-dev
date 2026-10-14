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


const props = defineProps({
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
const count = ref<number>(0);
const score = ref<number>(1);

const { groups } = useQuestionGroups(() => props.tenantId, () => !!show.value);
watch(() => props.tenantId, () => { groupId.value = ''; });

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

const currentCount = ref(0);
watch([() => props.tenantId, show, groupId, questionType, level], async ([tenant, open, group, type, difficulty], _, onCleanup) => {
  let current = true;
  onCleanup(() => { current = false; });
  currentCount.value = 0;
  count.value = 0;
  if (!open || !tenant?.trim() || !group) return;
  try {
    const result = await $api.tenant.questionGroupCountList(tenant, group, { Type: type, Level: difficulty });
    if (current) currentCount.value = result.data ?? 0;
  } catch (error) {
    if (current) console.error('加载题目数量失败:', error);
  }
});
</script>
