<template>
  <CommonPageLayout>
    <template #breadcrumb>
      <UBreadcrumb :items="links" />
    </template>

    <main class="flex-1 min-w-0">
      <!-- 试卷详情模态 -->
      <LazyQuizTemplateViewQuizTemplateDetailsModal
        v-if="showDetail"
        :id="viewId"
        v-model="showDetail"
        :tenant-id="tenantId"
      />

      <div class="space-y-4">
        <!-- 考试行：每个考试占一整行 -->
        <UCard
          v-for="template in templates"
          :key="template.id || ''"
          class="w-full"
          :ui="{ body: 'p-0 sm:p-0' }"
        >
          <template #header>
            <div class="flex flex-wrap items-center gap-x-4 gap-y-2">
              <h3 class="text-lg font-semibold truncate">
                {{ template.name }}
              </h3>

              <span class="text-sm text-gray-500">
                考试时长 {{ template.durationMinutes }} 分钟
              </span>

              <UBadge v-if="template.isUnlimitedTime" color="neutral" size="sm">
                无限时间
              </UBadge>
              <span v-else class="text-xs text-gray-500">
                <template v-if="template.beginTime">
                  开始：{{ formatDate(template.beginTime) }}
                </template>
                <template v-if="template.endTime">
                  ｜结束：{{ formatDate(template.endTime) }}
                </template>
              </span>

              <div class="ml-auto flex flex-wrap items-center justify-end gap-2">
                <UButton
                  color="neutral"
                  variant="outline"
                  size="sm"
                  @click="handleView(template)"
                >
                  查看详情
                </UButton>
                <UButton
                  color="neutral"
                  variant="soft"
                  size="sm"
                  :disabled="!isExamAvailable(template) || starting"
                  :loading="startingId === template.id && claiming"
                  @click="startAnswer(template.id || '', true)"
                >领取预生成试卷</UButton>
                <UButton
                  color="primary"
                  variant="solid"
                  size="sm"
                  :disabled="!isExamAvailable(template) || starting"
                  :loading="startingId === template.id && !claiming"
                  @click="startAnswer(template.id || '')"
                >
                  {{ getExamStatusText(template) }}
                </UButton>
              </div>
            </div>
          </template>

          <!-- 描述（有才显示） -->
          <div
            v-if="template.description"
            class="px-6 py-3 text-sm text-gray-600 dark:text-gray-400"
          >
            {{ template.description }}
          </div>
        </UCard>

        <!-- 无已发布考试 -->
        <div
          v-if="loaded && templates.length === 0"
          class="workspace-card py-16 text-center text-muted"
        >
          当前没有可参加的考试。
        </div>
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
import type { BreadcrumbItem } from "@nuxt/ui";

const toast = useToast();

const links = ref<BreadcrumbItem[]>([]);

const router = useRouter();
const route = useRoute();
const tenantId = ref("");

const pageSize = ref(10);

const templates = ref<QuizTemplateDto[]>([]);
const haveMore = ref(true);
const loaded = ref(false);
/** 下一页游标；追加模式下用「最后一条的 id」，不依赖数组增长。 */
const nextCursor = ref<string | undefined>(undefined);

onMounted(async () => {
  tenantId.value = route.params.tenantId as string;

  links.value.push({
    label: "考试中心",
    icon: "i-lucide-book-open",
    to: `/tenant/${tenantId.value}/quiz-attempt`,
  });

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
    const title = e?.response?.data?.title ?? "获取试卷失败";
    toast.add({ title: `获取试卷出错了：${title}`, color: "error" });
    return;
  }

  const batch = result.data ?? [];

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
  // 无限时间始终可开始
  if (template.isUnlimitedTime) return true;

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
  if (template.isUnlimitedTime) return "开始考试";

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
