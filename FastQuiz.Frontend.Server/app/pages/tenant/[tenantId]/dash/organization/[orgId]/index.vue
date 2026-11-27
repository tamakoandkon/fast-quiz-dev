<template>
  <CommonPageLayout :title="org.name || '组织成员'" :description="org.name ? '组织成员' : undefined">
    <template #actions>
      <OrganizationAddUserToOrgModal
        v-model="showCreateModal"
        @add="handleAddUser"
        :o-id="orgId"
        :users="otherUsers"
      />
      <OrganizationCreateOrganizationModal
        v-model="showCreateOrgModal"
        @create="handleCreateOrg"
        :parent-id="orgId"
      />
    </template>
    <section class="workspace-panel workspace-table-panel">
      <CommonDataTable :columns="cols" :data="users">
        <template #username="{ row }">
          <NuxtLink
            v-if="row.id"
            :to="`/tenant/${tenantId}/dash/user/${row.id}`"
            class="font-medium text-primary-600 hover:underline"
          >{{ row.username || "未命名用户" }}</NuxtLink>
          <span v-else>{{ row.username || "未命名用户" }}</span>
        </template>
        <template #actions="{ row }">
          <UButton
            v-if="row.id"
            color="neutral"
            variant="ghost"
            icon="i-lucide-user-minus"
            size="sm"
            :aria-label="`将 ${row.username || '未命名用户'} 移出组织`"
            @click="removeMember(row)"
          >移出组织</UButton>
        </template>
      </CommonDataTable>
      <CommonCursorPageManager v-model="pageSize" @change="fetchList" :current-size="users.length">
      </CommonCursorPageManager>
    </section>
  </CommonPageLayout>
</template>

<script setup lang="ts">
const { $api } = useNuxtApp();
const route = useRoute();

const tenantId = ref("");
const orgId = ref("");


const toast = useToast();

const users = ref<UserDto[]>([]);
const pageSize = ref(10);

onMounted(async () => {
  tenantId.value = route.params.tenantId as string;
  orgId.value = route.params.orgId as string;

  await fetchOrgAsync(orgId.value);


  await fetchList(false, pageSize.value, true);
  await fetchUsersList(false, pageSize.value, true);
});

// 3. 配置列 (TableColumn<UserDto>)
const cols = [
  { key: "username", label: "用户名" },
  { key: "actions", label: "操作", width: "9rem" },
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

const removeMember = async (user: UserDto) => {
  if (!user.id) return;
  try {
    await $api.tenant.organizationUserDelete(orgId.value, user.id);
  } catch (e) {
    const error = e as any;
    toast.add({ title: "移出组织失败：" + (error.response?.data?.title || error.message), color: "error" });
    return;
  }

  toast.add({ title: "已移出组织" });
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
