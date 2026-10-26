<template>
  <CommonPageLayout>
    <template #actions>
      <QuizTemplateQuickPublishQuizTemplateModal
        v-model="showQuickPublishModal"
        :tenant-id="tenantId"
        :orgs="orgs"
        @published="handleQuickPublished"
      >
      </QuizTemplateQuickPublishQuizTemplateModal>

      <QuizTemplateCreateQuizTemplateModal
        v-model="showCreateModal"
        @create="createQuizTemplate"
        :orgs="orgs"
      >
      </QuizTemplateCreateQuizTemplateModal>
    </template>

    <QuizTemplateUpdateQuizTemplateModal
      v-model="showUpdateModal"
      :id="currentId"
      @update="updateQuizTemplateAsync"
      :tenant-id="tenantId"
      :orgs="orgs"
    >
    </QuizTemplateUpdateQuizTemplateModal>

    <CommonConfirmModal
      v-model="showConfiemModal"
      @action="handleDelete"
      title="是否删除试卷模板"
      descript="删除后无法恢复，已有答卷不受影响。"
    >
    </CommonConfirmModal>

    <section
      class="data-list"
      :class="{ 'is-busy': refreshing }"
      :aria-busy="refreshing"
    >
      <div
        v-if="loading"
        class="data-list-skeleton"
        role="status"
        aria-label="正在加载试卷模板"
      >
        <div v-for="n in 4" :key="n" class="data-list-skeleton-row">
          <div class="data-list-main data-list-skeleton-main">
            <span class="skeleton-line"></span>
            <span class="skeleton-line is-short is-small"></span>
          </div>
          <span class="data-list-status-cell skeleton-pill"></span>
          <div class="data-list-meta data-list-skeleton-main">
            <span class="skeleton-line is-short"></span>
            <span class="skeleton-line is-short is-small"></span>
          </div>
          <span class="data-list-aside skeleton-line is-short"></span>
          <span class="data-list-actions skeleton-line is-short"></span>
        </div>
      </div>

      <div v-else-if="loadError && !templates.length" class="data-list-state">
        <UIcon name="i-lucide-cloud-off" aria-hidden="true" />
        <p>试卷列表暂时无法加载。</p>
        <p class="data-list-state-hint">检查网络连接后重试。</p>
        <UButton
          color="neutral"
          variant="outline"
          size="sm"
          icon="i-lucide-refresh-cw"
          @click="fetchList(false, pageSize, true)"
        >
          重新加载
        </UButton>
      </div>

      <div v-else-if="!templates.length" class="data-list-state">
        <UIcon name="i-lucide-file-text" aria-hidden="true" />
        <p>还没有试卷模板。</p>
        <p class="data-list-state-hint">
          用「新增试卷模板」创建第一份试卷，或直接快速发布。
        </p>
      </div>

      <template v-else>
        <div class="data-list-table" role="table" aria-label="试卷模板列表">
          <div class="data-list-head" role="row">
            <span role="columnheader">试卷</span>
            <span role="columnheader">状态</span>
            <span role="columnheader">考试安排</span>
            <span role="columnheader">所有者</span>
            <span role="columnheader">操作</span>
          </div>

          <div class="data-list-body" role="rowgroup">
            <article
              v-for="(row, index) in templates"
              :key="row.id || index"
              class="data-list-row"
              role="row"
            >
              <div class="data-list-main" role="cell">
                <NuxtLink
                  :to="`/tenant/${tenantId}/dash/quiz-template/${row.id}`"
                  class="data-list-title"
                  :title="row.id ? `ID：${row.id}` : undefined"
                >
                  {{ row.name || "未命名试卷" }}
                </NuxtLink>
                <p v-if="row.remark" class="data-list-note" :title="row.remark">
                  {{ row.remark }}
                </p>
              </div>

              <div class="data-list-status-cell" role="cell">
                <span
                  class="data-status"
                  :class="{ 'is-published': row.isPublished }"
                >
                  {{ row.isPublished ? "已发布" : "未发布" }}
                </span>
              </div>

              <div class="data-list-meta" role="cell">
                <span
                  class="data-list-meta-main"
                  :title="scheduleTitle(row)"
                >
                  {{ scheduleLabel(row) }}
                </span>
                <span class="data-list-meta-sub">
                  {{ durationLabel(row) }} · {{ attemptLabel(row) }}
                </span>
              </div>

              <span
                class="data-list-aside"
                role="cell"
                :title="row.ownerId || '未记录所有者'"
              >
                <span class="data-list-aside-label">所有者</span>
                <span class="data-list-aside-value">{{ row.ownerId || "—" }}</span>
              </span>

              <div class="data-list-actions" role="cell">
                <UButton
                  class="data-list-edit"
                  color="neutral"
                  variant="ghost"
                  size="sm"
                  icon="i-lucide-pen-line"
                  @click="handleEdit(row)"
                >
                  编辑
                </UButton>
                <UDropdownMenu
                  :items="rowMenu(row)"
                  :content="{ align: 'end' }"
                >
                  <UButton
                    class="data-list-kebab"
                    color="neutral"
                    variant="ghost"
                    size="sm"
                    icon="i-lucide-ellipsis"
                    aria-label="更多操作"
                  />
                </UDropdownMenu>
              </div>
            </article>
          </div>
        </div>

        <CommonListFooter
          :page-index="pageIndex"
          :page-size="pageSize"
          :item-count="templates.length"
          :size-options="pageSizeOptions"
          unit="份"
          :can-prev="canPrev"
          :can-next="canNext"
          :busy="refreshing"
          @prev="goPrev"
          @next="goNext"
          @update:page-size="onPageSize"
        />
      </template>
    </section>
  </CommonPageLayout>
