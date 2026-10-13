<template>
  <div>
    <UModal title="修改题目" v-model:open="show">
      <template #body>
        <CommonFormContainer>
          <UFormField label="题干" class="w-full">
            <CommonMarkUpEditor v-model="content" placeholder="请输入题干..." />
          </UFormField>

          <UFormField label="难度" class="w-full">
            <UInput
              type="number"
              v-model="level"
              class="w-full"
              placeholder="请输入数字0-100"
            />
          </UFormField>
          <CommonFormInput label="备注" v-model="remark" />
        </CommonFormContainer>

        <!-- 答案设置部分 -->
        <div class="my-4 border border-gray-200 rounded-lg p-3">
          <div
            class="flex justify-between items-center mb-3 pb-2 border-b border-gray-100"
          >
            <span class="font-medium text-sm">答案设置</span>
            <UButton
              label="添加答案"
              color="neutral"
              variant="subtle"
              size="sm"
              @click="addAnswer"
            />
          </div>

          <div
            v-for="(answer, index) in answers"
            :key="index"
            class="mb-3 p-2 bg-gray-50 rounded-md"
          >
            <div class="flex justify-between items-center gap-2">
              <UFormField label="是否正确" class="w-full">
                <USwitch v-model="answer.isCorrect" />
              </UFormField>

              <CommonFormInput label="答案内容" v-model="answer.content">
              </CommonFormInput>

              <UFormField label="排序" class="w-full">
                <UInput
                  type="number"
                  v-model="answer.sortOrder"
                  class="w-full"
                />
              </UFormField>
              <UFormField
                label="验证方式"
                class="w-full"
                v-if="type == QuestionType.FillIn"
              >
                <USelect
                  :items="strategies"
                  value-key="id"
                  v-model="answer.matchStrategy"
                  class="w-full"
                />
              </UFormField>
              <!-- 删除按钮应该为红色 -->
              <UButton
                label="删除"
                color="neutral"
                variant="subtle"
                size="sm"
                @click="removeAnswer(index)"
              />
            </div>
          </div>

          <div
            v-if="answers.length === 0"
            class="text-center text-gray-400 py-5 text-xs"
          >
            暂无答案，请点击"添加答案"按钮添加
          </div>
        </div>
        <UButton
          label="确认"
          color="primary"
          variant="subtle"
          @click="updateQuestion"
          class="w-full mt-4 justify-center"
        />
      </template>
    </UModal>
  </div>
</template>

<script setup lang="ts">
// TODO 题目从两个地方来，一个是本地缓存，一个是后端，本地缓存没有就在后端获取，提升响应速度
const { $api } = useNuxtApp();
const toast = useToast();

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

const fetchQuestion = async (id: string) => {
  if (!id || id.trim() === "") {
    return;
  }

  let result;
  try {
    result = await $api.tenant.questionGroupQuestionDetail(props.tenantId, id);
  } catch (e) {
    const error = e as any;
    const title = error.response.data.title;
    toast.add({ title: "获取题目失败：" + title, color: "error" });
    return;
  }

  content.value = result.data.markupContent as string;
  remark.value = result.data.remark as string;
  type.value = result.data.type as QuestionType;
  level.value = result.data.level as number;
  answers.value = (result.data.answers ?? []).map((item) => ({
    sortOrder: item.sortOrder ?? 0,
    content: item.markupContent ?? "", // 如果源数据用content字段
    isCorrect: item.isCorrect ?? false,
    matchStrategy: item.matchStrategy,
  }));
};

watch(
  () => props.id,
  async (newId) => {
    if (newId) {
      await fetchQuestion(newId);
    }
  },
  { immediate: true },
);

const show = defineModel({ default: false, required: true });

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

const strategies = ref([
  {
    label: "精确匹配",
    id: MatchStrategy.Exact,
  },
  {
    label: "关键词匹配",
    id: MatchStrategy.Contains,
  },
  {
    label: "正则表达式匹配",
    id: MatchStrategy.Regex,
  },
  {
    label: "手动确认",
    id: MatchStrategy.Manual,
  },
]);

const content = ref("");
const type = ref<QuestionType>(QuestionType.SingleChoice);
const level = ref(0);
const remark = ref("");

type EditableAnswer = Omit<CreateQuestionAnswerCommand, "content"> & { content: string };
const answers = ref<EditableAnswer[]>([]);
const emit = defineEmits<{
  (e: "update", id: string, payLoad: UpdateQuestionCommand): void;
}>();

const addAnswer = () => {
  answers.value.push({
    content: "",
    isCorrect: false,
    sortOrder: answers.value.length,
    matchStrategy: MatchStrategy.Exact,
  });
};

const removeAnswer = (index: number) => {
  answers.value.splice(index, 1);
};

const updateQuestion = async () => {
  let result: UpdateQuestionCommand = {
    content: content.value,
    type: type.value,
    level: level.value,
    remark: remark.value,
    answers: answers.value,
  };
  emit("update", props.id, result);
};
</script>
