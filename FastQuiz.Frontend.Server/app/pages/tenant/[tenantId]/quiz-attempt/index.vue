<template>
  <CommonPageLayout>
    <main class="flex-1 min-w-0">
      <!-- 试卷详情模态 -->
      <LazyQuizTemplateViewQuizTemplateDetailsModal
        v-if="showDetail"
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

        <template v-else-if="templates.length">
          <article
            v-for="template in templates"
            :key="template.id || ''"
            class="exam-row"
          >
            <div class="exam-main">
              <h2>{{ template.name }}</h2>
              <div class="exam-meta">
                <span>{{ template.isUnlimitedTime ? '不限时' : `${template.durationMinutes ?? 0} 分钟` }}</span>
                <span v-if="template.beginTime">{{ examMoment(template.beginTime) }} 开始</span>
                <span v-if="template.endTime">{{ examMoment(template.endTime) }} 截止</span>
              </div>
              <p v-if="template.description" class="exam-desc" :title="template.description">
                {{ template.description }}
              </p>
            </div>
            <div class="exam-actions">
              <UButton color="neutral" variant="ghost" size="sm" @click="handleView(template)">详情</UButton>
              <UButton color="neutral" variant="ghost" size="sm" :disabled="!isExamAvailable(template) || starting" :loading="startingId === template.id && claiming" @click="startAnswer(template.id || '', true)">领取试卷</UButton>
              <UButton color="primary" size="sm" :disabled="!isExamAvailable(template) || starting" :loading="startingId === template.id && !claiming" @click="startAnswer(template.id || '')">{{ getExamStatusText(template) }}</UButton>
            </div>
          </article>
        </template>

        <div v-else-if="loadError" class="exam-empty" role="status">
          <p>考试列表暂时无法加载。</p>
          <UButton color="neutral" variant="outline" size="sm" icon="i-lucide-refresh-cw" @click="fetchTemplates(true)">重新加载</UButton>
        </div>

        <div v-else class="exam-empty" role="status">当前没有可参加的考试。</div>
      </div>
    </main>

    <!-- 滚动到底自动续页，替代常驻按钮 -->
    <CommonListSentinel
      :loading="loadingMore"
      :end="loaded && !haveMore"
      :end-text="`已显示全部 ${templates.length} 份考试`"
      :disabled="!loaded || !haveMore || !!loadError"
      @reach="loadMore"
    />
  </CommonPageLayout>
</template>

<script setup lang="ts">
const { $api } = useNuxtApp();
const toast = useToast();

const router = useRouter();
const route = useRoute();
const tenantId = ref("");

const pageSize = ref(10);

const templates = ref<QuizTemplateDto[]>([]);
const haveMore = ref(true);
const loaded = ref(false);
const loadingMore = ref(false);
const loadError = ref(false);
/** 下一页游标；追加模式下用「最后一条的 id」，不依赖数组增长。 */
const nextCursor = ref<string | undefined>(undefined);

onMounted(async () => {
  tenantId.value = route.params.tenantId as string;

  await fetchTemplates(true);
  loaded.value = true;
});

/**
 * 拉取考试列表。refresh=true 时清空重建（第一页），否则按游标追加。
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
    loadError.value = true;
    const title = e?.response?.data?.title ?? "获取试卷失败";
    toast.add({ title: `获取试卷出错了：${title}`, color: "error" });
    return;
  }

  const batch = result.data ?? [];
  loadError.value = false;

  if (refresh) {
    templates.value = batch;
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

/** 触底续页：追加而非替换，失败时保留已有内容。 */
const loadMore = async () => {
  if (!loaded.value || !haveMore.value || loadingMore.value || loadError.value) return;
  loadingMore.value = true;
  try {
    await fetchTemplates(false);
  } finally {
    loadingMore.value = false;
  }
};

// 新增：检查考试是否可开始
const isExamAvailable = (template: QuizTemplateDto) => {
  const now = new Date();
  const beginTime = template.beginTime ? new Date(template.beginTime) : null;
  const endTime = template.endTime ? new Date(template.endTime) : null;

  // 如果有开始时间且当前时间小于开始时间，不可开始
  if (beginTime && now < beginTime) return false;

  // 如果有结束时间且当前时间大于结束时间，不可开始
  if (endTime && now > endTime) return false;

  return true;
};

// 新增：获取考试状态文本
const getExamStatusText = (template: QuizTemplateDto) => {
  const now = new Date();
  const beginTime = template.beginTime ? new Date(template.beginTime) : null;
  const endTime = template.endTime ? new Date(template.endTime) : null;

  if (beginTime && now < beginTime) {
    return "考试未开始";
  }

  if (endTime && now > endTime) {
    return "考试已结束";
  }

  return "开始考试";
};

const viewId = ref("");
const showDetail = ref(false);
// 查看详情处理函数
const handleView = (template: QuizTemplateDto) => {
  viewId.value = template.id ?? "";
  showDetail.value = true;
};

const starting = ref(false);
const startingId = ref("");
const claiming = ref(false);
const startAnswer = async (templateId: string, claim = false) => {
  if (starting.value) return;
  starting.value = true;
  startingId.value = templateId;
  claiming.value = claim;
  let result;
  try {
    result = claim
      ? await $api.tenant.quizTemplateQuizAttemptClaimCreate(tenantId.value, templateId)
      : await $api.tenant.quizTemplateQuizAttemptCreate(tenantId.value, templateId);

    await $api.tenant.quizTemplateQuizAttemptUpdate(
      tenantId.value,
      result.data.id as string,
      { status: QuizAttemptStatus.InProgress },
    );
  } catch (e: any) {
    const title = e?.response?.data?.title ?? "参加考试失败";
    toast.add({ title: `参加考试失败：${title}`, color: "error" });
    return;
  } finally {
    starting.value = false;
    startingId.value = "";
  }

  router.push(`/tenant/${tenantId.value}/quiz-attempt/${result.data.id}/answer`);
};

/** 时间只到分钟，避免秒与斜杠把一行元信息撑得很技术化。 */
const examMoment = (iso?: string | null): string =>
  iso ? formatDate(iso, "YYYY-MM-DD HH:mm") : "--";
</script>
