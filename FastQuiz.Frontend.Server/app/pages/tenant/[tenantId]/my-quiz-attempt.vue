<template>
  <CommonPageLayout>
    <main class="min-w-0">
      <!-- 试卷详情模态 -->
      <QuizTemplateViewQuizTemplateDetailsModal
        :id="viewId"
        v-model="showDetail"
        :tenant-id="tenantId"
      />

      <div class="exam-list" :aria-busy="loadingMore">
        <template v-if="!loaded">
          <div
            v-for="n in 3"
            :key="n"
            class="exam-skeleton-row"
            aria-hidden="true"
          >
            <div class="exam-skeleton-main">
              <span class="skeleton-line"></span>
              <span class="skeleton-line is-short is-small"></span>
            </div>
            <span class="skeleton-pill"></span>
          </div>
        </template>

        <template v-for="template in templates" :key="template.id || ''">
          <article class="exam-row">
            <div class="exam-main">
              <h2 class="exam-title-line">
                <button
                  type="button"
                  class="exam-title-button"
                  :aria-expanded="isExpanded(template)"
                  @click="toggleExpand(template)"
                >
                  <span>{{ template.name }}</span>
                  <UIcon
                    class="exam-title-chevron"
                    :name="isExpanded(template) ? 'i-lucide-chevron-down' : 'i-lucide-chevron-right'"
                  />
                </button>
              </h2>

              <div class="exam-meta">
                <span>{{ template.isUnlimitedTime ? '不限时' : `${template.durationMinutes ?? 0} 分钟` }}</span>
                <span v-if="template.beginTime">{{ formatDateTime(template.beginTime) }} 开始</span>
                <span v-if="template.endTime">{{ formatDateTime(template.endTime) }} 截止</span>
              </div>

              <p v-if="template.description" class="exam-desc">{{ template.description }}</p>
            </div>

            <div class="exam-actions">
              <UButton color="neutral" variant="ghost" size="sm" @click="handleView(template)">详情</UButton>
              <!-- 有进行中的记录时给出「继续考试」，否则「开始考试」 -->
              <UButton
                v-if="ongoingAttemptOf(template)"
                color="warning"
                variant="soft"
                size="sm"
                @click="resumeAttempt(ongoingAttemptOf(template)!)"
              >
                继续考试
              </UButton>
              <UButton
                v-else
                color="primary"
                size="sm"
                :disabled="!isExamAvailable(template)"
                @click="startAnswer(template.id || '')"
              >
                {{ getExamStatusText(template) }}
              </UButton>
            </div>
          </article>

          <!-- 展开区：该考试下的考试记录（独立分页） -->
          <div v-if="isExpanded(template)" class="exam-records">
            <div
              v-if="recordState(template).loading && !recordsOf(template).length"
              class="exam-records-empty"
              role="status"
            >
              正在加载考试记录…
            </div>
            <CommonDataTable
              v-else-if="recordsOf(template).length"
              :columns="recordCols"
              :data="recordsOf(template)"
              :selectable="false"
            >
              <template #status="{ row }">
                <UBadge
                  :color="getStatusColor(row.status)"
                  size="sm"
                  variant="subtle"
                >
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
                  {{ row.status === QuizAttemptStatus.Graded ? (row.score ?? "--") : "--" }}
                </span>
              </template>

              <template #actions="{ row }">
                <UButton
                  v-if="
                    row.status === QuizAttemptStatus.InProgress ||
                    row.status === QuizAttemptStatus.Pending
                  "
                  color="warning"
                  variant="outline"
                  size="xs"
                  @click="resumeAttempt(row)"
                >
                  继续考试
                </UButton>
                <UButton
                  v-else-if="row.status === QuizAttemptStatus.Graded"
                  color="neutral"
                  variant="outline"
                  size="xs"
                  icon="i-lucide-eye"
                  :to="`/tenant/${tenantId}/quiz-attempt/${row.id}/result`"
                >
                  查看答案
                </UButton>
                <span v-else class="text-xs text-gray-400">—</span>
              </template>
            </CommonDataTable>

            <!-- 该模板无记录 -->
            <div
              v-if="!recordState(template).loading && recordsOf(template).length === 0"
              class="exam-records-empty"
            >
              该考试暂无你的考试记录。
            </div>

            <!-- 记录分页（上一页 / 下一页，各自独立） -->
            <div
              v-if="recordsOf(template).length > 0 || recordState(template).page > 0"
              class="exam-records-footer"
            >
              <span class="text-xs">
                第 {{ recordState(template).page + 1 }} 页 · 每页 {{ recordPageSize }} 条
              </span>
              <div class="flex items-center gap-2">
                <UButton
                  color="neutral"
                  variant="outline"
                  size="xs"
                  icon="i-lucide-chevron-left"
                  :disabled="recordState(template).page === 0 || recordState(template).loading"
                  @click="fetchRecords(template, true)"
                >
                  上一页
                </UButton>
                <UButton
                  color="neutral"
                  variant="outline"
                  size="xs"
                  trailing-icon="i-lucide-chevron-right"
                  :disabled="!recordState(template).haveMore || recordState(template).loading"
                  @click="fetchRecords(template, false)"
                >
                  下一页
                </UButton>
              </div>
            </div>
          </div>
        </template>

        <div v-if="loaded && templates.length === 0" class="exam-empty" role="status">当前没有可参加的考试。</div>
      </div>

      <!-- 滚动到底自动续页，替代常驻按钮 -->
      <CommonListSentinel
        :loading="loadingMore"
        :end="loaded && !haveMore"
        :end-text="`已显示全部 ${templates.length} 份考试`"
        :disabled="!loaded || !haveMore"
        @reach="loadMore"
      />
    </main>
  </CommonPageLayout>