</template>

<script setup lang="ts">
import type { DropdownMenuItem } from "@nuxt/ui";

const { $api } = useNuxtApp();

const toast = useToast();
const router = useRouter();
const route = useRoute();

const tenantId = ref("");
const templates = ref<QuizTemplateDto[]>([]);
const loading = ref(true);
const refreshing = ref(false);
const loadError = ref(false);

const pageSize = ref(10);
const pageSizeOptions = [10, 20, 50];
const pageIndex = ref(0);

const canPrev = computed(() => pageIndex.value > 0 && !refreshing.value);
const canNext = computed(
  () => templates.value.length === pageSize.value && !refreshing.value,
);

const currentId = ref("");

onMounted(async () => {
  tenantId.value = route.params.tenantId as string;
  await Promise.all([
    fetchList(false, pageSize.value, true),
    loadAllOrgs(),
  ]);
});

const fetchList = async (
  isPre: boolean,
  size: number,
  refresh: boolean = false,
) => {
  if (refresh) {
    pageIndex.value = 0;
  }
  const cursor = refresh ? undefined : getCursor(isPre);

  if (!templates.value.length) loading.value = true;
  else refreshing.value = true;
  loadError.value = false;

  try {
    const result = await $api.tenant.quizTemplateList(tenantId.value, {
      Cursor: cursor,
      Count: size,
      IsPrevious: isPre,
    });
    const batch = result.data ?? [];

    // 游标分页没有总数。空页只说明当前方向到头了，保留现有行。
    if (!refresh && batch.length === 0) {
      toast.add({ title: "没有更多试卷了", color: "neutral" });
      return false;
    }

    templates.value = batch;
    return true;
  } catch (e: any) {
    loadError.value = true;
    if (!templates.value.length) templates.value = [];
    const title = e?.response?.data?.title ?? "请稍后重试";
    toast.add({ title: `获取试卷模板失败：${title}`, color: "error" });
    return false;
  } finally {
    loading.value = false;
    refreshing.value = false;
  }
};

const getCursor = (isPre: boolean) => {
  if (templates.value.length === 0) return undefined;
  return isPre
    ? (templates.value[0]?.id as string)
    : (templates.value[templates.value.length - 1]?.id as string);
};

const goPrev = async () => {
  if (!canPrev.value) return;
  if (await fetchList(true, pageSize.value, false)) {
    pageIndex.value = Math.max(0, pageIndex.value - 1);
  }
};

const goNext = async () => {
  if (!canNext.value) return;
  if (await fetchList(false, pageSize.value, false)) {
    pageIndex.value += 1;
  }
};

const onPageSize = async (next: number) => {
  pageSize.value = next;
  pageIndex.value = 0;
  await fetchList(false, next, true);
};

const formatScheduleDate = (value?: string | null) =>
  value ? formatDate(value, "YYYY-MM-DD") : "";
const formatScheduleClock = (value?: string | null) =>
  value ? formatDate(value, "HH:mm") : "";

const scheduleLabel = (row: QuizTemplateDto) => {
  if (row.isUnlimitedTime) return "长期有效";
  if (!row.beginTime || !row.endTime) return "时间待设置";

  const beginDate = formatScheduleDate(row.beginTime);
  const endDate = formatScheduleDate(row.endTime);
  const begin = `${beginDate} ${formatScheduleClock(row.beginTime)}`;
  const endClock = formatScheduleClock(row.endTime);

  return beginDate === endDate
    ? `${begin} – ${endClock}`
    : `${begin} – ${endDate} ${endClock}`;
};

const scheduleTitle = (row: QuizTemplateDto) => {
  if (row.isUnlimitedTime) return "长期有效，不限制开始与结束时间";
  if (!row.beginTime || !row.endTime) return "尚未设置考试时间";
  return `${formatDate(row.beginTime, "YYYY-MM-DD HH:mm")} 至 ${formatDate(
    row.endTime,
    "YYYY-MM-DD HH:mm",
  )}`;
};

