<template>
  <CommonPageLayout>
    <template #breadcrumb>
      <UBreadcrumb :items="links" class="py-2" />
    </template>

    <div class="flex justify-between items-center mb-4">
      <div class="flex gap-2">
        <!-- 主要操作 - 发布 -->
        <UButton
          @click="changePublishedStatus"
          :color="template.isPublished ? 'error' : 'primary'"
          size="md"
        >
          {{ template.isPublished ? "取消发布" : "发布考试" }}
        </UButton>

        <!-- 次要操作 - 查看试卷样本 -->
        <UButton
          :to="`/tenant/${tenantId}/dash/quiz-template/${templateId}/snapshots`"
          color="neutral"
          variant="outline"
          size="md"
        >
          查看试卷样本
        </UButton>

        <UButton
          :to="`/tenant/${tenantId}/dash/quiz-template/${templateId}/review`"
          color="neutral"
          variant="outline"
          size="md"
        >
          批改试卷
        </UButton>

        <!-- 次要操作 - 查看成绩列表 -->
        <UButton
          :to="`/tenant/${tenantId}/dash/quiz-template/${templateId}/records`"
          color="neutral"
          variant="outline"
          size="md"
        >
          查看成绩
        </UButton>
      </div>

      <!-- 右侧：创建抽题规则 -->
      <CommonButtonsContainer>
        <QuizTemplateCreateQuestionSelectionRuleModal
          v-model="showCreateModal"
          @create="createQuestionselectionRuleAsync"
          :tenant-id="tenantId"
        />
      </CommonButtonsContainer>
    </div>

    <UCard class="w-full">
      <div class="space-y-4">
        <!-- 基础信息 -->
        <div class="grid grid-cols-2 gap-4">
          <div>
            <p class="text-sm text-gray-500">模板ID</p>
            <p class="font-mono text-sm">{{ template.id }}</p>
          </div>
          <div>
            <p class="text-sm text-gray-500">模板名称</p>
            <p class="font-medium">{{ template.name }}</p>
          </div>
          <div class="col-span-2">
            <p class="text-sm text-gray-500">描述</p>
            <CommonMarkUpViewer
              class="text-sm"
              :content="template.description || '-'"
            ></CommonMarkUpViewer>
          </div>
          <div class="col-span-2">
            <p class="text-sm text-gray-500">备注</p>
            <p class="text-sm">{{ template.remark || "-" }}</p>
          </div>
        </div>

        <!-- 时间信息 -->
        <div class="grid grid-cols-2 gap-4 border-t pt-4">
          <div>
            <p class="text-sm text-gray-500">是否不限时</p>
            <p>{{ template.isUnlimitedTime ? "是" : "否" }}</p>
          </div>
          <div>
            <p class="text-sm text-gray-500">时长(分钟)</p>
            <p v-if="template.durationMinutes">
              {{
                template.durationMinutes > 0 ? template.durationMinutes : "无限"
              }}
            </p>
            <p v-else>
              {{ "-" }}
            </p>
          </div>
          <div v-if="!template.isUnlimitedTime">
            <p class="text-sm text-gray-500">开始时间</p>
            <p>{{ formatDate(template.beginTime) }}</p>
          </div>
          <div v-if="!template.isUnlimitedTime">
            <p class="text-sm text-gray-500">结束时间</p>
            <p>{{ formatDate(template.endTime) }}</p>
          </div>
          <div>
            <p class="text-sm text-gray-500">尝试次数限制</p>
            <p>{{ template.attemptLimit || "无限制" }}</p>
          </div>
        </div>

        <!-- 系统信息 -->
        <div class="grid grid-cols-2 gap-4 border-t pt-4">
          <div>
            <p class="text-sm text-gray-500">创建时间</p>
            <p>{{ formatDate(template.createdAt) }}</p>
          </div>
          <div>
            <p class="text-sm text-gray-500">更新时间</p>
            <p>{{ formatDate(template.updatedAt) }}</p>
          </div>
          <div>
            <p class="text-sm text-gray-500">是否发布</p>
            <p>{{ template.isPublished ? "已发布" : "未发布" }}</p>
          </div>
          <div>
            <p class="text-sm text-gray-500">所有者ID</p>
            <p>{{ template.ownerId }}</p>
          </div>
        </div>
      </div>
    </UCard>

    <CommonConfirmModal
      v-model="showConfiemModal"
      @action="handleRuleDelete"
      title="是否删除抽题规则"
    />
    <QuizTemplateUpdateQuestionSelectionRuleModal
      v-model="showUpdateModal"
      @update="updateQuestionSelectionRuleAsync"
      :id="currentRuleId"
      :tenant-id="tenantId"
    />

    <USeparator class="my-2"><span>抽题规则</span></USeparator>
    <UContextMenu :items="items">
      <CommonDataTable
        :columns="cols"
        :data="rules"
        :selectable="true"
        v-model="selected"
        @row-dbl-click="handleRowDblClick"
        @row-right-click="switchCurrentId"
      >
        <template #id="{ row }">
          <CommonTruncateText :text="row.id || ''" direction="rtl">
          </CommonTruncateText>
        </template>
        <template #groupId="{ row }">
          <CommonTruncateText
            :text="`${row.groupId}`"
            width="10rem"
            direction="rtl"
          >
          </CommonTruncateText>
        </template>
        <template #questionType="{ row }">
          <CommonTruncateText
            :text="
              getQuestionTypeName(
                row.questionType || QuestionType.SingleChoice,
              ) || ''
            "
            direction="rtl"
          >
          </CommonTruncateText>
        </template>
      </CommonDataTable>
    </UContextMenu>
    <CommonLoadMoreButton
      v-if="haveMore"
      @click="() => fetchList(false, pageSize, false)"
      :disabled="!haveMore"
    />
  </CommonPageLayout>
