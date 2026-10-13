<template>
  <CommonManagePageContainer>
    <UBreadcrumb :items="links" class="py-2" />
    <USeparator class="my-2" />
    <CommonButtonsContainer>
      <UserCreateUserModal
        v-model="showCreateModal"
        @create="handleCreateUser"
      />
      <RoleManageUserRoleModal
        v-model="showManageUserRoleModal"
        :id="currentId"
        :tenant-id="tenantId"
      />
      <!-- 新增：批量导入用户按钮 -->
      <UButton
        color="neutral"
        variant="outline"
        icon="i-lucide-upload"
        :loading="isImportingUsers"
        @click="triggerUserImport"
      >
        批量导入用户
      </UButton>
      <input
        ref="userFileInputRef"
        type="file"
        accept=".csv"
        class="hidden"
        @change="handleUserFileImport"
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

    <UserUpdateUserModal
      v-model="showEditModal"
      @update="handleUpdate"
      :id="currentId"
      :tenant-id="tenantId"
    />

  </CommonManagePageContainer>
</template>

<script setup lang="ts">
const { $api } = useNuxtApp();
import type { BreadcrumbItem } from "@nuxt/ui";
import type { ContextMenuItem } from "@nuxt/ui";
import Papa from "papaparse"; // 导入 papaparse

const route = useRoute();
const router = useRouter();

const tenantId = ref("");

const items = ref<ContextMenuItem[][]>([
  [
    {
      label: "查看详情",
      icon: "i-lucide-star",
      onSelect: () => {
        router.push(`/tenant/${tenantId}/dash/user/${currentId.value}`);
      },
    },
    {
      label: "管理角色",
      icon: "i-lucide-edit",
      onSelect: () => {
        showManageUserRoleModal.value = true;
      },
    },
    {
      label: "查看考试成绩",
      icon: "i-lucide-clipboard-list",
      onSelect: () => {
        router.push(
          `/tenant/${tenantId.value}/dash/user-records/${currentId.value}`,
        );
      },
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
  links.value.push({
    label: "用户管理",
    icon: "iconoir:user",
    to: `/tenant/${tenantId.value}/dash/user`,
  });
  await fetchList(false, pageSize.value, true);
});

// 3. 配置列 (TableColumn<UserDto>)
const cols = [
  { key: "id", label: "ID", width: "16px" },
  { key: "name", label: "用户名" },
];

const showCreateModal = ref(false);

const handleCreateUser = async (command: UserRegisterCommand) => {
  let result;
  try {
    result = await $api.tenant.userCreate(tenantId.value, command);
  } catch (e) {
    const error = e as any;
    const title = error.response.data.title;
    toast.add({ title: "新增用户失败：" + title });
    return;
  }

  toast.add({ title: "新增用户成功" });
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
    result = await $api.tenant.userList(tenantId.value, {
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

const handleUpdate = async (command: UpdateUserCommand) => {
  let result;
  try {
    result = await $api.tenant.userUpdate(
      tenantId.value,
      currentId.value,
      command,
    );
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
    await $api.tenant.userDelete(tenantId.value, currentId.value);
  } catch (e) {
    const error = e as any;
    const title = error.response.data.title;
    toast.add({ title: "删除用户失败：" + title, color: "error" });
    return;
  }

  toast.add({ title: "删除用户成功" });
  await fetchList(false, pageSize.value, true);
};

const showManageUserRoleModal = ref(false);

// ---------- 新增批量导入用户相关 ----------
const userFileInputRef = ref<HTMLInputElement | null>(null);
const isImportingUsers = ref(false);

// 触发文件选择
const triggerUserImport = () => {
  userFileInputRef.value?.click();
};

// 处理用户CSV导入
const handleUserFileImport = async (event: Event) => {
  const target = event.target as HTMLInputElement;
  const file = target.files?.[0];
  if (!file) return;

  try {
    const text = await file.text();

    // 使用 Papa Parse 解析 CSV
    const result = Papa.parse<string[]>(text, {
      skipEmptyLines: true,
      trimHeaders: false,
    });

    const rows = result.data;
    if (rows.length < 2) {
      toast.add({ title: "CSV文件无有效数据", color: "warning" });
      return;
    }

    // 跳过表头（第一行），从第2行开始解析
    const userCommands: UserRegisterCommand[] = [];
    for (let i = 1; i < rows.length; i++) {
      const row = rows[i];
      if (row.length < 3) {
        console.warn(`第 ${i + 1} 行格式错误，跳过`, row);
        continue;
      }

      const username = row[0]?.trim() || "";
      const email = row[1]?.trim() || "";
      const password = row[2]?.trim() || "";

      if (username && email && password) {
        userCommands.push({
          username,
          email,
          password,
        });
      } else {
        console.warn(`第 ${i + 1} 行数据不完整，跳过`, row);
      }
    }

    if (userCommands.length === 0) {
      toast.add({ title: "未解析到有效用户数据", color: "warning" });
      return;
    }

    // 批量创建用户
    isImportingUsers.value = true;
    let successCount = 0;
    let failCount = 0;

    for (const user of userCommands) {
      try {
        await $api.tenant.userCreate(tenantId.value, user);
        successCount++;
      } catch (e) {
        failCount++;
        console.error("导入用户失败:", e, user);
      }
    }

    isImportingUsers.value = false;
    // 清空 input 值，允许重复选择同一文件
    if (userFileInputRef.value) userFileInputRef.value.value = "";

    if (successCount > 0) {
      toast.add({ title: `成功导入 ${successCount} 个用户`, color: "success" });
      await fetchList(false, pageSize.value, true); // 刷新列表
    }
    if (failCount > 0) {
      toast.add({
        title: `${failCount} 个用户导入失败，请检查控制台日志`,
        color: "error",
      });
    }
  } catch (e) {
    toast.add({ title: "读取或解析CSV文件失败", color: "error" });
    console.error(e);
  }
};
</script>
