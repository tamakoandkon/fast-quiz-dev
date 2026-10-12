<template>
  <CommonManagePageContainer>
    <UBreadcrumb :items="links" class="py-2" />
    <USeparator class="my-2" />

    <!-- 标题与筛选 -->
    <div class="mb-4 flex flex-wrap items-center gap-3">
      <h2 class="text-xl font-semibold">考试成绩记录</h2>
      <span class="text-sm text-gray-500">考生：{{ userId }}</span>

      <div class="ml-auto flex flex-wrap items-center gap-3">
        <!-- 列表筛选：考试模板（仅列出该考生参加过的考试） -->
        <div class="flex items-center gap-2">
          <USelect
            v-model="templateId"
            :items="templateOptions"
            value-key="id"
            placeholder="全部考试"
            icon="i-lucide-search"
            size="md"
            class="w-64"
          />
        </div>

        <!-- 列表筛选：是否只取最好的一次成绩 -->
        <div class="flex items-center gap-2">
          <USwitch v-model="onlyBest" color="primary" size="md" />
          <span class="text-sm text-gray-500">只取最好的一次成绩</span>
        </div>

        <UButton
          color="neutral"
          variant="outline"
          icon="i-lucide-download"
          @click="showExportModal = true"
        >
          导出 CSV
        </UButton>
      </div>
    </div>

    <CommonDataTable :columns="cols" :data="records" :selectable="false">
      <template #quizTemplateNema="{ row }">
        {{ row.quizTemplateNema || "--" }}
      </template>

      <template #status="{ row }">
        <UBadge :color="getStatusColor(row.status)" size="sm" variant="subtle">
          {{ getStatusText(row.status) }}
        </UBadge>
      </template>

      <template #startTime="{ row }">
        {{ row.startTime ? formatDateTime(row.startTime) : "--" }}
      </template>

      <template #submitTime="{ row }">
        {{ row.submitTime ? formatDateTime(row.submitTime) : "--" }}
      </template>

      <template #score="{ row }">
        <span
          :class="
            row.status === QuizAttemptStatus.Graded
              ? 'text-green-600 font-medium'
              : ''
          "
        >
          {{ row.score ?? "--" }}
        </span>
      </template>

      <template #actions="{ row }">
        <UButton
          :to="`/tenant/${tenantId}/dash/quiz-template/${row.quizTemplateId}/review/${row.id}`"
          color="neutral"
          variant="outline"
          size="xs"
          icon="i-lucide-eye"
        >
          查看
        </UButton>
      </template>
    </CommonDataTable>

    <CommonCursorPageManager
      @change="fetchList"
      :current-size="records.length"
      v-model="pageSize"
    />

    <!-- 空状态 -->
    <div
      v-if="loaded && records.length === 0"
      class="mt-6 bg-white rounded-lg shadow p-12 text-center text-gray-500"
    >
      该考生暂无考试记录。
    </div>

    <QuizAttemptExportUserRecordsModal
      v-model="showExportModal"
      v-model:loading="exporting"
      :templates="attendedTemplates"
      @export="handleExport"
    />
  </CommonManagePageContainer>
</template>

<script setup lang="ts">
import type { BreadcrumbItem } from "@nuxt/ui";

const { $api } = useNuxtApp();
const route = useRoute();
const toast = useToast();

const tenantId = ref("");
const userId = ref("");

const links = ref<BreadcrumbItem[]>([]);
const records = ref<QuizAttempDto[]>([]);
const loaded = ref(false);

// 列表筛选（templateId 为 undefined 表示「全部考试」）
const templateId = ref<string | undefined>(undefined);
const onlyBest = ref(false);

// 该考生参加过的考试（去重后），用于下拉选项
const attendedTemplates = ref<{ id: string; label: string }[]>([]);

const pageSize = ref(10);

