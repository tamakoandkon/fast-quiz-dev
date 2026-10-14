<template>
  <CommonManagePageContainer>
    <UBreadcrumb :items="links" class="py-2" />
    <USeparator class="my-2" />
    <CommonButtonsContainer>
      <TenantCreateTenantModal
        v-model="showCreateModal"
        @create="handleCreateTenant"
      />
    </CommonButtonsContainer>
    <UContextMenu :items="items">
      <CommonDataTable
        :columns="cols"
        :data="tenants"
        :selectable="true"
        v-model="selected"
        @row-right-click="(row) => switchCurrentId(row)"
      >
        <template #id="{ row }">
          <CommonTruncateText :text="row.id || ''" direction="rtl">
            {{ row.id }}
          </CommonTruncateText>
        </template>

        <template #name="{ row }">
          <CommonTruncateText :text="row.name || ''" width="10rem">
          </CommonTruncateText>
        </template>
        <template #createdAt="{ row }">
          {{ formatDate(row.createdAt) }}
        </template>
        <template #updatedAt="{ row }">
          {{ formatDate(row.updatedAt) }}
        </template>
        <template #actions="{ row }">
          <UButton color="neutral" variant="soft" size="sm" icon="i-lucide-user-plus"
            @click="addMember(row)">添加已有账号</UButton>
        </template>
      </CommonDataTable>
    </UContextMenu>
    <CommonCursorPageManager
      @change="fectchList"
      :current-size="tenants.length"
    >
    </CommonCursorPageManager>
    <TenantAddExistingMemberModal v-model="showAddMember" :tenant-id="memberTenant?.id || ''"
      :tenant-name="memberTenant?.name || ''" />
    <TenantUpdateTenantModal
      v-model="showEditModal"
      @update="handleUpdate"
      :id="currentId"
    />
    <CommonConfirmModal
      title="确认删除"
      :descript="deleteDescription"
      v-model="showConfirDelete"
      @action="handleDelete"
    ></CommonConfirmModal>
  </CommonManagePageContainer>
</template>

<script setup lang="ts">
definePageMeta({ layout: "tenant" });
const { $api } = useNuxtApp();
import type { BreadcrumbItem } from "@nuxt/ui";
import type { ContextMenuItem } from "@nuxt/ui";

const items = ref<ContextMenuItem[][]>([
  [
    {
      label: "编辑",
      icon: "i-lucide-edit",
      onSelect: () => handleEdit(),
    },
    {
      label: "删除",
      color: "error",
      icon: "i-lucide-trash",
      onSelect: () => {
        showConfirDelete.value = true;
      },
    },
  ],
]);
const links = ref<BreadcrumbItem[]>([
  {
    label: "租户管理",
    icon: "i-lucide-book-open",
    to: "/dash/tenant",
  },
]);

const toast = useToast();

const tenants = ref<TenantDto[]>([]);
const pageSize = ref(10);

const selected = ref<TenantDto[]>([]);
const showAddMember = ref(false);
const memberTenant = ref<TenantDto>();
const addMember = (tenant: TenantDto) => {
  memberTenant.value = tenant;
  showAddMember.value = true;
};

onMounted(async () => {
  await fectchList(false, pageSize.value, true);
});

// 3. 配置列 (TableColumn<UserDto>)
const cols = [
  { key: "id", label: "ID", width: "16px" },
  { key: "name", label: "名称" },
  { key: "description", label: "详情" },
  { key: "createdAt", label: "创建时间" },
  { key: "updatedAt", label: "更新时间" },
  { key: "actions", label: "成员管理" },
];

const showCreateModal = ref(false);

const handleCreateTenant = async (command: CreateTenantCommand) => {
  let result;
  try {
    result = await $api.tenant.tenantCreate(command);
  } catch (e) {
    const error = e as any;
    const title = error.response.data.title;
    toast.add({ title: "新增租户失败：" + title });
    return;
  }

  toast.add({ title: "新增租户成功" });
  showCreateModal.value = false;
  await fectchList(false, pageSize.value, true);
};

const currentId = ref("");
const currentItem = computed(
  () => tenants.value.filter((x) => x.id === currentId.value)[0],
);

const switchCurrentId = (row: TenantDto) => {
  currentId.value = row.id || "";
};

const fectchList = async (
  isPre: boolean,
  pageSize: number,
  refresh: boolean = false,
) => {
  let cusor = getCursor(isPre);
  if (refresh) {
    cusor = undefined;
  }
  let result;
  try {
    result = await $api.tenant.tenantList({
      Cursor: cusor,
      Count: pageSize,
      IsPrevious: isPre,
    });
  } catch (e) {
    const error = e as any;
    const title = error.response.data.title;
    toast.add({ title: "获取租户列表失败：" + title });
    return;
  }

  tenants.value = result.data;
};

const getCursor = (isPre: boolean) => {
  if (tenants.value.length == 0) {
    return undefined;
  }
  if (isPre) {
    return tenants.value[0]?.id as string;
  } else {
    return tenants.value[tenants.value.length - 1]?.id as string;
  }
};

const showEditModal = ref(false);

const handleEdit = () => {
  showEditModal.value = true;
};

const handleUpdate = async (command: UpdateTenantCommand) => {
  let result;
  try {
    result = await $api.tenant.tenantUpdate(currentId.value, command);
  } catch (e) {
    const error = e as any;
    const title = error.response.data.title;
    toast.add({ title: "修改租户失败：" + title });
    return;
  }

  toast.add({ title: "修改租户成功" });
  showEditModal.value = false;
  await fectchList(false, pageSize.value, true);
};

const showConfirDelete = ref(false);
const deleteDescription = computed(
  () =>
    `是否删除ID: ${currentId.value}, 名称: ${currentItem.value?.name || "-"} 的租户？`,
);

const handleDelete = async () => {
  try {
    await $api.tenant.tenantDelete(currentId.value);
  } catch (e: any) {
    const title = e.response.data.title;
    toast.add({ title: "删除租户失败：" + title, color: "error" });
    return;
  }

  toast.add({ title: "删除租户成功" });
  showConfirDelete.value = false;
  await fectchList(false, pageSize.value, true);
};
</script>