</template>

<script setup lang="ts">
const { $api } = useNuxtApp();
import type { BreadcrumbItem } from "@nuxt/ui";
import type { ContextMenuItem } from "@nuxt/ui";

const toast = useToast();
const route = useRoute();

const templateId = ref<string>("");
const tenantId = ref("");

const template = ref<QuizTemplateDto>({});

const links = ref<BreadcrumbItem[]>([]);

onMounted(async () => {
  tenantId.value = route.params.tenantId as string;
  templateId.value = route.params.templateId as string;

  await fetchQuizTemplate();

  await fetchList(false, 10);

  links.value.push({
    label: "试卷管理",
    icon: "iconoir:atom",
    to: `/tenant/${tenantId.value}/dash/quiz-template`,
  });
  links.value.push({
    label: template.value.name as string,
    icon: "iconoir:atom",
    to: `/tenant/${tenantId.value}/dash/quiz-template/${templateId.value}`,
  });
});

const fetchQuizTemplate = async () => {
  let result;
  try {
    result = await $api.tenant.quizTemplateDetail(
      tenantId.value,
      templateId.value,
    );
  } catch (e: any) {
    let title = e.response.data.title;
    toast.add({ title: "获取试卷模板失败：" + title, color: "error" });
    return;
  }

  template.value = result.data;
};

const rules = ref<QuestionSelectionRuleDto[]>([]);
const selected = ref<QuestionSelectionRuleDto[]>([]);

const pageSize = ref(10);

const haveMore = ref(true);

const cols = [
  { key: "id", label: "ID", width: "16px" },
  { key: "groupId", label: "题库", width: "16px" },
  { key: "questionType", label: "题目类型" },
  { key: "level", label: "题目难度" },
  { key: "count", label: "数量" },
  { key: "score", label: "分数" },
];

const currentRuleId = ref("");
const items = ref<ContextMenuItem[][]>([
  [
    {
      label: "编辑",
      icon: "i-lucide-edit",
      onSelect: () => (showUpdateModal.value = true),
    },
    {
      label: "删除",
      color: "error",
      icon: "i-lucide-trash",
      onSelect: () => handleRuleDelete(),
    },
  ],
]);

