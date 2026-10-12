<template>
  <CommonPageLayout>
    <template #breadcrumb>
      <UBreadcrumb :items="links" />
    </template>

    <QuestionGroupHeroCard
      :name="group.name as string"
      :remark="group.remark || ''"
      :count="group.count"
    />
    <CommonButtonsContainer>
      <QuestionCreateQuestionModal
        v-model="showNewModel"
        @create="createQuestion"
      ></QuestionCreateQuestionModal>
      <QuestionUpdateQuestionModal
        v-model="isOpen"
        @update="handleUpdateQuestion"
        :id="currentId"
        :tenant-id="tenantId"
      >
      </QuestionUpdateQuestionModal>
      <CommonConfirmModal
        v-model="showConfirmModel"
        @action="handleDelete"
        title="是否删除题目"
      />

      <UButton
        color="secondary"
        icon="i-lucide-upload"
        :loading="isImporting"
        @click="triggerImport"
      >
        批量导入CSV
      </UButton>
      <input
        ref="fileInputRef"
        type="file"
        accept=".csv"
        class="hidden"
        @change="handleFileImport"
      />
    </CommonButtonsContainer>

    <QuestionGroupStatistics
      ref="statisticsRef"
      class="mb-6"
      :tenant-id="tenantId"
      :group-id="id"
    />

    <UContextMenu :items="items">
      <CommonDataTable
        :columns="cols"
        :data="questions"
        :selectable="true"
        @row-right-click="switchCurrentId"
      >
        <template #id="{ row }">
          <CommonTruncateText :text="row.id || ''" direction="rtl">
            <div>{{ row.id }}</div>
          </CommonTruncateText>
        </template>
        <template #markUpContent="{ row }">
          <CommonTruncateText :text="row.markupContent || ''">
          </CommonTruncateText>
        </template>
        <template #remark="{ row }">
          <CommonTruncateText :text="row.remark || ''"> </CommonTruncateText>
        </template>
        <template #type="{ row }">
          {{ getQuestionTypeName(row.type as QuestionType) }}
        </template>
      </CommonDataTable>
    </UContextMenu>

    <CommonCursorPageManager
      v-model="pageSize"
      @change="fectchList"
      :current-size="questions.length"
    ></CommonCursorPageManager>
  </CommonPageLayout>
</template>

<script setup lang="ts">
const route = useRoute();
const { $api } = useNuxtApp();
import type { BreadcrumbItem } from "@nuxt/ui";
import type { ContextMenuItem } from "@nuxt/ui";
import Papa from "papaparse";

const links = ref<BreadcrumbItem[]>([]);

const toast = useToast();

const pageSize = ref(10);

const cols = [
  { key: "id", label: "ID", width: "80px" },
  { key: "markUpContent", label: "题干" },
  { key: "type", label: "题型" },
  { key: "level", label: "难度" },
  { key: "remark", label: "备注" },
];

const items = ref<ContextMenuItem[][]>([
  [
    {
      label: "编辑",
      icon: "i-lucide-edit",
      onSelect: () => (isOpen.value = true),
    },
    {
      label: "删除",
      color: "error",
      icon: "i-lucide-trash",
      onSelect: () => handleDelete(),
    },
  ],
]);

const selected = ref<QuestionDto[]>([]);
const showNewModel = ref(false);

const id = ref<string>("");
const tenantId = ref("");

const group = ref<QuestionGroupDto>({});

const questions = ref<QuestionDto[]>([]);

const currentId = ref("");

const statisticsRef = ref<{ refresh: () => Promise<void> } | null>(null);

/**
 * 题目发生增删改后：刷新列表、刷新统计面板，
 * 并重新拉一次题库详情，让顶部的题目数量与统计保持一致。
 */
const refreshAfterQuestionChange = async () => {
  await fectchList(false, pageSize.value, true);
  await statisticsRef.value?.refresh();
  await refreshGroup();
};

const refreshGroup = async () => {
  try {
    const result = await $api.tenant.questionGroupDetail(tenantId.value, id.value);
    group.value = result.data;
  } catch (e) {
    // 仅刷新头部摘要，失败不打断主流程
    console.error("刷新题库信息失败:", e);
  }
};

onMounted(async () => {
  id.value = route.params.qgId as string;
  tenantId.value = route.params.tenantId as string;

  links.value.push({
    label: "题库列表",
    icon: "i-lucide-book-open",
    to: `/tenant/${tenantId.value}/dash/question-group`,
  });

  let result;
  try {
    result = await $api.tenant.questionGroupDetail(tenantId.value, id.value);
  } catch (e) {
    const error = e as any;
    const title = error.response.data.title;
    toast.add({ title: "获取题库失败：" + title, color: "error" });
    return;
  }
  group.value = result.data;

  links.value.push({
    label: group.value.name as string,
    icon: "i-lucide-book-open",
    to: `/tenant/${tenantId.value}/dash/question-group/${group.value.id}`,
  });

  await fectchList(false, 10, true);
});

const fectchList = async (
  isPre: boolean,
  pageSize: number,
  refresh: boolean = false,
) => {
  let cusor = getCursor(isPre);
  if (refresh) {
    cusor = undefined;
  }

  let result;
  try {
    result = await $api.tenant.questionGroupQuestionList(
      tenantId.value,
      id.value as string,
      {
        Cursor: cusor,
        Level: -1,
        Count: pageSize,
        IsPrevious: isPre,
        Type: undefined,
      },
    );
  } catch (e) {
    const error = e as any;
    const title = error.response.data.title;
    toast.add({ title: "获取题目失败：" + title, color: "error" });
    return;
  }

  questions.value = result.data;
};

