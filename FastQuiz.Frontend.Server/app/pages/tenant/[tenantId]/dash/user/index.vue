<template>
  <CommonPageLayout>
    <template #actions>
      <UButton
        color="neutral"
        variant="outline"
        icon="i-lucide-upload"
        :loading="isImportingUsers"
        @click="triggerUserImport"
      >
        批量导入用户
      </UButton>
      <UserCreateUserModal v-model="showCreateModal" @create="handleCreateUser" />
      <input
        ref="userFileInputRef"
        type="file"
        accept=".csv"
        class="hidden"
        @change="handleUserFileImport"
      />
    </template>

    <section class="data-list user-list" aria-label="用户列表" :aria-busy="loading">
      <div v-if="loading && !loaded" role="status" aria-label="正在加载用户">
        <div v-for="n in 3" :key="n" class="user-list-row" aria-hidden="true">
          <div class="data-list-skeleton-main max-w-44">
            <span class="skeleton-line"></span>
            <span class="skeleton-line is-short is-small"></span>
          </div>
        </div>
      </div>
      <div v-else-if="loadError && !users.length" class="data-list-state" role="status">
        <p>用户列表暂时无法加载。</p>
        <UButton color="neutral" variant="outline" size="sm" @click="fetchUsers">重新加载</UButton>
      </div>
      <div v-else-if="!users.length" class="data-list-state" role="status">
        <p>当前没有用户。</p>
      </div>
      <template v-else>
        <UContextMenu :items="items">
          <ul class="divide-y divide-[var(--line-hair)]">
            <li
              v-for="(user, index) in visibleUsers"
              :key="user.id || index"
              class="user-list-row"
              @contextmenu="selectUser(user)"
            >
              <div class="min-w-0">
                <NuxtLink
                  v-if="user.id"
                  :to="`/tenant/${tenantId}/dash/user/${user.id}`"
                  class="data-list-title"
                >
                  {{ user.username || "未命名用户" }}
                </NuxtLink>
                <span v-else class="data-list-title">{{ user.username || "未命名用户" }}</span>
                <p v-if="user.email" class="data-list-note" :title="user.email">
                  {{ user.email }}
                </p>
              </div>
              <div v-if="user.id" class="flex shrink-0 items-center gap-1">
                <UButton
                  :to="recordsPath(user.id)"
                  :aria-label="`查看 ${user.username || '该用户'} 的成绩`"
                  color="neutral"
                  variant="ghost"
                  size="sm"
                >
                  查看成绩
                </UButton>
                <UDropdownMenu :items="userActions(user)" :content="{ align: 'end' }">
                  <UButton
                    icon="i-lucide-ellipsis"
                    color="neutral"
                    variant="ghost"
                    size="sm"
                    :aria-label="`管理 ${user.username || '该用户'}`"
                  />
                </UDropdownMenu>
              </div>
            </li>
          </ul>
        </UContextMenu>

        <CommonListFooter
          :page-index="pageIndex"
          :page-size="pageSize"
          :item-count="visibleUsers.length"
          :can-prev="pageIndex > 0"
          :can-next="(pageIndex + 1) * pageSize < users.length"
          :busy="loading"
          :size-options="[10, 20, 50, 100]"
          unit="人"
          @prev="pageIndex--"
          @next="pageIndex++"
          @update:page-size="setPageSize"
        />
      </template>
    </section>

    <RoleManageUserRoleModal
      v-model="showManageUserRoleModal"
      :id="currentId"
      :tenant-id="tenantId"
      :username="currentUsername"
    />
    <UserUpdateUserModal
      v-model="showEditModal"
      :id="currentId"
      :tenant-id="tenantId"
      @update="handleUpdate"
    />
  </CommonPageLayout>
</template>

<script setup lang="ts">
const { $api } = useNuxtApp();
import type { DropdownMenuItem } from "@nuxt/ui";
import Papa from "papaparse"; // 导入 papaparse

const route = useRoute();

const tenantId = computed(() => route.params.tenantId as string);
const recordsPath = (id: string) => `/tenant/${tenantId.value}/dash/user-records/${id}`;

const userActions = (user: UserDto) => {
  const id = user.id;
  if (!id) return [];
  return [
    [
      {
        label: "管理角色",
        icon: "i-lucide-shield",
        onSelect: () => {
          selectUser(user);
          showManageUserRoleModal.value = true;
        },
      },
      {
        label: "编辑用户资料",
        icon: "i-lucide-edit",
        onSelect: () => {
          selectUser(user);
          showEditModal.value = true;
        },
      },
    ],
    [
      {
        label: "移出租户",
        color: "error",
        icon: "i-lucide-user-minus",
        onSelect: () => removeMember(id),
      },
    ],
  ] satisfies DropdownMenuItem[][];
};

const toast = useToast();

const users = ref<UserDto[]>([]);
const pageIndex = ref(0);
const pageSize = ref(10);
const loading = ref(false);
const loaded = ref(false);
const loadError = ref(false);
const visibleUsers = computed(() => {
  const start = pageIndex.value * pageSize.value;
  return users.value.slice(start, start + pageSize.value);
});

const setPageSize = (size: number) => {
  pageSize.value = size;
  pageIndex.value = 0;
};

onMounted(fetchUsers);

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
  await fetchUsers();
};

const currentUser = ref<UserDto>();
const currentId = computed(() => currentUser.value?.id || "");
const currentUsername = computed(() => currentUser.value?.username || "");
const items = computed(() => currentUser.value ? userActions(currentUser.value) : []);

const selectUser = (row: UserDto) => {
  currentUser.value = row;
};

async function fetchUsers() {
  if (loading.value) return;
  loading.value = true;
  loadError.value = false;
  try {
    // The tenant user endpoint returns the complete list.
    const result = await $api.tenant.userList(tenantId.value, { Count: 100, IsPrevious: false });
    users.value = result.data ?? [];
    pageIndex.value = Math.min(pageIndex.value, Math.max(0, Math.ceil(users.value.length / pageSize.value) - 1));
  } catch {
    loadError.value = true;
    if (users.value.length) toast.add({ title: "刷新用户列表失败", color: "error" });
  } finally {
    loaded.value = true;
    loading.value = false;
  }
}

const showEditModal = ref(false);

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
  await fetchUsers();
};

const removeMember = async (userId: string) => {
  try {
    await $api.tenant.userDelete(tenantId.value, userId);
  } catch (e) {
    const error = e as any;
    toast.add({ title: "移出租户失败：" + (error.response?.data?.title || error.message), color: "error" });
    return;
  }

  toast.add({ title: "已移出租户" });
  await fetchUsers();
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
      if (!row || row.length < 3) {
        console.warn(`第 ${i + 1} 行格式错误，跳过`);
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
        console.warn(`第 ${i + 1} 行数据不完整，跳过`);
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
        console.error("导入用户失败:", user.username, e);
      }
    }

    isImportingUsers.value = false;
    // 清空 input 值，允许重复选择同一文件
    if (userFileInputRef.value) userFileInputRef.value.value = "";

    if (successCount > 0) {
      toast.add({ title: `成功导入 ${successCount} 个用户`, color: "success" });
      await fetchUsers(); // 刷新列表
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

<style scoped>
.user-list-row {
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto;
  align-items: center;
  gap: 12px;
  min-height: 64px;
  padding: 12px 16px;
}

.user-list-row:hover,
.user-list-row:focus-within {
  background: var(--tone-soft);
}

.user-list-row .data-list-title {
  overflow-wrap: anywhere;
}

@media (min-width: 640px) {
  .user-list-row {
    padding-inline: 20px;
  }
}
</style>
