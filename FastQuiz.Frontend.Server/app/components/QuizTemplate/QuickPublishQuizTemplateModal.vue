<template>
  <UModal title="快速发布" v-model:open="show">
    <UButton label="快速发布" color="primary" variant="solid" />

    <template #body>
      <div class="space-y-4">
        <!-- 步骤指示器 -->
        <div class="flex items-center gap-1">
          <template v-for="(s, i) in steps" :key="s.title">
            <div
              class="flex items-center gap-2 px-2 py-1 rounded-md transition-colors"
              :class="
                step === i ? 'bg-primary-50 text-primary-700' : 'text-gray-400'
              "
            >
              <div
                class="w-5 h-5 rounded-full flex items-center justify-center text-xs font-medium shrink-0"
                :class="
                  step >= i
                    ? 'bg-primary-500 text-white'
                    : 'bg-gray-200 text-gray-500'
                "
              >
                {{ i + 1 }}
              </div>
              <span class="text-sm whitespace-nowrap">{{ s.title }}</span>
            </div>
            <div
              v-if="i < steps.length - 1"
              class="flex-1 h-px bg-gray-200 min-w-[8px]"
            />
          </template>
        </div>

        <USeparator />

        <!-- 第 1 步：试卷信息 -->
        <CommonFormContainer v-if="step === 0">
          <CommonFormInput label="名称" v-model="name" :required="true" />
          <CommonFormInput label="备注" v-model="remark" />
          <UFormField label="详情" class="w-full">
            <CommonMarkUpEditor
              v-model="description"
              placeholder="输入详情内容..."
            />
          </UFormField>
          <CommonFormNumberInput
            label="考试次数"
            v-model="attemptLimit"
            :min="1"
            :max="100"
          />
          <CommonFormSwitch label="是否长期有效" v-model="isUnlimitedTime" />
          <div v-show="!isUnlimitedTime">
            <CommonFormTimePicker label="开始时间" v-model="beginTime" />
            <CommonFormTimePicker label="结束时间" v-model="endTime" />
          </div>
          <CommonFormNumberInput
            label="考试时长（分钟）"
            v-model="durationMinutes"
            :min="0"
            :max="2880"
          />
          <UFormField label="限定组织" class="w-full">
            <USelect
              :items="orgOptions"
              value-key="id"
              label="限定组织"
              v-model="orgId"
              class="w-full"
            />
          </UFormField>
          <p class="text-sm text-gray-500 px-1">默认为长期有效、不限组织。</p>
        </CommonFormContainer>

        <!-- 第 2 步：抽题规则 -->
        <CommonFormContainer v-else-if="step === 1">
          <div
            v-if="loadingGroups"
            class="flex justify-center py-6 text-sm text-gray-500"
          >
            正在加载题库...
          </div>
          <template v-else>
            <UFormField label="题库" class="w-full">
              <USelect
                :items="groupOptions"
                value-key="id"
                label="题库"
                :disabled="groups.length === 0"
                :placeholder="groups.length === 0 ? '没有可用题库' : '选择题库'"
                v-model="groupId"
                class="w-full"
              />
            </UFormField>
            <UFormField label="题型" class="w-full">
              <USelect
                :items="questionTypeItems"
                value-key="id"
                label="题型"
                v-model="questionType"
                class="w-full"
              />
            </UFormField>
            <CommonFormNumberInput
              label="难度"
              v-model="level"
              :min="0"
              :max="5"
            />
            <CommonFormNumberInput
              label="抽取数量"
              v-model="ruleCount"
              :min="1"
              :max="currentCount > 0 ? currentCount : undefined"
            />
            <CommonFormNumberInput
              label="每题分数"
              v-model="ruleScore"
              :min="1"
            />

            <div v-if="currentCount > 0" class="text-sm text-gray-500 px-1">
              当前题库共有 {{ currentCount }} 道符合条件的题目
            </div>
            <div
              v-else-if="groupId && fetchingCount"
              class="text-sm text-gray-500 px-1"
            >
              正在统计题目数量...
            </div>
            <div
              v-else-if="groupId && !fetchingCount"
              class="text-sm text-yellow-600 px-1"
            >
              没有符合条件的题目，请调整题型或难度，否则试卷样本会生成失败
            </div>

            <p class="text-sm text-gray-500 px-1">
              已按默认值预填：第一个题库 · 单选 · 全部难度 · 1 题 · 1
              分。发布至少需要 1 条抽题规则。
            </p>
          </template>
        </CommonFormContainer>

        <!-- 第 3 步：试卷样本数量 -->
        <CommonFormContainer v-else-if="step === 2">
          <CommonFormNumberInput
            label="试卷样本数量"
            v-model="snapshotCount"
            :min="1"
            :max="100"
          />
          <p class="text-sm text-gray-500 px-1">
            每份样本会按抽题规则从题库随机抽题。发布至少需要 1 份试卷样本。
          </p>
        </CommonFormContainer>

        <!-- 第 4 步：确认发布 -->
        <div v-else class="space-y-3">
          <div class="space-y-2 text-sm">
            <div class="flex border-b border-gray-100 pb-2">
              <span class="w-24 text-gray-500 shrink-0">名称</span>
              <span class="text-gray-900 break-all">{{ name || "-" }}</span>
            </div>
            <div class="flex border-b border-gray-100 pb-2">
              <span class="w-24 text-gray-500 shrink-0">备注</span>
              <span class="text-gray-900 break-all">{{ remark || "-" }}</span>
            </div>
            <div class="flex border-b border-gray-100 pb-2">
              <span class="w-24 text-gray-500 shrink-0">考试次数</span>
              <span class="text-gray-900">{{ attemptLimit }} 次</span>
            </div>
            <div class="flex border-b border-gray-100 pb-2">
              <span class="w-24 text-gray-500 shrink-0">有效期</span>
              <span class="text-gray-900">
                {{
                  isUnlimitedTime
                    ? "长期有效"
                    : `${formatDate(beginTime)} ~ ${formatDate(endTime)}`
                }}
              </span>
            </div>
            <div class="flex border-b border-gray-100 pb-2">
              <span class="w-24 text-gray-500 shrink-0">考试时长</span>
              <span class="text-gray-900">{{ durationMinutes }} 分钟</span>
            </div>
            <div class="flex border-b border-gray-100 pb-2">
              <span class="w-24 text-gray-500 shrink-0">限定组织</span>
              <span class="text-gray-900">{{ orgLabel }}</span>
            </div>
            <div class="flex border-b border-gray-100 pb-2">
              <span class="w-24 text-gray-500 shrink-0">抽题规则</span>
              <span class="text-gray-900">
                {{ groupLabel }} · {{ questionTypeLabel }} · 难度
                {{ level === 0 ? "全部" : level }} · {{ ruleCount }} 题 ×
                {{ ruleScore }} 分
              </span>
            </div>
            <div class="flex pb-1">
              <span class="w-24 text-gray-500 shrink-0">试卷样本</span>
              <span class="text-gray-900">{{ snapshotCount }} 份</span>
            </div>
          </div>

          <UAlert
            v-if="error"
            color="error"
            variant="soft"
            :title="error"
            @close="error = ''"
          />

          <UAlert
            v-if="warnings.length > 0"
            color="warning"
            variant="soft"
            title="请先确认"
            :description="warnings.join('；')"
          />

          <ul class="text-sm text-gray-500 list-disc pl-5 space-y-1">
            <li>发布后会依次创建：试卷模板 → 抽题规则 → 试卷样本 → 发布。</li>
            <li>
              任一步失败会停在当前步，已创建的内容会保留，请到试卷管理页手动处理。
            </li>
          </ul>
        </div>

        <USeparator />

        <!-- 底部按钮 -->
        <div class="flex gap-3 justify-center">
          <UButton
            v-if="step > 0"
            class="flex-1 justify-center"
            color="neutral"
            variant="outline"
            :disabled="loading"
            @click="prev"
          >
            上一步
          </UButton>
          <UButton
            v-if="step < steps.length - 1"
            class="flex-1 justify-center"
            @click="next"
          >
            下一步
          </UButton>
          <UButton
            v-else
            class="flex-1 justify-center"
            :loading="loading"
            :disabled="warnings.length > 0"
            @click="submit"
          >
            确认发布
          </UButton>
          <UButton
            class="flex-1 justify-center"
            color="error"
            :disabled="loading"
            @click="show = false"
          >
            取消
          </UButton>
        </div>
      </div>
    </template>
  </UModal>