</template>

<script setup lang="ts">
const { $api } = useNuxtApp();

const router = useRouter();
const route = useRoute();
const toast = useToast();
const userStore = useUserStore();

const tenantId = ref("");
const pageSize = ref(10);
const recordPageSize = 5;

// ---------------- 考试模板列表（外部分页 + 加载更多追加） ----------------

const templates = ref<QuizTemplateDto[]>([]);
const haveMore = ref(true);
const loaded = ref(false);
const loadingMore = ref(false);
/** 是否还有下一页；用「最后一条的 id」作游标，追加模式下不会因数组增长而错位。 */
const nextCursor = ref<string | undefined>(undefined);

onMounted(async () => {
  tenantId.value = route.params.tenantId as string;

  await fetchTemplates(true);
  // 首屏把第一页每个模板的「进行中」状态探出来，行首按钮才能显示「继续考试」
  await probeOngoingForAll();
  loaded.value = true;
});

/** 为当前已加载的所有模板补齐「进行中」判定（并发探测，互不阻塞）。
 *  探测在命中 InProgress 或遇到短页时即返回，通常只有 1 个请求。 */
const probeOngoingForAll = async () => {
  await Promise.all(templates.value.map((t) => probeOngoing(t)));
};

/**
 * 拉取模板列表。refresh=true 时清空重建（第一页），否则按游标追加。
 */
const fetchTemplates = async (refresh: boolean = false) => {
  const cursor = refresh ? undefined : nextCursor.value;

  let result;
  try {
    result = await $api.tenant.quizTemplatePublishedList(tenantId.value, {
      Cursor: cursor,
      Count: pageSize.value,
    });
  } catch (e: any) {
    const title = e?.response?.data?.title ?? "获取试卷失败";
    toast.add({ title: `获取试卷出错了：${title}`, color: "error" });
    return;
  }

  const batch = result.data ?? [];

  if (refresh) {
    templates.value = batch;
    // 清掉上一轮的展开态与缓存记录
    expandedId.value = null;
    recordStore.value = {};
  } else {
    // 追加：过滤掉已存在的 id，避免重复键
    const known = new Set(templates.value.map((t) => t.id));
    templates.value = [
      ...templates.value,
      ...batch.filter((t) => !known.has(t.id)),
    ];
  }

  // 还有下一页的判据：本批拿满了 pageSize
  haveMore.value = batch.length === pageSize.value;
  nextCursor.value = batch.length
    ? (batch[batch.length - 1]?.id as string)
    : undefined;
};

