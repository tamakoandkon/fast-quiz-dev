<template>
  <UModal v-model:open="show" title="管理用户角色">
    <UButton label="管理用户角色" color="primary" variant="solid" />

    <template #body>
      <div class="space-y-4 p-2">
        <!-- 加载状态 -->
        <div v-if="loading" class="flex justify-center py-8">
          <USpinner size="md" />
        </div>

        <!-- 角色列表 -->
        <div v-else>
          <div
            v-if="roleItems.length === 0"
            class="text-center py-8 text-gray-500"
          >
            暂无角色数据
          </div>

          <div v-else class="space-y-2">
            <div
              v-for="item in roleItems"
              :key="item.id"
              class="flex items-center justify-between p-3 rounded-lg border border-gray-200 hover:bg-gray-50 transition-colors"
            >
              <div class="flex items-center gap-3">
                <UCheckbox
                  :model-value="item.checked"
                  :disabled="updatingRoleId === item.id"
                  @update:model-value="
                    (checked) => handleRoleToggle(item.id, checked as boolean)
                  "
                />
                <div class="flex flex-col">
                  <span class="text-sm font-medium">{{ item.name }}</span>
                  <span class="text-xs text-gray-400">{{ item.id }}</span>
                </div>
              </div>

              <!-- 更新中的加载状态 -->
              <USpinner v-if="updatingRoleId === item.id" size="xs" />
              <span v-else class="text-xs text-gray-400">
                {{ item.checked ? "已启用" : "未启用" }}
              </span>
            </div>
          </div>
        </div>

        <!-- 错误提示 -->
        <UAlert
          v-if="error"
          color="error"
          variant="soft"
          :title="error"
          @close="error = ''"
        />
      </div>
    </template>
  </UModal>
</template>

<script setup lang="ts">
const { $api } = useNuxtApp();
const toast = useToast();

// 弹窗显示控制
const show = defineModel({ required: true, default: true });

// 角色相关状态
const allRoles = ref<RoleDto[]>([]);
const userRoles = ref<RoleDto[]>([]);
const roleItems = ref<Array<{ id: string; name: string; checked: boolean }>>(
  [],
);
const loading = ref(false);
const updatingRoleId = ref<string | null>(null);
const error = ref("");

const props = defineProps({
  id: {
    type: String,
    default: "",
  },
  tenantId: {
    type: String,
    default: "",
  },
});

// 监听弹窗打开和用户ID变化
watch(
  () => props.id,
  async (newId) => {
    if (newId) {
      await loadData(newId);
    }
  },
  { immediate: true },
);

// 加载所有数据
const loadData = async (userId: string) => {
  try {
    loading.value = true;
    error.value = "";

    // 并行获取所有角色和用户角色
    await Promise.all([getAllRolesAsync(), getUserRoleAsync(userId)]);

    // 构建角色列表（包含选中状态）
    buildRoleItems(allRoles.value, userRoles.value);
  } catch (err) {
    error.value = "加载角色数据失败，请重试";
    console.error("加载角色数据失败:", err);
  } finally {
    loading.value = false;
  }
};

// 构建角色列表
const buildRoleItems = (all: RoleDto[], user: RoleDto[]) => {
  const userRoleIdSet = new Set(user.map((role) => role.id));
  roleItems.value = all.map((role) => ({
    id: role.id as string,
    name: role.name as string,
    checked: userRoleIdSet.has(role.id),
  }));
};

// 处理角色切换
const handleRoleToggle = async (roleId: string, checked: boolean) => {
  if (!props.id) return;

  // 防止重复操作
  if (updatingRoleId.value) return;

  // 保存当前状态以便回滚
  const previousItems = JSON.parse(JSON.stringify(roleItems.value));
  const previousUserRoles = [...userRoles.value];

  try {
    updatingRoleId.value = roleId;
    error.value = "";

    // 乐观更新UI
    updateLocalRoleState(roleId, checked);

    if (checked) {
      // 添加角色
      await addRoleToUserAsync(props.id, roleId);
      // 更新本地用户角色列表
      const roleToAdd = allRoles.value.find((r) => r.id === roleId);
      if (roleToAdd && !userRoles.value.some((r) => r.id === roleId)) {
        userRoles.value.push(roleToAdd);
      }
    } else {
      // 移除角色
      await removeRoleFromUser(props.id, roleId);
      // 更新本地用户角色列表
      userRoles.value = userRoles.value.filter((role) => role.id !== roleId);
    }
  } catch (err) {
    // 操作失败，回滚状态
    roleItems.value = previousItems;
    userRoles.value = previousUserRoles;
    error.value = `操作失败: ${err instanceof Error ? err.message : "未知错误"}`;
    console.error("角色操作失败:", err);
  } finally {
    updatingRoleId.value = null;
  }
};

// 更新本地UI状态
const updateLocalRoleState = (roleId: string, checked: boolean) => {
  const item = roleItems.value.find((item) => item.id === roleId);
  if (item) {
    item.checked = checked;
  }
};

// ========== API 方法 ==========
const getUserRoleAsync = async (userId: string) => {
  if (!userId || userId === "") {
    userRoles.value = [];
    return;
  }

  try {
    const result = await $api.tenant.roleUserDetail(props.tenantId, userId, {
      Cursor: undefined,
      Count: 200,
    });
    userRoles.value = result.data || [];
  } catch (e) {
    const error = e as any;
    const title = error.response?.data?.title || "获取用户角色失败";
    toast.add({ title, color: "error" });
    throw e;
  }
};

const getAllRolesAsync = async () => {
  try {
    const result = await $api.tenant.roleList(props.tenantId, {
      Cursor: undefined,
      Count: 200,
      IsPrevious: false,
    });
    allRoles.value = result.data || [];
  } catch (e) {
    const error = e as any;
    const title = error.response?.data?.title || "获取角色列表失败";
    toast.add({ title, color: "error" });
    throw e;
  }
};

const addRoleToUserAsync = async (userId: string, roleId: string) => {
  try {
    await $api.tenant.roleUserCreate(userId, roleId);
    toast.add({
      title: "添加角色成功",
      color: "success",
      icon: "i-heroicons-check-circle",
    });
  } catch (e) {
    const error = e as any;
    const title = error.response?.data?.title || "添加角色失败";
    toast.add({ title, color: "error" });
    throw e;
  }
};

const removeRoleFromUser = async (userId: string, roleId: string) => {
  try {
    await $api.tenant.roleUserDelete(userId, roleId);
    toast.add({
      title: "移除角色成功",
      color: "success",
      icon: "i-heroicons-check-circle",
    });
  } catch (e) {
    const error = e as any;
    const title = error.response?.data?.title || "移除角色失败";
    toast.add({ title, color: "error" });
    throw e;
  }
};
</script>

<style scoped>
/* 可以添加一些自定义样式 */
</style>