</template>

<script setup lang="ts">
const { $api } = useNuxtApp();
const toast = useToast();

const show = defineModel({ required: true, default: false });

const props = defineProps<{
  tenantId: string;
  orgs: OrganizationDto[];
}>();

const steps = [
  { title: "试卷信息" },
  { title: "抽题规则" },
  { title: "试卷样本" },
  { title: "确认发布" },
];

const questionTypeItems = [
  { label: "单选", id: QuestionType.SingleChoice },
  { label: "多选", id: QuestionType.MultipleChoice },
  { label: "填空", id: QuestionType.FillIn },
];

// ---------- 步骤 1：试卷信息 ----------
const name = ref<string>("");
const remark = ref<string>("");
const description = ref<string>("");
const attemptLimit = ref<number>(1);
const isUnlimitedTime = ref<boolean>(true);
const beginTime = ref<string>("");
const endTime = ref<string>("");
const durationMinutes = ref<number>(60);
const orgId = ref<string | undefined>(undefined);

// ---------- 步骤 2：抽题规则 ----------
const groups = ref<QuestionGroupDto[]>([]);
const loadingGroups = ref(false);
const groupId = ref<string>("");
const questionType = ref<QuestionType>(QuestionType.SingleChoice);
const level = ref<number>(0);
const ruleCount = ref<number>(1);
const ruleScore = ref<number>(1);
const currentCount = ref<number>(0);
const fetchingCount = ref(false);

