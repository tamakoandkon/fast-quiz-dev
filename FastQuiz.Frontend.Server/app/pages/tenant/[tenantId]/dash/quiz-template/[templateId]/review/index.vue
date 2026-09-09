<template>
  <CommonManagePageContainer>
    <UBreadcrumb :items="links" class="py-2" />
    <USeparator class="my-2" />

    <!-- 阅卷入口提示 -->
    <div
      v-if="loaded && !isReadyToOpen"
      class="mb-4 rounded-lg px-4 py-3 text-sm text-amber-700 bg-amber-50 border border-amber-200 flex items-center gap-2"
    >
      <UIcon name="i-lucide-clock" />
      <span>
        本场考试为限时试卷，考试结束（
        {{ template.endTime ? formatDateTime(template.endTime) : "--" }}
        ）后方可开始批阅（#2）。当前交卷的作答也会在考试结束后才计入待批队列。
      </span>
    </div>

    <div v-if="loaded" class="mb-4 flex flex-wrap items-center gap-3">
      <h2 class="text-xl font-semibold">{{ template.name || "未命名试卷" }}</h2>
      <UBadge v-if="!template.isUnlimitedTime" color="info" size="sm">
        限时：{{ formatDateTime(template.beginTime) }} ~
        {{ formatDateTime(template.endTime) }}
      </UBadge>
      <UBadge v-else color="neutral" size="sm">不限时</UBadge>

      <div class="ml-auto flex items-center gap-2">
        <span class="text-sm text-gray-500">
          待批：{{ pendingCount }}
        </span>
        <UButton
          v-if="isReadyToOpen && pendingCount > 0"
          color="primary"
          icon="i-lucide-play"
          @click="startFirstReview"
        >
          按交卷顺序批改最早一份
        </UButton>
      </div>
    </div>

    <CommonDataTable
      :columns="cols"
      :data="attempts"
      :selectable="false"
    >
      <template #id="{ row }">
        <NuxtLink
          :to="`/tenant/${tenantId}/dash/quiz-template/${templateId}/review/${row.id}`"
          class="text-primary-600 hover:text-primary-700 hover:underline cursor-pointer"
        >
          {{ row.id ? displayId(row.id) : "--" }}
        </NuxtLink>
      </template>

      <template #status="{ row }">
        <UBadge :color="getStatusColor(row.status)" size="sm" variant="subtle">
          {{ getStatusText(row.status) }}
        </UBadge>
        <span
          v-if="needsReview(row)"
          class="text-red-500 text-xs ml-1"
        >· 待批</span>
      </template>

      <template #submitTime="{ row }">
        {{ row.submitTime ? formatDateTime(row.submitTime) : "--" }}
      </template>

      <template #score="{ row }">
        <span
          :class="
            row.status === QuizAttemptStatus.Graded ? 'text-green-600' : ''
          "
        >
          {{ row.score ?? "--" }}
        </span>
      </template>

      <template #actions="{ row }">
        <div class="flex flex-wrap gap-1">
          <!-- 可批改的卷（限时已结束 或 不限时，且已提交） -->
          <UButton
            v-if="needsReview(row)"
            :to="`/tenant/${tenantId}/dash/quiz-template/${templateId}/review/${row.id}`"
            color="primary"
            size="xs"
            icon="i-lucide-pen-line"
          >
            阅卷
          </UButton>
          <!-- 其它状态：只读查看 -->
          <UButton
            v-else
            :to="`/tenant/${tenantId}/dash/quiz-template/${templateId}/review/${row.id}`"
            color="neutral"
            variant="outline"
            size="xs"
            icon="i-lucide-eye"
          >
            查看
          </UButton>
        </div>
      </template>
    </CommonDataTable>

    <CommonCursorPageManager
      @change="fectchList"
      :current-size="attempts.length"
    />

    <!-- 空状态（整场没有需批的卷） -->
    <div
      v-if="loaded && attempts.length === 0"
      class="mt-6 bg-white rounded-lg shadow p-12 text-center text-gray-500"
    >
      {{ isReadyToOpen ? "当前没有待批阅的试卷。" : "考试尚未结束，暂不能开始批改。" }}
    </div>
  </CommonManagePageContainer>
