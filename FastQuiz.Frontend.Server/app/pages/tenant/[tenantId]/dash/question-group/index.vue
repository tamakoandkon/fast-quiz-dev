<template>
  <CommonPageLayout>
    <template #breadcrumb>
      <UBreadcrumb :items="links" />
    </template>

    <CommonButtonsContainer>
      <QuestionCreateQuestionGroupModal
        v-model="showCreateModal"
        @create="createGroup"
      >
      </QuestionCreateQuestionGroupModal>

      <UButton
        @click="batchDeleteGroups"
        color="error"
        :disabled="disableBatchDeleteBtn"
      >
        删除{{ selected.length }}个题库
      </UButton>

      <CommonSearchInput v-model="searchInput" @search="handleSearch">
      </CommonSearchInput>
    </CommonButtonsContainer>

    <QuestionUpdateQuestionGroupModal
      v-model="showUpdateModal"
      @update="updateGroup"
      :id="currentId"
      :tenant-id="tenantId"
    >
    </QuestionUpdateQuestionGroupModal>

    <CommonConfirmModal
      v-model="showConfiemModal"
      @action="handleDelete"
      title="是否删除题库"
    >
    </CommonConfirmModal>
    <CommonConfirmModal
      v-model="showConfirmBatchDeleteGroupsModal"
      @action="handleBatchDeleteGroup"
      title="是否删除题库"
    >
    </CommonConfirmModal>
    <UContextMenu :items="items">
      <CommonDataTable
        :columns="cols"
        :data="groups"
        :selectable="true"
        v-model="selected"
        @row-right-click="switchCurrentId"
        @row-dbl-click="
          (row) =>
            router.push(`/tenant/${tenantId}/dash/question-group/${row.id}`)
        "
      >
        <template #id="{ row }">
          <CommonTruncateText :text="row.id || ''" direction="rtl">
            <NuxtLink
              :to="`/tenant/${tenantId}/dash/question-group/${row.id}`"
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
      </CommonDataTable>
    </UContextMenu>

    <CommonCursorPageManager @change="fectchList" :current-size="groups.length">
    </CommonCursorPageManager>
  </CommonPageLayout>
</template>
<script setup lang="ts">
const { $api } = useNuxtApp();
import type { BreadcrumbItem } from "@nuxt/ui";
import type { ContextMenuItem } from "@nuxt/ui";

const route = useRoute();
const router = useRouter();
const currentId = ref("");

const links = ref<BreadcrumbItem[]>([]);

const items = ref<ContextMenuItem[][]>([
  [
    {
      label: "进入",
      icon: "i-lucide-star",
      onSelect: () =>
        router.push(
          `/tenant/${tenantId.value}/dash/question-group/${currentId.value}`,
        ),
    },
    {
      label: "编辑",
      icon: "i-lucide-edit",
      onSelect: () => (showUpdateModal.value = true),
    },
    {
      label: "删除",
      color: "error",
      icon: "i-lucide-trash",
      onSelect: () => handleDelete(),
    },
  ],
]);

const toast = useToast();

const groups = ref<QuestionGroupDto[]>([]);
const pageSize = ref(10);
const searchInput = ref("");

const selected = ref<QuestionGroupDto[]>([]);

const tenantId = ref("");

onMounted(async () => {
  tenantId.value = route.params.tenantId as string;

  links.value.push({
    label: "题库列表",
    icon: "i-lucide-book-open",
    to: `/tenant/${tenantId.value}/dash/question-group`,
  });

  await fectchList(false, pageSize.value, true);
});

// 3. 配置列 (TableColumn<UserDto>)
const cols = [
  { key: "id", label: "ID", width: "16px" },
  { key: "ownerId", label: "创建者", width: "16px" },
  { key: "name", label: "名称" },
  { key: "remark", label: "备注" },
  { key: "count", label: "题目数量" },
];

const showConfiemModal = ref(false);

const handleDelete = async () => {
  if (!currentId.value || currentId.value.trim().length == 0) {
    toast.add({ title: "ID参数异常" });
    console.log(currentId.value);
    return;
  }
  try {
    await $api.tenant.questionGroupDelete(tenantId.value, currentId.value);
  } catch (e) {
    const error = e as any;
    const title = error.response.data.title;
    toast.add({ title: "删除题库失败：" + title, color: "error" });
    return;
  }

  toast.add({ title: "题库删除成功。" });
  showConfiemModal.value = false;
  await fectchList(false, pageSize.value, true, searchInput.value);
};

const showConfirmBatchDeleteGroupsModal = ref(false);

const disableBatchDeleteBtn = computed(() => {
  return !selected.value || selected.value.length === 0;
});

const batchDeleteGroups = () => {
  showConfirmBatchDeleteGroupsModal.value = true;
};

const handleBatchDeleteGroup = async () => {
  if (!selected.value || selected.value.length === 0) {
    return;
  }

  const groupIds = selected.value
    .map((item) => item.id)
    .filter((id): id is string => typeof id === "string" && id.trim() !== "");

  if (groupIds.length === 0) {
    return;
  }
  try {
    await $api.tenant.questionGroupBatchDeleteCreate(tenantId.value, {
      groupIds,
    });
  } catch (e) {
    const error = e as any;
    const title = error.response.data.title;
    toast.add({ title: "删除题库失败：" + title, color: "error" });
    return;
  }

  // if success
  selected.value = [];
  showConfirmBatchDeleteGroupsModal.value = false;
  await fectchList(false, pageSize.value, true, searchInput.value);

  toast.add({ title: "批量删除题库成功" });
};

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
    result = await $api.tenant.questionGroupList(tenantId.value, {
      Cursor: cusor,
      Count: pageSize,
      IsPrevious: isPre,
      NamePrefix: namePrefix,
    });
  } catch (e) {
    const error = e as any;
    const title = error.response.data.title;
    toast.add({ title: "获取题库失败：" + title, color: "error" });
    return;
  }
  groups.value = result.data;
};

const getCursor = (isPre: boolean) => {
  if (groups.value.length == 0) {
    return undefined;
  }
  if (isPre) {
    return groups.value[0]?.id as string;
  } else {
    return groups.value[groups.value.length - 1]?.id as string;
  }
};

const handleSearch = async (value: string | undefined) => {
  await fectchList(false, pageSize.value, true, value);
};

const showCreateModal = ref(false);
const createGroup = async (command: CreateQuestionGroupCommand) => {
  try {
    await $api.tenant.questionGroupCreate(tenantId.value, {
      name: command.name,
      remark: command.remark,
    });
  } catch (e) {
    const error = e as any;
    const title = error.response.data.title;
    toast.add({ title: "创建题库失败：" + title, color: "error" });
    return;
  }

  toast.add({ title: "题库创建成功。" });

  // if success
  showCreateModal.value = false;
  await fectchList(false, pageSize.value, true, searchInput.value);
};

const showUpdateModal = ref(false);

const updateGroup = async (command: UpdateQuestionGroupCommand) => {
  try {
    await $api.tenant.questionGroupUpdate(tenantId.value, currentId.value, {
      name: command.name,
      remark: command.remark,
    });
  } catch (e) {
    const error = e as any;
    const title = error.response.data.title;
    toast.add({ title: "修改题库失败：" + title, color: "error" });
    return;
  }

  toast.add({ title: "题库修改成功。" });

  // if success
  showUpdateModal.value = false;

  await fectchList(false, pageSize.value, true, searchInput.value);
};

const switchCurrentId = (row: QuestionGroupDto) => {
  currentId.value = row.id || "";
};
</script>