const getCursor = (isPre: boolean) => {
  if (questions.value.length == 0) {
    return undefined;
  }
  if (isPre) {
    return questions.value[0]?.id as string;
  } else {
    return questions.value[questions.value.length - 1]?.id as string;
  }
};

const handleDelete = async () => {
  if (!currentId.value || currentId.value.trim().length == 0) {
    toast.add({ title: "ID参数异常" });
    console.log(currentId.value);
    return;
  }
  try {
    await $api.tenant.questionGroupQuestionDelete(
      tenantId.value,
      currentId.value,
    );
  } catch (e) {
    const error = e as any;
    const title = error.response.data.title;
    toast.add({ title: "删除题目失败：" + title, color: "error" });
    return;
  }

  showConfirmModel.value = false;
  toast.add({ title: "题目删除成功。" });
  await refreshAfterQuestionChange();
};

const createQuestion = async (command: CreateQuestionCommand) => {
  try {
    await $api.tenant.questionGroupQuestionCreate(
      tenantId.value,
      id.value as string,
      command,
    );
  } catch (e) {
    const error = e as any;
    const title = error.response.data.title;
    toast.add({ title: "添加题目失败：" + title, color: "error" });
    return;
  }

  toast.add({ title: "题目创建成功。" });
  showNewModel.value = false;
  await refreshAfterQuestionChange();
};

const handleUpdateQuestion = async (
  id: string,
  command: UpdateQuestionCommand,
) => {
  try {
    await $api.tenant.questionGroupQuestionUpdate(tenantId.value, id, {
      content: command.content,
      type: command.type,
      level: command.level,
      remark: command.remark,
      answers: command.answers,
    });
  } catch (e) {
    const error = e as any;
    const title = error.response.data.title;
    toast.add({ title: "修改题目失败：" + title, color: "error" });
    return;
  }

  toast.add({ title: "题目修改成功。" });
  isOpen.value = false;
  await refreshAfterQuestionChange();
};

const isOpen = ref(false);

const showConfirmModel = ref(false);

const switchCurrentId = (row: QuestionDto) => {
  currentId.value = row.id || "";
};

// ---------- 新增变量 ----------
const fileInputRef = ref<HTMLInputElement | null>(null);
const isImporting = ref(false);

// ---------- 触发文件选择 ----------
const triggerImport = () => {
  fileInputRef.value?.click();
};

// ---------- CSV 行解析（支持引号包裹的字段）----------
const parseCSVRow = (row: string): string[] => {
  const values: string[] = [];
  let current = "";
  let insideQuotes = false;
  for (let i = 0; i < row.length; i++) {
    const ch = row[i];
    if (ch === '"') {
      if (insideQuotes && row[i + 1] === '"') {
        current += '"';
        i++;
      } else {
        insideQuotes = !insideQuotes;
      }
    } else if (ch === "," && !insideQuotes) {
      values.push(current.trim());
      current = "";
    } else {
      current += ch;
    }
  }
  values.push(current.trim());
  return values;
};

// ---------- 解析单个答案格式: [*]答案内容[@@匹配类型] ----------
const parseAnswer = (raw: string) => {
  let content = raw.trim();
  let isCorrect = false;
  let matchType = "Exact"; // 默认精准匹配

  // 检查是否以 * 开头（正确答案标记）
  if (content.startsWith("*")) {
    isCorrect = true;
    content = content.substring(1);
  }

  // 检查是否包含 @@ 匹配类型
  const matchIdx = content.indexOf("@@");
  if (matchIdx !== -1) {
    matchType = content.substring(matchIdx + 2);
    content = content.substring(0, matchIdx);
  }

  return { content, isCorrect, matchType };
};

// ---------- 处理文件导入 ----------
const handleFileImport = async (event: Event) => {
  const target = event.target as HTMLInputElement;
  const file = target.files?.[0];
  if (!file) return;

  try {
    const text = await file.text();
    const result = Papa.parse(text, { header: false, skipEmptyLines: true });
    const rows = result.data as string[][];

    if (rows.length < 2) {
      toast.add({ title: "CSV文件无有效数据", color: "warning" });
      return;
    }

    // 跳过表头（第一行）
    const commands: CreateQuestionCommand[] = rows.slice(1).map((row) => {
      const [content, remark, type, levelStr, ...answerRaw] = row;
      const answers = answerRaw.map(parseAnswer); // parseAnswer 保留不变
      return {
        content,
        remark: remark || "",
        type: type,
        level: parseInt(levelStr, 10) || 0,
        answers,
      };
    });

    if (commands.length === 0) {
      toast.add({ title: "未解析到有效题目", color: "warning" });
      return;
    }

    // 循环调用现有 API 进行批量创建
    isImporting.value = true;
    let successCount = 0;
    let failCount = 0;

    for (const cmd of commands) {
      try {
        await $api.tenant.questionGroupQuestionCreate(
          tenantId.value,
          id.value,
          cmd,
        );
        successCount++;
      } catch (e) {
        failCount++;
        console.error("导入失败:", e, cmd);
      }
    }

    isImporting.value = false;
    // 清空 input 值，允许重复选择同一文件
    if (fileInputRef.value) fileInputRef.value.value = "";

    if (successCount > 0) {
      toast.add({ title: `成功导入 ${successCount} 道题目`, color: "success" });
      await refreshAfterQuestionChange(); // 刷新列表与统计
    }
    if (failCount > 0) {
      toast.add({
        title: `${failCount} 道题目导入失败，请检查控制台日志`,
        color: "error",
      });
    }
  } catch (e) {
    toast.add({ title: "读取或解析CSV文件失败", color: "error" });
    console.error(e);
  }
};
</script>
