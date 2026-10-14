<template>
  <CommonManagePageContainer>
    <UBreadcrumb :items="links" class="py-2" />
    <USeparator class="my-2" />

    <!-- 标题与筛选 -->
    <div v-if="loaded" class="mb-4 flex flex-wrap items-center gap-3">
      <h2 class="text-xl font-semibold">{{ template.name || "未命名试卷" }}</h2>
      <UBadge v-if="!template.isUnlimitedTime" color="info" size="sm">
        限时：{{ formatDateTime(template.beginTime) }} ~
        {{ formatDateTime(template.endTime) }}
      </UBadge>
      <UBadge v-else color="neutral" size="sm">不限时</UBadge>

      <div class="ml-auto flex flex-wrap items-center gap-3">
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

    <QuizAttemptResultsTable :records="records" :tenant-id="tenantId" :can-review="can('quiz-attempt')" show-assignee />

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
      当前考试暂无考试记录。
    </div>

    <QuizAttemptExportQuizRecordsModal
      v-model="showExportModal"
      v-model:loading="exporting"
      @export="handleExport"
    />
  </CommonManagePageContainer>
</template>

<script setup lang="ts">
import type { BreadcrumbItem } from "@nuxt/ui";

const { $api } = useNuxtApp();
const route = useRoute();
const toast = useToast();
const { can } = useWorkspaceAccess();

const tenantId = ref("");
const templateId = ref("");

const links = ref<BreadcrumbItem[]>([]);
const template = ref<QuizTemplateDto>({});
const records = ref<QuizAttempDto[]>([]);
const loaded = ref(false);

// 列表筛选：是否只取每个考生最好的一次成绩
const onlyBest = ref(false);

const pageSize = ref(10);

onMounted(async () => {
  tenantId.value = route.params.tenantId as string;
  templateId.value = route.params.templateId as string;

  try {
    const tpl = await $api.tenant.quizTemplateDetail(
      tenantId.value,
      templateId.value,
    );
    template.value = tpl.data;
  } catch (e: any) {
    const title = e?.response?.data?.title ?? "获取试卷失败";
    toast.add({ title: `获取试卷失败：${title}`, color: "error" });
  }

  links.value.push({
    label: "试卷管理",
    icon: "i-lucide-book-open",
    to: `/tenant/${tenantId.value}/dash/quiz-template`,
  });
  links.value.push({
    label: template.value.name ?? templateId.value,
    to: `/tenant/${tenantId.value}/dash/quiz-template/${templateId.value}`,
  });
  links.value.push({ label: "查看成绩" });

  await fetchList(false, pageSize.value, true);
  loaded.value = true;
});

/** 切换筛选后重新拉取第一页。 */
watch(onlyBest, () => {
  fetchList(false, pageSize.value, true);
});

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
      return "success";
    case QuizAttemptStatus.Submitted:
      return "warning";
    case QuizAttemptStatus.InProgress:
      return "info";
    case QuizAttemptStatus.Pending:
      return "neutral";
    default:
      return "neutral";
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
    const result = await $api.tenant.quizTemplateQuizAttemptRecordsList(
      tenantId.value,
      templateId.value,
      {
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

export interface ExportRecordsFilter {
  onlyBest: boolean;
}

/** 拉取全部记录：按游标自动翻页，直到拉空。 */
const fetchAllRecords = async (best: boolean): Promise<QuizAttempDto[]> => {
  const all: QuizAttempDto[] = [];
  let cursor: string | undefined = undefined;
  const size = 100;

  // 防御性上限，避免服务端游标异常导致死循环
  for (let page = 0; page < 1000; page++) {
    const result = await $api.tenant.quizTemplateQuizAttemptRecordsList(
      tenantId.value,
      templateId.value,
      {
        OnlyBest: best,
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
const handleExport = async (filter: ExportRecordsFilter) => {
  if (exporting.value) return;
  exporting.value = true;
  try {
    const rows = await fetchAllRecords(filter.onlyBest);
    if (rows.length === 0) {
      toast.add({ title: "没有可导出的考试记录", color: "warning" });
      return;
    }

    const csv = buildCsv(rows);
    const suffix = filter.onlyBest ? "最好成绩" : "全部记录";
    const name = `${template.value.name || "考试"}-${suffix}-${new Date()
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