</template>

<script setup lang="ts">
import type { BreadcrumbItem } from "@nuxt/ui";

const { $api } = useNuxtApp();
const router = useRouter();
const route = useRoute();
const toast = useToast();

const tenantId = ref("");
const templateId = ref("");

const links = ref<BreadcrumbItem[]>([]);
const template = ref<QuizTemplateDto>({});
const attempts = ref<QuizAttempDto[]>([]);
const loaded = ref(false);

const pageSize = 10;

const cols = [
  { key: "id", label: "考试 / 作答", width: "200px" },
  { key: "status", label: "状态" },
  { key: "submitTime", label: "交卷时间" },
  { key: "score", label: "得分" },
  { key: "actions", label: "操作", width: "160px" },
];

onMounted(async () => {
  tenantId.value = route.params.tenantId as string;
  templateId.value = route.params.templateId as string;

  // 先取模板，以便用考试时间窗判断是否可阅
  try {
    const tpl = await $api.tenant.quizTemplateDetail(tenantId.value, templateId.value);
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
  links.value.push({ label: "批改试卷" });

  await fectchList(false, pageSize);
  loaded.value = true;
});

const isReadyToOpen = computed(() => canStartReviewing(template.value));

/** 是否已进入“待批”状态（限时：考试结束 + 已提交；不限时：仅已提交）。 */
const needsReview = (row: QuizAttempDto): boolean =>
  isAttemptNeedsReview(row, template.value);

const pendingCount = computed(() => attempts.value.filter(needsReview).length);

const displayId = (id: string) => id.slice(0, 8);

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

const fectchList = async (
  isPre: boolean,
  pageSize: number,
  refresh: boolean = false,
) => {
  let cursor = getCursor(isPre);
  if (refresh) cursor = undefined;

  try {
    const result = await $api.tenant.quizTemplateQuizAttemptList(
      tenantId.value,
      templateId.value,
      { Cursor: cursor, Count: pageSize, IsPrevious: isPre },
    );
    attempts.value = result.data ?? [];
  } catch (e: any) {
    const title = e?.response?.data?.title ?? "获取列表失败";
    toast.add({ title: `获取待批列表失败：${title}`, color: "error" });
  }
};

const getCursor = (isPre: boolean) => {
  if (attempts.value.length === 0) return undefined;
  return isPre
    ? (attempts.value[0]?.id as string)
    : (attempts.value[attempts.value.length - 1]?.id as string);
};

/** 跳到按交卷先后最早的一份待批卷。 */
const startFirstReview = async () => {
  // 扩大拉取一页（取 100），从中挑最早交卷的一份
  let reviewable: QuizAttempDto[] = [];
  try {
    if (
      attempts.value.filter(needsReview).length === 0 ||
      attempts.value.length < pageSize
    ) {
      // 可能还有其它页：扩大拉一次以覆盖
      const result = await $api.tenant.quizTemplateQuizAttemptList(
        tenantId.value,
        templateId.value,
        { Cursor: undefined, Count: 100, IsPrevious: false },
      );
      reviewable = (result.data ?? []).filter(needsReview);
    } else {
      reviewable = attempts.value.filter(needsReview);
    }
  } catch (e: any) {
    const title = e?.response?.data?.title ?? "拉取失败";
    toast.add({ title: `获取待批列表失败：${title}`, color: "error" });
    return;
  }

  const first = reviewable
    .slice()
    .sort((a, b) => (a.submitTime ?? "").localeCompare(b.submitTime ?? ""))[0];
  if (!first?.id) {
    toast.add({ title: "当前没有可批改的试卷", color: "info" });
    return;
  }
  router.push(
    `/tenant/${tenantId.value}/dash/quiz-template/${templateId.value}/review/${first.id}`,
  );
};
</script>

<style scoped></style>