/** 触底续页：追加下一页（追加后为新模板补「进行中」探测） */
const loadMore = async () => {
  if (!loaded.value || !haveMore.value || loadingMore.value) return;
  loadingMore.value = true;
  try {
    await fetchTemplates(false);
    await probeOngoingForAll();
  } finally {
    loadingMore.value = false;
  }
};

// ---------------- 展开态（同一时间只展开一个模板） ----------------

const expandedId = ref<string | null>(null);

const isExpanded = (template: QuizTemplateDto) =>
  !!template.id && expandedId.value === template.id;

const toggleExpand = async (template: QuizTemplateDto) => {
  const id = template.id || "";
  if (!id) return;

  if (expandedId.value === id) {
    expandedId.value = null;
    return;
  }

  expandedId.value = id;
  // 首次展开才拉记录
  if (!recordStore.value[id]) {
    await fetchRecords(template, false, true);
  }
  // 「继续考试」的判定独立于分页表格
  await probeOngoing(template);
};

// ---------------- 展开区：该模板下的考试记录（独立分页） ----------------

interface RecordState {
  rows: QuizAttempDto[];
  /** 当前页码（0 基，仅用于展示与「上一页」可用性） */
  page: number;
  haveMore: boolean;
  loading: boolean;
  loadedOnce: boolean;
}

const recordStore = ref<Record<string, RecordState>>({});

const emptyState = (): RecordState => ({
  rows: [],
  page: 0,
  haveMore: false,
  loading: false,
  loadedOnce: false,
});

const recordState = (template: QuizTemplateDto): RecordState => {
  const id = template.id || "";
  if (!recordStore.value[id]) {
    recordStore.value[id] = emptyState();
  }
  return recordStore.value[id]!;
};

const recordsOf = (template: QuizTemplateDto) => recordState(template).rows;

/**
 * 拉取某模板下的记录。isPrevious=true 时用当前页首条作游标往回翻。
 */
const fetchRecords = async (
  template: QuizTemplateDto,
  isPrevious: boolean,
  refresh: boolean = false,
) => {
  const tplId = template.id || "";
  if (!tplId) return;

  const state = recordState(template);
  if (state.loading) return;

  state.loading = true;

  let cursor: string | undefined;
  if (isPrevious) {
    cursor = state.rows[0]?.id as string;
  } else if (!refresh) {
    cursor = state.rows[state.rows.length - 1]?.id as string;
  }

  let result;
  try {
    result = await $api.tenant.quizTemplateQuizAttemptMeTemplateDetail(
      tenantId.value,
      tplId,
      {
        Cursor: refresh ? undefined : cursor,
        Count: recordPageSize,
        IsPrevious: isPrevious,
      },
    );
  } catch (e: any) {
    const title = e?.response?.data?.title ?? "获取考试记录失败";
    toast.add({ title: `获取考试记录失败：${title}`, color: "error" });
    state.loading = false;
    return;
  }

  const batch = result.data ?? [];
  state.rows = batch;
  state.haveMore = !isPrevious && batch.length === recordPageSize;
  if (isPrevious) {
    state.page = Math.max(0, state.page - 1);
  } else if (!refresh) {
    state.page += 1;
  } else {
    state.page = 0;
  }
  state.loadedOnce = true;
  state.loading = false;
};

// ---------------- 「继续考试」判定 ----------------

/**
 * 各模板下「进行中」的记录，与分页表格完全解耦。
 * 不能把它塞进 state.rows —— 那样会破坏分页游标(rows 首/尾必须始终是当前页的真实边界)。
 */
const ongoingStore = ref<Record<string, QuizAttempDto | null>>({});

/** 该模板下是否有进行中的记录（用于把按钮换成「继续考试」）。 */
const ongoingAttemptOf = (template: QuizTemplateDto) =>
  ongoingStore.value[template.id || ""] ?? null;

