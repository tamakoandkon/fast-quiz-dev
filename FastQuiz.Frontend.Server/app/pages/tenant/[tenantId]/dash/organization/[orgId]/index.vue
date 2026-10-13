<template>
  <CommonManagePageContainer>
    <UBreadcrumb :items="links" class="py-2" />
    <USeparator class="my-2" />
    <CommonButtonsContainer>
      <OrganizationAddUserToOrgModal
        v-model="showCreateModal"
        @add="handleAddUser"
        :o-id="orgId"
        :tenant-id="tenantId"
        :users="otherUsers"
      />
      <OrganizationCreateOrganizationModal
        v-model="showCreateOrgModal"
        @create="handleCreateOrg"
        :parent-id="orgId"
      />
    </CommonButtonsContainer>
    <UContextMenu :items="items">
      <CommonDataTable
        :columns="cols"
        :data="users"
        :selectable="true"
        v-model="selected"
        @row-right-click="(row) => switchCurrentId(row)"
      >
        <template #id="{ row }">
          <CommonTruncateText :text="row.id || ''" direction="rtl">
            <NuxtLink
              :to="`/tenant/${tenantId}/dash/user/${row.id}`"
              class="text-primary-600 hover:text-primary-700 hover:underline cursor-pointer"
            >
              {{ row.id }}
            </NuxtLink>
          </CommonTruncateText>
        </template>

        <template #name="{ row }">
          <CommonTruncateText :text="row.username || ''" width="10rem">
          </CommonTruncateText>
        </template>
      </CommonDataTable>
    </UContextMenu>
    <CommonCursorPageManager @change="fetchList" :current-size="users.length">
    </CommonCursorPageManager>

    <!-- <OrganizationUpdateOrganizationModal
      v-model="showEditModal"
      @update="handleUpdate"
      :id="currentId"
    /> -->
  </CommonManagePageContainer>
</template>

<script setup lang="ts">
const { $api } = useNuxtApp();
import type { BreadcrumbItem } from "@nuxt/ui";
import type { ContextMenuItem } from "@nuxt/ui";
const route = useRoute();

const tenantId = ref("");
const orgId = ref("");

const items = ref<ContextMenuItem[][]>([
  [
    {
      label: "进入",
      icon: "i-lucide-star",
      onSelect: () => handleEdit(),
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
const links = ref<BreadcrumbItem[]>([]);

const toast = useToast();

const users = ref<UserDto[]>([]);
const pageSize = ref(10);

const selected = ref<UserDto[]>([]);

onMounted(async () => {
  tenantId.value = route.params.tenantId as string;
  orgId.value = route.params.orgId as string;

  await fetchOrgAsync(orgId.value);

  links.value.push({
    label: "组织管理",
    icon: "iconoir:city",
    to: `/tenant/${tenantId.value}/dash/organization`,
  });

  links.value.push({
    label: org.value.name ?? "未知组织",
    icon: "iconoir:city",
    to: `/tenant/${tenantId.value}/dash/organization/${org.value.id}`,
  });
  await fetchList(false, pageSize.value, true);
  await fetchUsersList(false, pageSize.value, true);
});

// 3. 配置列 (TableColumn<UserDto>)
const cols = [
  { key: "id", label: "ID", width: "16px" },
  { key: "name", label: "用户名" },
];

const showCreateModal = ref(false);

const handleAddUser = async (command: { userId: string; orgId: string }) => {
  let result;
  try {
    result = await $api.tenant.organizationUserCreate(
      command.orgId,
      command.userId,
    );
  } catch (e) {
    const error = e as any;
    const title = error.response.data.title;
    toast.add({ title: "新增用户失败：" + title });
    return;
  }

  toast.add({ title: "新增用户成功" });
  showCreateModal.value = false;
  await fetchList(false, pageSize.value, true);
  await fetchUsersList(false, pageSize.value, true);
};

const currentId = ref("");

const switchCurrentId = (row: UserDto) => {
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
    result = await $api.tenant.organizationUserList(orgId.value, {
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

  users.value = result.data;
};

const getCursor = (isPre: boolean) => {
  if (users.value.length == 0) {
    return undefined;
  }
  if (isPre) {
    return users.value[0]?.id as string;
  } else {
    return users.value[users.value.length - 1]?.id as string;
  }
};

const showEditModal = ref(false);

const handleEdit = () => {
  showEditModal.value = true;
};

const handleUpdate = async (command: UpdateOrganizationCommand) => {
  let result;
  try {
    result = await $api.tenant.organizationUpdate(currentId.value, command);
  } catch (e) {
    const error = e as any;
    const title = error.response.data.title;
    toast.add({ title: "修改用户失败：" + title });
    return;
  }

  toast.add({ title: "修改用户成功" });
  showEditModal.value = false;
  await fetchList(false, pageSize.value, true);
};

const handleDelete = async () => {
  try {
    await $api.tenant.organizationUserDelete(orgId.value, currentId.value);
  } catch (e) {
    const error = e as any;
    const title = error.response.data.title;
    toast.add({ title: "删除用户失败：" + title, color: "error" });
    return;
  }

  toast.add({ title: "删除用户成功" });
  await fetchList(false, pageSize.value, true);
  await fetchUsersList(false, pageSize.value, true);
};

const org = ref<OrganizationDto>({});

const fetchOrgAsync = async (orgId: string) => {
  let result;
  try {
    result = await $api.tenant.organizationDetail2(orgId);
  } catch (e) {
    const error = e as any;
    const title = error.response.data.title;
    toast.add({ title: "获取组织失败：" + title });
    return;
  }

  org.value = result.data;
};

const showCreateOrgModal = ref(false);
// 主组织操作
const handleCreateOrg = async (command: CreateOrganizationCommand) => {
  try {
    await $api.tenant.organizationCreate(tenantId.value, command);
    toast.add({ title: "新增组织成功" });
    showCreateOrgModal.value = false;
    await fetchList(false, pageSize.value, true);
  } catch (e) {
    const error = e as any;
    const title = error.response?.data?.title || error.message;
    toast.add({ title: "新增组织失败：" + title, color: "error" });
  }
};

const otherUsers = ref<UserDto[]>([]);

const fetchUsersList = async (
  isPre: boolean,
  pageSize: number,
  refresh: boolean = false,
) => {
  if (!tenantId.value || tenantId.value == "") {
    return;
  }

  let cusor = getUsersCursor(isPre);
  if (refresh) {
    cusor = undefined;
  }
  let result;
  try {
    result = await $api.tenant.organizationUserNotInList(orgId.value, {
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

  otherUsers.value = result.data;
};

const getUsersCursor = (isPre: boolean) => {
  if (otherUsers.value.length == 0) {
    return undefined;
  }
  if (isPre) {
    return otherUsers.value[0]?.id as string;
  } else {
    return otherUsers.value[otherUsers.value.length - 1]?.id as string;
  }
};
</script>