const cols = [
  { key: "quizTemplateNema", label: "考试" },
  { key: "status", label: "状态" },
  { key: "startTime", label: "开始时间" },
  { key: "submitTime", label: "交卷时间" },
  { key: "score", label: "得分" },
  { key: "actions", label: "操作", width: "120px" },
];

// 下拉选项：首位「全部考试」（id 为 undefined），其后为该考生参加过的各场考试。
const templateOptions = computed(() => [
  { id: undefined, label: "全部考试" },
  ...attendedTemplates.value,
]);

onMounted(async () => {
  tenantId.value = route.params.tenantId as string;
  userId.value = route.params.userId as string;

  links.value.push({
    label: "用户管理",
    icon: "iconoir:user",
    to: `/tenant/${tenantId.value}/dash/user`,
  });
  links.value.push({ label: `考试成绩记录` });

  await Promise.all([
    fetchList(false, pageSize.value, true),
    loadAttendedTemplates(),
  ]);
  loaded.value = true;
});

/** 切换筛选后重新拉取第一页。 */
watch([onlyBest, templateId], () => {
  fetchList(false, pageSize.value, true);
});

/**
 * 全量拉取该考生的所有考试记录，按 quizTemplateId 去重，
 * 得到「该考生参加过的考试」下拉选项（用 quizTemplateNema 作为显示名）。
 */
const loadAttendedTemplates = async () => {
  const seen = new Map<string, string>();
  let cursor: string | undefined = undefined;
  const size = 100;

  try {
    // 防御性上限，避免服务端游标异常导致死循环
    for (let page = 0; page < 1000; page++) {
      const result = await $api.tenant.quizTemplateQuizAttemptUserRecordsList(
        tenantId.value,
        userId.value,
        {
          OnlyBest: false,
          Cursor: cursor,
          Count: size,
          IsPrevious: false,
        },
      );
      const batch = result.data ?? [];
      if (batch.length === 0) break;
      for (const r of batch) {
        if (r.quizTemplateId && !seen.has(r.quizTemplateId)) {
          seen.set(r.quizTemplateId, r.quizTemplateNema ?? r.quizTemplateId);
        }
      }
      if (batch.length < size) break;
      cursor = batch[batch.length - 1]?.id as string;
    }

    attendedTemplates.value = [...seen.entries()].map(([id, label]) => ({
      id,
      label,
    }));
  } catch (e: any) {
    const title = e?.response?.data?.title ?? "加载考试列表失败";
    toast.add({ title: `加载考试列表失败：${title}`, color: "error" });
  }
};

const getStatusText = (status?: QuizAttemptStatus) => {
  switch (status) {
    case QuizAttemptStatus.Pending:
      return "未开始";
    case QuizAttemptStatus.InProgress:
      return "进行中";
    case QuizAttemptStatus.Submitted:
      return "待批阅";
    case QuizAttemptStatus.Graded:
      return "已批阅";
    default:
      return "未知";
  }
};

const getStatusColor = (status?: QuizAttemptStatus) => {
  switch (status) {
    case QuizAttemptStatus.Graded:
      return "green";
    case QuizAttemptStatus.Submitted:
      return "yellow";
    case QuizAttemptStatus.InProgress:
      return "blue";
    case QuizAttemptStatus.Pending:
      return "gray";
    default:
      return "gray";
  }
};

const formatDateTime = (iso?: string | null): string => {
  if (!iso) return "--";
  const d = new Date(iso);
  return Number.isNaN(d.getTime())
    ? "--"
    : d.toLocaleString("zh-CN", { hour12: false });
};

const getCursor = (isPre: boolean) => {
  if (records.value.length === 0) return undefined;
  return isPre
    ? (records.value[0]?.id as string)
    : (records.value[records.value.length - 1]?.id as string);
};