const switchCurrentId = (rule: QuestionSelectionRuleDto) => {
  currentRuleId.value = rule.id || "";
};

const fetchList = async (
  isPre: boolean,
  pageSize: number,
  refresh: boolean = false,
) => {
  let cursor = getCursor(isPre);
  if (refresh) {
    cursor = undefined;
    // 如果是刷新，清空现有列表
    rules.value = [];
  }
  let result;
  try {
    result = await $api.tenant.quizTemplateRuleList(
      tenantId.value,
      templateId.value,
      {
        Cursor: cursor,
        Count: pageSize,
      },
    );
  } catch (e: any) {
    let title = e.response.data.title;
    toast.add({ title: "抽题规则获取失败：" + title, color: "error" });
    return;
  }

  // 拼接新数据而不是替换
  if (refresh) {
    // 刷新模式：直接替换
    rules.value = result.data;
  } else {
    // 加载更多模式：拼接数据
    if (isPre) {
      // 向前加载：新数据插入到前面
      rules.value = [...result.data, ...rules.value];
    } else {
      // 向后加载：新数据追加到后面
      rules.value = [...rules.value, ...result.data];
    }
  }

  haveMore.value = result.data.length == pageSize;
};

const getCursor = (isPre: boolean) => {
  if (rules.value.length == 0) {
    return undefined;
  }
  if (isPre) {
    return rules.value[0]?.id as string;
  } else {
    return rules.value[rules.value.length - 1]?.id as string;
  }
};

const showCreateModal = ref(false);
const createQuestionselectionRuleAsync = async (
  command: CreateQuestionSelectionRuleCommand,
) => {
  try {
    await $api.tenant.quizTemplateRuleCreate(
      tenantId.value,
      templateId.value,
      command,
    );
  } catch (e: any) {
    let title = e.response.data.title;
    toast.add({ title: "抽题规则创建失败：" + title, color: "error" });
    return;
  }

  toast.add({ title: "抽题规则创建成功。" });

  // if success
  showCreateModal.value = false;
  await fetchList(false, pageSize.value, true);
};

const showUpdateModal = ref(false);

const updateQuestionSelectionRuleAsync = async (
  command: UpdateQuestionSelectionRuleCommand,
) => {
  try {
    await $api.tenant.quizTemplateRuleUpdate(
      tenantId.value,
      currentRuleId.value,
      command,
    );
  } catch (e: any) {
    let title = e.response.data.title;
    toast.add({ title: "抽题规则更新失败：" + title, color: "error" });
    return;
  }

  toast.add({ title: "抽题规则修改成功。" });
  showUpdateModal.value = false;
  await fetchList(false, pageSize.value, true);
};

const showConfiemModal = ref(false);

const handleRuleDelete = async () => {
  if (!currentRuleId.value || currentRuleId.value.trim().length == 0) {
    toast.add({ title: "ID参数异常" });
    console.log(currentRuleId.value);
    return;
  }
  try {
    await $api.tenant.quizTemplateRuleDelete(
      tenantId.value,
      currentRuleId.value,
    );
  } catch (e: any) {
    let title = e.response.data.title;
    toast.add({ title: "抽题规则删除失败：" + title, color: "error" });
    return;
  }

  toast.add({ title: "抽题规则删除成功。" });
  showConfiemModal.value = false;
  await fetchList(false, pageSize.value, true);
};

const changePublishedStatus = async () => {
  let result;
  try {
    result = await $api.tenant.quizTemplatePublishedUpdate(
      tenantId.value,
      template.value.id ?? "",
      {
        isPublished: !template.value.isPublished as boolean,
      },
    );
  } catch (e: any) {
    let title = e.response.data.title;
    toast.add({ title: "切换试卷模板状态失败：" + title, color: "error" });
    return;
  }

  toast.add({ title: "切换试卷模板状态成功" });
  await fetchQuizTemplate();
  await fetchList(false, 10);
};

const handleRowDblClick = (row: QuestionSelectionRuleDto) => {
  currentRuleId.value = row.id ?? "";
  showUpdateModal.value = true;
};
</script>

<style scoped></style>
