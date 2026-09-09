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

const groups = ref<QuestionGroupDto[]>([]);

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
  {
    label: "材料题",
    id: QuestionType.CaseStudy,
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

// 加载所有题库分组（不分页，加载全部）
const loadAllGroups = async () => {
  try {
    let allGroups: QuestionGroupDto[] = [];
    let cursor: string | undefined = undefined;
    let hasMore = true;

    // 循环加载所有分组，直到没有更多数据
    while (hasMore) {
      const result = await $api.tenant.questionGroupList(props.tenantId, {
        Cursor: cursor,
        Count: 200, // 每页加载50条
        IsPrevious: false,
        NamePrefix: undefined,
      });

      if (result.data && result.data.length > 0) {
        allGroups = [...allGroups, ...result.data];
        // 如果返回的数据少于请求的数量，说明没有更多数据了
        if (result.data.length < 50) {
          hasMore = false;
        } else {
          // 获取最后一个元素的ID作为下一页的游标
          cursor = result.data[result.data.length - 1]?.id ?? undefined;
        }
      } else {
        hasMore = false;
      }
    }

    groups.value = allGroups;
  } catch (error) {
    console.error("加载题库分组失败:", error);
    groups.value = [];
  }
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

// 组件挂载时加载所有分组
onMounted(async () => {
  await loadAllGroups();
  await fetchQuestionSelectionRuleAsync(props.id);
});

const fetchQuestionSelectionRuleAsync = async (id: string) => {
  if (!id || id.trim() === "") {
    return;
  }

  let result = await $api.tenant.quizTemplateRuleDetail(
    props.tenantId,
    props.id,
  );

  groupId.value = result.data.groupId ?? "";
  questionType.value = result.data.questionType ?? QuestionType.SingleChoice;
  level.value = result.data.level ?? 0;
  count.value = result.data.count ?? 1;
  score.value = result.data.score ?? 1;
};
</script>