/**
 * 扫描该模板的全部分页，找出「进行中」的记录。
 * 结果写进 ongoingStore，不动分页状态。
 */
const probeOngoing = async (template: QuizTemplateDto) => {
  const tplId = template.id || "";
  if (!tplId) return;

  let cursor: string | undefined = undefined;
  ongoingStore.value[tplId] = null;

  // 防御性上限，避免游标异常导致死循环
  for (let i = 0; i < 100; i++) {
    let result;
    try {
      result = await $api.tenant.quizTemplateQuizAttemptMeTemplateDetail(
        tenantId.value,
        tplId,
        { Cursor: cursor, Count: 100, IsPrevious: false },
      );
    } catch {
      return;
    }

    const batch = result.data ?? [];
    if (batch.length === 0) return;

    const hit = batch.find(
      (r) => r.status === QuizAttemptStatus.InProgress,
    );
    if (hit) {
      ongoingStore.value[tplId] = hit;
      return;
    }

    if (batch.length < 100) return;
    cursor = batch[batch.length - 1]?.id as string;
  }
};

/** 继续答题。 */
const resumeAttempt = async (attempt: QuizAttempDto) => {
  // Pending 需要先切到 InProgress 才能读题
  if (attempt.status === QuizAttemptStatus.Pending) {
    try {
      await $api.tenant.quizTemplateQuizAttemptUpdate(
        tenantId.value,
        attempt.id as string,
        { status: QuizAttemptStatus.InProgress },
      );
    } catch (e: any) {
      const title = e?.response?.data?.title ?? "继续考试失败";
      toast.add({ title: `继续考试失败：${title}`, color: "error" });
      return;
    }
  }
  router.push(`/tenant/${tenantId.value}/quiz-attempt/${attempt.id}/answer`);
};

// ---------------- 开始新的考试 ----------------

const isExamAvailable = (template: QuizTemplateDto) => {
  if (template.isUnlimitedTime) return true;

  const now = new Date();
  const beginTime = template.beginTime ? new Date(template.beginTime) : null;
  const endTime = template.endTime ? new Date(template.endTime) : null;

  if (beginTime && now < beginTime) return false;
  if (endTime && now > endTime) return false;

  return true;
};

const getExamStatusText = (template: QuizTemplateDto) => {
  if (template.isUnlimitedTime) return "开始考试";

  const now = new Date();
  const beginTime = template.beginTime ? new Date(template.beginTime) : null;
  const endTime = template.endTime ? new Date(template.endTime) : null;

  if (beginTime && now < beginTime) return "考试未开始";
  if (endTime && now > endTime) return "考试已结束";

  return "开始考试";
};

const viewId = ref("");
const showDetail = ref(false);
const handleView = (template: QuizTemplateDto) => {
  viewId.value = template.id ?? "";
  showDetail.value = true;
};

const startAnswer = async (templateId: string) => {
  let result;
  try {
    result = await $api.tenant.quizTemplateQuizAttemptCreate(
      tenantId.value,
      templateId,
    );

    await $api.tenant.quizTemplateQuizAttemptUpdate(
      tenantId.value,
      result.data.id as string,
      { status: QuizAttemptStatus.InProgress },
    );
  } catch (e: any) {
    const title = e?.response?.data?.title ?? "参加考试失败";
    toast.add({ title: `参加考试失败：${title}`, color: "error" });
    return;
  }

  router.push(`/tenant/${tenantId.value}/quiz-attempt/${result.data.id}/answer`);
};

// ---------------- 记录表列与格式化 ----------------

const recordCols = [
  { key: "id", label: "考试ID", width: "260px" },
  { key: "status", label: "状态", width: "110px" },
  { key: "startTime", label: "开始时间" },
  { key: "submitTime", label: "交卷时间" },
  { key: "score", label: "得分", width: "100px" },
  { key: "actions", label: "操作", width: "140px" },
];

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

const formatDateTime = (iso?: string | null): string =>
  iso ? formatDate(iso, "YYYY-MM-DD HH:mm") : "--";
</script>

<style scoped></style>
