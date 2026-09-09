<template>
  <CommonPageLayout>
    <template #breadcrumb>
      <UBreadcrumb :items="links" class="py-2" />
    </template>

    <CommonButtonsContainer>
      <QuizTemplateCreateQuizTemplateModal
        v-model="showCreateModal"
        @create="createQuizTemplate"
        :orgs="orgs"
      >
      </QuizTemplateCreateQuizTemplateModal>
    </CommonButtonsContainer>

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
    >
    </CommonConfirmModal>

    <UContextMenu :items="items">
      <CommonDataTable
        :columns="cols"
        :data="templates"
        :selectable="true"
        v-model="selected"
        @rowDblClick="handleRowDblClick"
        @row-right-click="(row) => switchCurrentId(row)"
      >
        <template #id="{ row }">
          <CommonTruncateText :text="row.id || ''" direction="rtl">
            <NuxtLink
              :to="`/tenant/${tenantId}/dash/quiz-template/${row.id}`"
              class="text-primary-600 hover:text-primary-700 hover:underline cursor-pointer"
            >
              {{ row.id }}
            </NuxtLink>
          </CommonTruncateText>
        </template>
        <template #ownerId="{ row }">
          <CommonTruncateText :text="row.ownerId || ''" direction="rtl">
          </CommonTruncateText>
        </template>
        <template #name="{ row }">
          <CommonTruncateText :text="row.name || ''" width="10rem">
          </CommonTruncateText>
        </template>
        <template #remark="{ row }">
          <CommonTruncateText :text="row.remark || ''" width="10rem">
          </CommonTruncateText>
        </template>

        <template #beginTime="{ row }">
          {{ row.isUnlimitedTime ? "-" : formatDate(row.beginTime) }}
        </template>

        <template #endTime="{ row }">
          {{ row.isUnlimitedTime ? "-" : formatDate(row.endTime) }}
        </template>

        <template #createdAt="{ row }">
          {{ formatDate(row.createdAt) }}
        </template>

        <template #updatedAt="{ row }">
          {{ formatDate(row.updatedAt) }}
        </template>

        <template #isUnlimitedTime="{ row }">
          {{ row.isUnlimitedTime ? "是" : "否" }}
        </template>

        <template #isPublished="{ row }">
          {{ row.isPublished ? "是" : "否" }}
        </template>

        <template #durationMinutes="{ row }">
          {{
            row.durationMinutes
              ? row.durationMinutes > 0
                ? row.durationMinutes
                : "无限"
              : "-"
          }}
        </template>
      </CommonDataTable>
    </UContextMenu>

    <CommonCursorPageManager
      @change="fectchList"
      :current-size="templates.length"
    >
    </CommonCursorPageManager>
  </CommonPageLayout>
</template>

<script setup lang="ts">
const { $api } = useNuxtApp();
import type { BreadcrumbItem } from "@nuxt/ui";
import type { ContextMenuItem } from "@nuxt/ui";

const toast = useToast();
const router = useRouter();
const route = useRoute();

const templates = ref<QuizTemplateDto[]>([]);
const selected = ref<QuizTemplateDto[]>([]);

const pageSize = ref(10);

const currentId = ref("");

const cols = [
  { key: "id", label: "ID", width: "16px" },
  { key: "name", label: "名称" },
  { key: "remark", label: "备注" },
  { key: "attemptLimit", label: "考试次数" },
  { key: "isUnlimitedTime", label: "是否无限时间" },
  { key: "beginTime", label: "开始时间" },
  { key: "endTime", label: "结束时间" },
  { key: "durationMinutes", label: "考试时长（分钟）" },
  { key: "ownerId", label: "所有者" },
  { key: "isPublished", label: "是否发布" },
  { key: "createdAt", label: "创建于" },
  { key: "updatedAt", label: "更新于" },
];

const items = ref<ContextMenuItem[][]>([
  [
    {
      label: "进入",
      icon: "i-lucide-star",
      onSelect: () =>
        router.push(
          `/tenant/${tenantId.value}/dash/quiz-template/${currentId.value}`,
        ),
    },
    {
      label: "编辑",
      icon: "i-lucide-edit",
      onSelect: () => handleEdit(),
    },
    {
      label: "删除",
      color: "error",
      icon: "i-lucide-trash",
      onSelect: () => handleDelete(),
    },
  ],
]);

const switchCurrentId = (template: QuizTemplateDto) => {
  currentId.value = template.id || "";
};

const tenantId = ref("");

const links = ref<BreadcrumbItem[]>([]);

onMounted(async () => {
  tenantId.value = route.params.tenantId as string;

  links.value.push({
    label: "试卷管理",
    icon: "iconoir:atom",
    to: `/tenant/${tenantId.value}/dash/quiz-template`,
  });

  await fectchList(false, 10, true, undefined);
  await loadAllOrgs();
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
  let result;
  try {
    result = await $api.tenant.quizTemplateList(tenantId.value, {
      Cursor: cusor,
      Count: pageSize,
      IsPrevious: isPre,
    });
  } catch (e: any) {
    let title = e.response.data.title;
    toast.add({ title: "获取试卷模板失败：" + title, color: "error" });
    return;
  }

  if (result.data.length === 0) {
    toast.add({ title: "没有更多了", color: "warning" });
    return;
  }

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

const showCreateModal = ref(false);
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
    let title = e.response.data.title;
    toast.add({ title: "创建试卷模板失败：" + title, color: "error" });
    return;
  }

  toast.add({ title: "试卷模板创建成功。" });
  showCreateModal.value = false;
  await fectchList(false, pageSize.value, true, undefined);
};

const showUpdateModal = ref(false);

const handleEdit = () => {
  showUpdateModal.value = true;
};

const updateQuizTemplateAsync = async (command: UpdateQuizTemplateCommand) => {
  let result;
  try {
    result = await $api.tenant.quizTemplateUpdate(
      tenantId.value,
      currentId.value,
      command,
    );
  } catch (e: any) {
    let title = e.response.data.title;
    toast.add({ title: "修改试卷模板失败：" + title, color: "error" });
    return;
  }

  toast.add({ title: "试卷模板修改成功。" });
  showUpdateModal.value = false;
  await fectchList(false, pageSize.value, true, undefined);
};

const showConfiemModal = ref(false);

const handleDelete = async () => {
  if (!currentId.value || currentId.value.trim().length == 0) {
    toast.add({ title: "ID参数异常" });
    console.log(currentId.value);
    return;
  }

  await $api.tenant.quizTemplateDelete(tenantId.value, currentId.value);
  toast.add({ title: "试卷模板删除成功。" });
  showConfiemModal.value = false;
  await fectchList(false, pageSize.value, true, undefined);
};

const handleRowDblClick = (row: QuizTemplateDto) => {
  if (row.id) {
    router.push(`/tenant/${tenantId.value}/dash/quiz-template/${row.id}`);
  }
};
</script>

<style scoped></style>