// ---------- 步骤 3：试卷样本 ----------
const snapshotCount = ref<number>(1);

// ---------- 流程状态 ----------
const step = ref(0);
const loading = ref(false);
const error = ref("");

const orgOptions = computed(() => [
  { id: undefined, label: "全部组织" },
  ...props.orgs.map((org) => ({
    id: org.id,
    label: org.name ?? "未知组织",
  })),
]);

const groupOptions = computed(() =>
  groups.value.map((group) => ({
    id: group.id,
    label: group.name ?? "未知题库",
  })),
);

const orgLabel = computed(
  () => orgOptions.value.find((o) => o.id === orgId.value)?.label ?? "全部组织",
);

const groupLabel = computed(
  () =>
    groupOptions.value.find((g) => g.id === groupId.value)?.label ??
    "未选择题库",
);

const questionTypeLabel = computed(
  () =>
    questionTypeItems.find((t) => t.id === questionType.value)?.label ?? "-",
);

// 发布前的前端体检（后端也会校验，这里只是提前拦掉确定失败的提交）
const warnings = computed(() => {
  const list: string[] = [];
  if (!groupId.value) {
    list.push("还没有可用的抽题规则（需要先在题库管理里创建题库）");
  }
  if (groupId.value && currentCount.value === 0) {
    list.push("当前题库没有符合条件的题目，试卷样本会生成失败");
  }
  if (!isUnlimitedTime.value && new Date(beginTime.value) < new Date()) {
    list.push("开始时间不能早于当前时间，否则无法发布");
  }
  return list;
});

// 打开弹窗时重置为默认值，并后台加载题库
watch(show, (open) => {
  if (open) {
    reset();
    groupId.value = "";
    loadAllGroups();
  }
});

const reset = () => {
  step.value = 0;
  loading.value = false;
  error.value = "";

  name.value = "";
  remark.value = "";
  description.value = "";
  attemptLimit.value = 1;
  isUnlimitedTime.value = true;
  beginTime.value = "";
  endTime.value = "";
  durationMinutes.value = 60;
  orgId.value = undefined;

  questionType.value = QuestionType.SingleChoice;
  level.value = 0;
  ruleCount.value = 1;
  ruleScore.value = 1;
  currentCount.value = 0;

  snapshotCount.value = 1;
};

// 默认选第一个题库
const loadAllGroups = async () => {
  loadingGroups.value = true;
  try {
    let allGroups: QuestionGroupDto[] = [];
    let cursor: string | undefined = undefined;
    let hasMore = true;

    while (hasMore) {
      const result = await $api.tenant.questionGroupList(props.tenantId, {
        Cursor: cursor,
        Count: 200,
        IsPrevious: false,
        NamePrefix: undefined,
      });

      if (result.data && result.data.length > 0) {
        allGroups = [...allGroups, ...result.data];
        if (result.data.length < 200) {
          hasMore = false;
        } else {
          cursor = result.data[result.data.length - 1]?.id ?? undefined;
        }
      } else {
        hasMore = false;
      }
    }

    groups.value = allGroups;

    if (allGroups.length > 0) {
      groupId.value = allGroups[0]?.id ?? "";
    }
  } catch (e: any) {
    groups.value = [];
    console.error("加载题库失败:", e);
  } finally {
    loadingGroups.value = false;
  }
};