const durationLabel = (row: QuizTemplateDto) =>
  row.durationMinutes && row.durationMinutes > 0
    ? `${row.durationMinutes} 分钟`
    : "不限时长";

const attemptLabel = (row: QuizTemplateDto) =>
  row.attemptLimit && row.attemptLimit > 0
    ? `${row.attemptLimit} 次`
    : "不限次数";

const openTemplate = (row: QuizTemplateDto) => {
  if (row.id) {
    router.push(`/tenant/${tenantId.value}/dash/quiz-template/${row.id}`);
  }
};

const handleEdit = (row: QuizTemplateDto) => {
  currentId.value = row.id || "";
  showUpdateModal.value = true;
};

const confirmDelete = (row: QuizTemplateDto) => {
  currentId.value = row.id || "";
  showConfiemModal.value = true;
};

const copyId = async (row: QuizTemplateDto) => {
  if (!row.id) return;
  try {
    await navigator.clipboard.writeText(row.id);
    toast.add({ title: "试卷 ID 已复制" });
  } catch {
    toast.add({ title: "复制失败，请到详情页手动复制", color: "error" });
  }
};

const rowMenu = (row: QuizTemplateDto): DropdownMenuItem[][] => [
  [
    {
      label: "查看详情",
      icon: "i-lucide-arrow-up-right",
      onSelect: () => openTemplate(row),
    },
    {
      label: "编辑",
      icon: "i-lucide-pen-line",
      onSelect: () => handleEdit(row),
    },
    {
      label: "复制 ID",
      icon: "i-lucide-copy",
      onSelect: () => copyId(row),
    },
  ],
  [
    {
      label: "删除",
      color: "error",
      icon: "i-lucide-trash-2",
      onSelect: () => confirmDelete(row),
    },
  ],
];

const showCreateModal = ref(false);
const showQuickPublishModal = ref(false);
const showUpdateModal = ref(false);
const showConfiemModal = ref(false);

const orgs = ref<OrganizationDto[]>([]);
// 加载所有题库分组（不分页，加载全部）
const loadAllOrgs = async () => {
  try {
    let allItems: OrganizationDto[] = [];
    let cursor: string | undefined = undefined;
    let hasMore = true;

    // 循环加载所有分组，直到没有更多数据
    while (hasMore) {
      const result = await $api.tenant.organizationList(tenantId.value, {
        Cursor: cursor,
        Count: 200,
        IsPrevious: false,
      });

      if (result.data && result.data.length > 0) {
        allItems = [...allItems, ...result.data];
        if (result.data.length < 200) {
          hasMore = false;
        } else {
          cursor = result.data[result.data.length - 1]?.id ?? undefined;
        }
      } else {
        hasMore = false;
      }
    }

    orgs.value = allItems;
  } catch (error) {
    console.error("加载题库分组失败:", error);
    orgs.value = [];
  }
};

const createQuizTemplate = async (command: CreateQuizTemplateCommand) => {
  try {
    await $api.tenant.quizTemplateCreate(tenantId.value, command);
  } catch (e: any) {
    const title = e?.response?.data?.title ?? "请稍后重试";
    toast.add({ title: `创建试卷模板失败：${title}`, color: "error" });
    return;
  }

  toast.add({ title: "试卷模板创建成功。" });
  showCreateModal.value = false;
  await fetchList(false, pageSize.value, true);
};

// 快速发布模态框自己完成全部接口调用，页面只负责刷新列表
const handleQuickPublished = async (_templateId: string) => {
  await fetchList(false, pageSize.value, true);
};

const updateQuizTemplateAsync = async (command: UpdateQuizTemplateCommand) => {
  try {
    await $api.tenant.quizTemplateUpdate(
      tenantId.value,
      currentId.value,
      command,
    );
  } catch (e: any) {
    const title = e?.response?.data?.title ?? "请稍后重试";
    toast.add({ title: `修改试卷模板失败：${title}`, color: "error" });
    return;
  }

  toast.add({ title: "试卷模板修改成功。" });
  showUpdateModal.value = false;
  await fetchList(false, pageSize.value, true);
};

const handleDelete = async () => {
  if (!currentId.value || currentId.value.trim().length === 0) {
    toast.add({ title: "ID 参数异常", color: "error" });
    return;
  }

  try {
    await $api.tenant.quizTemplateDelete(tenantId.value, currentId.value);
  } catch (e: any) {
    const title = e?.response?.data?.title ?? "请稍后重试";
    toast.add({ title: `删除试卷模板失败：${title}`, color: "error" });
    return;
  }

  toast.add({ title: "试卷模板删除成功。" });
  showConfiemModal.value = false;
  await fetchList(false, pageSize.value, true);
};
</script>

<style scoped></style>