const fetchList = async (
  isPre: boolean,
  size: number,
  refresh: boolean = false,
) => {
  let cursor = getCursor(isPre);
  if (refresh) cursor = undefined;

  try {
    const result = await $api.tenant.quizTemplateQuizAttemptUserRecordsList(
      tenantId.value,
      userId.value,
      {
        TemplateId: templateId.value || undefined,
        OnlyBest: onlyBest.value,
        Cursor: cursor,
        Count: size,
        IsPrevious: isPre,
      },
    );
    records.value = result.data ?? [];
  } catch (e: any) {
    const title = e?.response?.data?.title ?? "获取列表失败";
    toast.add({ title: `获取成绩列表失败：${title}`, color: "error" });
  }
};

// ---------- 导出 ----------

const showExportModal = ref(false);
const exporting = ref(false);

export interface ExportUserRecordsFilter {
  templateId?: string;
  onlyBest: boolean;
}

/** 拉取全部记录：按游标自动翻页，直到拉空。 */
const fetchAllRecords = async (
  filter: ExportUserRecordsFilter,
): Promise<QuizAttempDto[]> => {
  const all: QuizAttempDto[] = [];
  let cursor: string | undefined = undefined;
  const size = 100;

  // 防御性上限，避免服务端游标异常导致死循环
  for (let page = 0; page < 1000; page++) {
    const result = await $api.tenant.quizTemplateQuizAttemptUserRecordsList(
      tenantId.value,
      userId.value,
      {
        TemplateId: filter.templateId || undefined,
        OnlyBest: filter.onlyBest,
        Cursor: cursor,
        Count: size,
        IsPrevious: false,
      },
    );
    const batch = result.data ?? [];
    if (batch.length === 0) break;
    all.push(...batch);
    if (batch.length < size) break;
    cursor = batch[batch.length - 1]?.id as string;
  }

  return all;
};

/** 生成 CSV 文本（RFC4180 转义）。 */
const buildCsv = (rows: QuizAttempDto[]): string => {
  const header = ["考生", "考试", "状态", "开始时间", "交卷时间", "得分"];
  const escape = (v: unknown): string => {
    const s = v === null || v === undefined ? "" : String(v);
    return /[",\n\r]/.test(s) ? `"${s.replace(/"/g, '""')}"` : s;
  };

  const lines = [header.map(escape).join(",")];
  for (const r of rows) {
    lines.push(
      [
        r.assignId ?? "",
        r.quizTemplateNema ?? "",
        getStatusText(r.status),
        r.startTime ? formatDateTime(r.startTime) : "",
        r.submitTime ? formatDateTime(r.submitTime) : "",
        r.score ?? "",
      ]
        .map(escape)
        .join(","),
    );
  }
  return lines.join("\r\n");
};

const downloadCsv = (csv: string, filename: string) => {
  // 加 UTF-8 BOM，避免 Excel 打开中文乱码
  const blob = new Blob(["\uFEFF" + csv], {
    type: "text/csv;charset=utf-8;",
  });
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = filename;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  URL.revokeObjectURL(url);
};

/** 导出：使用模态里重新选择的筛选，而非列表页的当前筛选。 */
const handleExport = async (filter: ExportUserRecordsFilter) => {
  if (exporting.value) return;
  exporting.value = true;
  try {
    const rows = await fetchAllRecords(filter);
    if (rows.length === 0) {
      toast.add({ title: "没有可导出的考试记录", color: "warning" });
      return;
    }

    const csv = buildCsv(rows);
    const suffix = filter.onlyBest ? "最好成绩" : "全部记录";
    const name = `${userId.value}-考试成绩-${suffix}-${new Date()
      .toISOString()
      .slice(0, 10)}.csv`;
    downloadCsv(csv, name);

    toast.add({ title: `已导出 ${rows.length} 条记录` });
    showExportModal.value = false;
  } catch (e: any) {
    const title = e?.response?.data?.title ?? "导出失败";
    toast.add({ title: `导出失败：${title}`, color: "error" });
  } finally {
    exporting.value = false;
  }
};
</script>

<style scoped></style>
