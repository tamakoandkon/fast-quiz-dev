<template>
  <CommonPageLayout>
    <template #breadcrumb>
      <UBreadcrumb :items="links" />
    </template>

    <main class="flex-1 overflow-y-auto bg-gray-50">
      <div class="p-4">
        <div class="flex flex-wrap gap-4">
          <UCard
            v-for="attempt in attempts"
            :key="attempt.id || ''"
            class="w-80 flex-shrink-0 transition-shadow"
            :class="[
              isClickable(attempt.status ?? QuizAttemptStatus.Pending)
                ? 'cursor-pointer hover:shadow-lg'
                : 'cursor-not-allowed opacity-80',
            ]"
            @click="handleCardClick(attempt)"
          >
            <template #header>
              <div class="flex items-center justify-between">
                <h3 class="text-lg font-semibold truncate">
                  {{ attempt.quizTemplateNema }}
                </h3>
              </div>
            </template>

            <div class="space-y-2">
              <div class="text-sm text-gray-500">考试ID：{{ attempt.id }}</div>
              <!-- 已批阅显示得分 -->
              <div
                v-if="attempt.status === QuizAttemptStatus.Graded"
                class="text-sm"
              >
                <span class="text-gray-500">得分：</span>
                <span class="font-semibold text-green-600">
                  {{ attempt.score ?? "--" }}
                </span>
              </div>
            </div>

            <template #footer>
              <div class="flex justify-end">
                <UButton
                  color="primary"
                  variant="solid"
                  size="md"
                  class="w-full"
                  :disabled="
                    !isClickable(attempt.status ?? QuizAttemptStatus.Pending)
                  "
                  :class="
                    !isClickable(attempt.status ?? QuizAttemptStatus.Pending)
                      ? 'cursor-not-allowed'
                      : ''
                  "
                  @click.stop="handleAction(attempt)"
                >
                  {{ getButtonText(attempt.status) }}
                </UButton>
              </div>
            </template>
          </UCard>
        </div>
      </div>
    </main>
    <CommonLoadMoreButton
      v-if="haveMore"
      @click="() => fetchList(pageSize, false)"
    ></CommonLoadMoreButton>
  </CommonPageLayout>
</template>

<script setup lang="ts">
const { $api } = useNuxtApp();
import type { BreadcrumbItem } from "@nuxt/ui";

const links = ref<BreadcrumbItem[]>([]);

const router = useRouter();
const route = useRoute();
const toast = useToast();
const tenantId = ref("");
const pageSize = ref(20);

const attempts = ref<QuizAttempDto[]>([]);

onMounted(async () => {
  tenantId.value = route.params.tenantId as string;

  links.value.push({
    label: "我的考试",
    icon: "i-lucide-book-open",
    to: `/tenant/${tenantId.value}/my-quiz-attempt`,
  });

  await fetchList(pageSize.value, true);
});

const haveMore = ref(true);

const fetchList = async (pageSize: number, refresh: boolean = false) => {
  if (!haveMore.value) {
    return;
  }

  let cursor = getCursor(false);
  if (refresh) {
    cursor = undefined;
  }

  let result;
  try {
    result = await $api.tenant.quizTemplateQuizAttemptMeList(tenantId.value, {
      Cursor: cursor,
      Count: pageSize,
    });
  } catch (e: any) {
    let title = e.response.data.title;
    toast.add({ title: `获取试卷出错了：${title}`, color: "error" });
    return;
  }

  attempts.value = result.data;

  if (result.data.length != pageSize) {
    haveMore.value = false;
    return;
  }
};

const getCursor = (isPre: boolean) => {
  if (attempts.value.length == 0) {
    return undefined;
  }
  if (isPre) {
    return attempts.value[0]?.id as string;
  } else {
    return attempts.value[attempts.value.length - 1]?.id as string;
  }
};

// ✅ 修复：明确参数类型为 QuizAttemptStatus | undefined
const isClickable = (status: QuizAttemptStatus) => {
  return (
    status === QuizAttemptStatus.InProgress ||
    status === QuizAttemptStatus.Graded
  );
};

// ✅ 修复：明确参数类型为 QuizAttemptStatus | undefined
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

// ✅ 修复：明确参数类型为 QuizAttemptStatus | undefined
const getStatusColor = (status?: QuizAttemptStatus) => {
  switch (status) {
    case QuizAttemptStatus.Pending:
      return "gray";
    case QuizAttemptStatus.InProgress:
      return "blue";
    case QuizAttemptStatus.Submitted:
      return "yellow";
    case QuizAttemptStatus.Graded:
      return "green";
    default:
      return "gray";
  }
};

// ✅ 修复：明确参数类型为 QuizAttemptStatus | undefined
const getButtonText = (status?: QuizAttemptStatus) => {
  switch (status) {
    case QuizAttemptStatus.Pending:
      return "未开始";
    case QuizAttemptStatus.InProgress:
      return "继续考试";
    case QuizAttemptStatus.Submitted:
      return "待批阅";
    case QuizAttemptStatus.Graded:
      return "查看答案";
    default:
      return "查看";
  }
};

// 卡片点击处理
const handleCardClick = async (attempt: QuizAttempDto) => {
  const status = (attempt.status ??
    QuizAttemptStatus.Pending) as QuizAttemptStatus;

  if (status == QuizAttemptStatus.Pending) {
    await $api.tenant.quizTemplateQuizAttemptUpdate(
      tenantId.value,
      attempt.id as string,
      { status: QuizAttemptStatus.InProgress },
    );
    router.push(`/tenant/${tenantId.value}/quiz-attempt/${attempt.id}/answer`);
    return;
  }

  if (status === QuizAttemptStatus.InProgress) {
    router.push(`/tenant/${tenantId.value}/quiz-attempt/${attempt.id}/answer`);
  } else if (status === QuizAttemptStatus.Graded) {
    router.push(`/tenant/${tenantId.value}/quiz-attempt/${attempt.id}/result`);
  }
};

// 按钮点击处理
const handleAction = async (attempt: QuizAttempDto) => {
  const status = (attempt.status ??
    QuizAttemptStatus.Pending) as QuizAttemptStatus;

  if (status == QuizAttemptStatus.Pending) {
    await $api.tenant.quizTemplateQuizAttemptUpdate(
      tenantId.value,
      attempt.id as string,
      { status: QuizAttemptStatus.InProgress },
    );
    router.push(`/tenant/${tenantId.value}/quiz-attempt/${attempt.id}/answer`);
    return;
  }

  if (status === QuizAttemptStatus.InProgress) {
    router.push(`/tenant/${tenantId.value}/quiz-attempt/${attempt.id}/answer`);
  } else if (status === QuizAttemptStatus.Graded) {
    router.push(`/tenant/${tenantId.value}/quiz-attempt/${attempt.id}/result`);
  }
  // Pending 和 Submitted 按钮被禁用，不会触发
};
</script>

<style scoped></style>
