<template>
  <CommonManagePageContainer>
    <CommonButtonsContainer>
      <RoleCreateRoleModal
        v-model="showCreateModal"
        @create="handleCreateOrg"
      />
    </CommonButtonsContainer>
    <UContextMenu :items="items">
      <CommonDataTable
        :columns="cols"
        :data="roles"
        :selectable="true"
        v-model="selected"
        @row-right-click="(row) => switchCurrentId(row)"
      >
        <template #id="{ row }">
          <CommonTruncateText :text="row.id || ''" direction="rtl">
            <NuxtLink
              :to="`/dash/question-group/${row.id}`"
              class="text-primary-600 hover:text-primary-700 hover:underline cursor-pointer"
            >
              {{ row.id }}
            </NuxtLink>
          </CommonTruncateText>
        </template>

        <template #name="{ row }">
          <CommonTruncateText :text="row.name || ''" width="10rem">
          </CommonTruncateText>
        </template>

        <template #isEnable="{ row }">
          {{ row.isEnable ? "启用" : "禁用" }}
        </template>

        <template #createdAt="{ row }">
          {{ formatDate(row.createdAt) }}
        </template>

        <template #updatedAt="{ row }">
          {{ formatDate(row.updatedAt) }}
        </template>
      </CommonDataTable>
    </UContextMenu>
    <CommonCursorPageManager @change="fetchList" :current-size="roles.length">
    </CommonCursorPageManager>
    <RoleUpdateRoleModal
      v-model="showEditModal"
      @update="handleUpdate"
      :id="currentId"
    />
  </CommonManagePageContainer>
</template>

<script setup lang="ts">
const { $api } = useNuxtApp();
import type { ContextMenuItem } from "@nuxt/ui";
const route = useRoute();

const tenantId = ref("");

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
      onSelect: () => handleDelete(),
    },
  ],
]);

const toast = useToast();

const roles = ref<RoleDto[]>([]);
const pageSize = ref(10);

const selected = ref<RoleDto[]>([]);

onMounted(async () => {
  tenantId.value = route.params.tenantId as string;
  await fetchList(false, pageSize.value, true);
});

// 3. 配置列 (TableColumn<UserDto>)
const cols = [
  { key: "id", label: "ID", width: "16px" },
  { key: "name", label: "角色名" },
  { key: "description", label: "详情" },
  { key: "isEnable", label: "启用状态" },
  { key: "createdAt", label: "创建时间" },
  { key: "updatedAt", label: "更新时间" },
];

const showCreateModal = ref(false);

const handleCreateOrg = async (command: CreateRoleCommand) => {
  let result;
  try {
    result = await $api.tenant.roleCreate(tenantId.value, command);
  } catch (e) {
    const error = e as any;
    const title = error.response.data.title;
    toast.add({ title: "新增角色失败：" + title });
    return;
  }

  toast.add({ title: "新增角色成功" });
  showCreateModal.value = false;
  await fetchList(false, pageSize.value, true);
};

const currentId = ref("");

const switchCurrentId = (row: TenantDto) => {
  currentId.value = row.id || "";
};

const fetchList = async (
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
    result = await $api.tenant.roleList(tenantId.value, {
      Cursor: cusor,
      Count: pageSize,
      IsPrevious: isPre,
    });
  } catch (e) {
    const error = e as any;
    const title = error.response.data.title;
    toast.add({ title: "获用户列表失败：" + title });
    return;
  }

  roles.value = result.data;
};

const getCursor = (isPre: boolean) => {
  if (roles.value.length == 0) {
    return undefined;
  }
  if (isPre) {
    return roles.value[0]?.id as string;
  } else {
    return roles.value[roles.value.length - 1]?.id as string;
  }
};

const showEditModal = ref(false);

const handleEdit = () => {
  showEditModal.value = true;
};

const handleUpdate = async (command: UpdateRoleCommand) => {
  let result;
  try {
    result = await $api.tenant.roleUpdate(currentId.value, command);
  } catch (e) {
    const error = e as any;
    const title = error.response.data.title;
    toast.add({ title: "修改角色失败：" + title });
    return;
  }

  toast.add({ title: "修改角色成功" });
  showEditModal.value = false;
  await fetchList(false, pageSize.value, true);
};

const handleDelete = async () => {
  try {
    await $api.tenant.roleDelete(currentId.value);
  } catch (e) {
    const error = e as any;
    const title = error.response.data.title;
    toast.add({ title: "删除角色失败：" + title, color: "error" });
    return;
  }

  toast.add({ title: "删除角色成功" });
  await fetchList(false, pageSize.value, true);
};
</script>
