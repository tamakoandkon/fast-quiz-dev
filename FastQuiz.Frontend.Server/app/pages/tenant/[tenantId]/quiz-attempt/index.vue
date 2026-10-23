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

      <div class="exam-list">
        <article
          v-for="template in templates"
          :key="template.id || ''"
          class="exam-row"
        >
          <div class="exam-main">
            <h2>{{ template.name }}</h2>
            <div class="exam-meta">
              <span>{{ template.isUnlimitedTime ? '不限时' : `${template.durationMinutes ?? 0} 分钟` }}</span>
              <span v-if="template.beginTime">{{ formatDate(template.beginTime) }} 开始</span>
              <span v-if="template.endTime">{{ formatDate(template.endTime) }} 截止</span>
            </div>
          </div>
          <div class="exam-actions">
            <UButton color="neutral" variant="ghost" size="sm" @click="handleView(template)">详情</UButton>
            <UButton color="neutral" variant="outline" size="sm" :disabled="!isExamAvailable(template) || starting" :loading="startingId === template.id && claiming" @click="startAnswer(template.id || '', true)">领取试卷</UButton>
            <UButton color="primary" size="sm" :disabled="!isExamAvailable(template) || starting" :loading="startingId === template.id && !claiming" @click="startAnswer(template.id || '')">{{ getExamStatusText(template) }}</UButton>
          </div>
        </article>

        <div v-if="!loaded" class="exam-empty" role="status">正在加载考试…</div>
        <div v-else-if="loadError && templates.length === 0" class="exam-empty">
          <p>考试列表暂时无法加载。</p>
          <UButton color="neutral" variant="outline" size="sm" @click="fetchTemplates(true)">重试</UButton>
        </div>
        <div v-else-if="templates.length === 0" class="exam-empty">当前没有可参加的考试。</div>
      </div>
    </main>

    <!-- 加载更多：追加，而非替换 -->
    <CommonLoadMoreButton
      v-if="haveMore"
      @click="loadMore"
    ></CommonLoadMoreButton>
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

/** 「加载更多」：追加下一页 */
const loadMore = async () => {
  if (!haveMore.value) return;
  await fetchTemplates(false);
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

/** 时间展示，与「我的考试」页保持一致。 */
const formatDate = (iso?: string | null): string => {
  if (!iso) return "--";
  const d = new Date(iso);
  return Number.isNaN(d.getTime())
    ? "--"
    : d.toLocaleString("zh-CN", { hour12: false });
};
</script>

<style scoped></style>
