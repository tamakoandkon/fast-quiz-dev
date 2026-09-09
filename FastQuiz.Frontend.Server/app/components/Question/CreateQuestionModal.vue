<template>
  <div>
    <UModal title="新增题目" v-model:open="show">
      <UButton label="新增题目" color="primary" variant="solid" />

      <template #body>
        <CommonFormContainer>
          <UFormField label="题干" class="w-full">
            <CommonMarkUpEditor
              v-model="content"
              placeholder="请输入题干..."
            ></CommonMarkUpEditor>
          </UFormField>
          <UFormField label="题型" class="w-full">
            <USelect
              :items="items"
              value-key="id"
              label="题型"
              v-model="type"
              class="w-full"
            />
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

              <CommonFormInput
                label="答案内容"
                v-model="answer.content as string"
              ></CommonFormInput>

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
          @click="createQuestion"
          class="w-full mt-4 justify-center"
        />
      </template>
    </UModal>
  </div>
</template>

<script setup lang="ts">
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

const props = defineProps({
  parentId: {
    type: String,
    default: undefined,
  },
});

onMounted(() => {
  handleSingleChoiceType();
});

const show = defineModel({ required: true, default: false });
const content = ref("");
const type = ref<QuestionType>(QuestionType.SingleChoice);
const level = ref(0);
const remark = ref("");

const answers = ref<CreateQuestionAnswerCommand[]>([]);
const emit = defineEmits<{
  (e: "create", payLoad: CreateQuestionCommand): void;
}>();

const addAnswer = () => {
  answers.value.push({} as CreateQuestionAnswerCommand);
};

const removeAnswer = (index: number) => {
  answers.value.splice(index, 1);
};

const createQuestion = async () => {
  let result: CreateQuestionCommand = {
    content: content.value,
    type: type.value,
    level: level.value,
    remark: remark.value,
    answers: answers.value,
    parentId: props.parentId,
  };
  emit("create", result);
  content.value = "";
  level.value = 0;
  remark.value = "";
  type.value = QuestionType.SingleChoice;
  handleSingleChoiceType();
};

// 监控 type 变化
watch(type, (newType, oldType) => {
  // 在这里处理 type 变化后的逻辑
  handleTypeChange(newType, oldType);
});

// type 变化时的处理方法
const handleTypeChange = (newType: QuestionType, oldType: QuestionType) => {
  console.log(`题型从 ${oldType} 变为 ${newType}`); // delete this

  // 根据不同题型处理答案
  switch (newType) {
    case QuestionType.SingleChoice:
      handleSingleChoiceType();
      break;
    case QuestionType.MultipleChoice:
      handleMultipleChoiceType();
      break;
    case QuestionType.FillIn:
      handleFillInType();
      break;
    default:
      break;
  }
};

// 各题型对应的处理方法
const handleSingleChoiceType = () => {
  // 单选：两个预设答案，第一个为正确答案
  answers.value = [
    {
      content: "答案#1",
      isCorrect: true,
      sortOrder: 0,
    },
    {
      content: "答案#2",
      isCorrect: false,
      sortOrder: 1,
    },
  ];
};

const handleMultipleChoiceType = () => {
  // 多选：两个预设答案，第一个为正确答案
  answers.value = [
    {
      content: "答案#1",
      isCorrect: true,
      sortOrder: 0,
    },
    {
      content: "答案#2",
      isCorrect: true,
      sortOrder: 1,
    },
  ];
};

const handleFillInType = () => {
  // 填空：只有一个答案，默认精确匹配
  answers.value = [
    {
      content: "答案",
      isCorrect: true,
      sortOrder: 0,
      matchStrategy: MatchStrategy.Exact,
    },
  ];
};
</script>