const fetchQuestionCountAsync = async () => {
  if (!groupId.value) {
    currentCount.value = 0;
    return;
  }

  fetchingCount.value = true;
  try {
    const result = await $api.tenant.questionGroupCountList(
      props.tenantId,
      groupId.value,
      { type: questionType.value, level: level.value },
    );
    currentCount.value = result.data ?? 0;
  } catch (e: any) {
    currentCount.value = 0;
  } finally {
    fetchingCount.value = false;
    if (ruleCount.value > currentCount.value && currentCount.value > 0) {
      ruleCount.value = currentCount.value;
    }
  }
};

watch([groupId, questionType, level], () => {
  fetchQuestionCountAsync();
});

const next = () => {
  if (step.value === 0) {
    const trimmed = name.value.trim();
    if (trimmed.length < 2 || trimmed.length > 100) {
      error.value = "名称长度需在 2 到 100 个字符之间";
      return;
    }
  }

  if (step.value === 1) {
    if (!groupId.value) {
      error.value = "请选择题库";
      return;
    }
    if (ruleCount.value < 1 || ruleScore.value < 1) {
      error.value = "抽取数量与每题分数都需要大于等于 1";
      return;
    }
  }

  if (step.value === 2) {
    if (snapshotCount.value < 1) {
      error.value = "试卷样本数量至少为 1";
      return;
    }
  }

  error.value = "";
  step.value = Math.min(step.value + 1, steps.length - 1);
};

const prev = () => {
  error.value = "";
  step.value = Math.max(step.value - 1, 0);
};

const stepTitles = ["创建试卷模板", "创建抽题规则", "生成试卷样本", "发布"];

// 依次调用 4 个接口，任一步失败就停在当前步，已创建的资源保留
const submit = async () => {
  if (loading.value) {
    return;
  }

  loading.value = true;
  error.value = "";

  let templateId = "";
  let done = 0;

  try {
    // 1. 创建试卷模板
    const created = await $api.tenant.quizTemplateCreate(props.tenantId, {
      name: name.value.trim(),
      description: description.value,
      remark: remark.value,
      attemptLimit: attemptLimit.value,
      isUnlimitedTime: isUnlimitedTime.value,
      beginTime: beginTime.value,
      endTime: endTime.value,
      durationMinutes: durationMinutes.value,
      userGroupId: orgId.value,
    });
    done = 1;
    templateId = created.data?.id ?? "";

    if (!templateId) {
      error.value = "试卷模板已创建但未返回 ID，请到试卷管理页确认";
      toast.add({ title: "快速发布中断：试卷模板未返回 ID", color: "error" });
      return;
    }

    // 2. 创建抽题规则
    await $api.tenant.quizTemplateRuleCreate(props.tenantId, templateId, {
      groupId: groupId.value,
      questionType: questionType.value,
      level: level.value,
      count: ruleCount.value,
      score: ruleScore.value,
    });
    done = 2;

    // 3. 生成试卷样本
    await $api.tenant.quizTemplateQuizSnapshotCreate(
      props.tenantId,
      templateId,
      { count: snapshotCount.value },
    );
    done = 3;

    // 4. 发布
    await $api.tenant.quizTemplatePublishedUpdate(props.tenantId, templateId, {
      isPublished: true,
    });
    done = 4;
  } catch (e: any) {
    const title = e?.response?.data?.title ?? e?.message ?? "未知错误";
    const failedStep = stepTitles[Math.min(done, 3)];

    error.value = `${failedStep}失败：${title}${
      templateId ? `（已创建试卷模板 ${templateId}，请到试卷管理页处理）` : ""
    }`;
    toast.add({
      title: `快速发布中断：${failedStep}失败：${title}`,
      color: "error",
    });
    return;
  } finally {
    loading.value = false;
  }

  toast.add({ title: `试卷「${name.value.trim()}」快速发布成功` });
  show.value = false;
  emit("published", templateId);
};

const emit = defineEmits<{
  (e: "published", templateId: string): void;
}>();
</script>

<style scoped></style>
