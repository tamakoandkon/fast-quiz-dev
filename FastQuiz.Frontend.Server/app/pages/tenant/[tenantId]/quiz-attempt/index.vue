<template>
  <CommonPageLayout>
    <template #breadcrumb>
      <UBreadcrumb :items="links" />
    </template>
    <main class="flex-1 overflow-y-auto bg-gray-50">
      <QuizTemplateViewQuizTemplateDetailsModal
        :id="viewId"
        v-model="showDetail"
        :tenant-id="tenantId"
      >
      </QuizTemplateViewQuizTemplateDetailsModal>
      <div class="p-4">
        <div class="flex flex-wrap gap-4">
          <UCard
            v-for="template in templates"
            :key="template.id || ''"
            class="w-80 flex-shrink-0 cursor-pointer hover:shadow-lg transition-shadow"
          >
            <template #header>
              <div class="flex items-center justify-between">
                <h3 class="text-lg font-semibold truncate">
                  {{ template.name }}
                </h3>
              </div>
            </template>

            <div class="space-y-2">
              <!-- 详情字段 -->
              <div
                v-if="template.description"
                class="text-sm text-gray-600 dark:text-gray-400"
              >
                {{ template.description }}
              </div>

              <!-- 考试相关字段 -->
              <div class="grid grid-cols-2 gap-2 text-sm">
                <div>
                  <span class="text-xs text-gray-500">考试时长</span>
                  <div class="font-medium">
                    {{ template.durationMinutes }} 分钟
                  </div>
                </div>
              </div>

              <!-- 时间字段 -->
              <div
                class="border-t pt-2 mt-2 text-xs space-y-1"
                v-if="!template.isUnlimitedTime"
              >
                <div v-if="template.beginTime">
                  <span class="text-gray-500">开始：</span>
                  {{ formatDate(template.beginTime) }}
                </div>
                <div v-if="template.endTime">
                  <span class="text-gray-500">结束：</span>
                  {{ formatDate(template.endTime) }}
                </div>
              </div>
              <div v-else class="border-t pt-2 mt-2 text-xs space-y-1">
                <span class="text-xs text-gray-500">无限时间</span>
                <div class="font-medium">
                  {{ template.isUnlimitedTime ? "是" : "否" }}
                </div>
              </div>
            </div>

            <template #footer>
              <div class="flex justify-end gap-2">
                <UButton
                  color="primary"
                  variant="solid"
                  size="sm"
                  @click.stop="handleView(template)"
                >
                  查看详情
                </UButton>
                <UButton
                  :disabled="!isExamAvailable(template)"
                  @click="() => startAnswer(template.id || '')"
                >
                  {{ getExamStatusText(template) }}
                </UButton>
              </div>
            </template>
          </UCard>
        </div>
      </div>
    </main>
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

const templates = ref<QuizTemplateDto[]>([]);

onMounted(async () => {
  tenantId.value = route.params.tenantId as string;

  links.value.push({
    label: "考试中心",
    icon: "i-lucide-book-open",
    to: `/tenant/${tenantId.value}/quiz-attempt`,
  });

  await fectchList(false, 10, true, undefined);
});

const fectchList = async (
  isPre: boolean,
  pageSize: number,
  refresh: boolean = false,
  namePrefix: string | undefined = undefined,
) => {
  let cusor = getCursor(isPre);
  if (refresh) {
    cusor = undefined;
  }

  let result = await $api.tenant.quizTemplatePublishedList(tenantId.value, {
    Cursor: cusor,
    Count: pageSize,
  });
  templates.value = result.data;
};

const getCursor = (isPre: boolean) => {
  if (templates.value.length == 0) {
    return undefined;
  }
  if (isPre) {
    return templates.value[0]?.id as string;
  } else {
    return templates.value[templates.value.length - 1]?.id as string;
  }
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
    let title = e.response.data.title;
    toast.add({ title: `参加考试失败：${title}`, color: "error" });
    return;
  }

  router.push(
    `/tenant/${tenantId.value}/quiz-attempt/${result.data.id}/answer`,
  );
};
</script>

<style scoped></style>
