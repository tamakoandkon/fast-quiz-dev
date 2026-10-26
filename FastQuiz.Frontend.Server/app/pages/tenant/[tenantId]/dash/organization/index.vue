<template>
  <CommonManagePageContainer>

    <CommonButtonsContainer>
      <OrganizationCreateOrganizationModal
        v-model="showCreateModal"
        @create="handleCreateOrg"
      />
    </CommonButtonsContainer>

    <!-- 组织列表 -->
    <div class="space-y-2">
      <div
        v-for="org in orgs"
        :key="org.id || ''"
        class="border border-gray-200 dark:border-gray-700 rounded-lg overflow-hidden"
      >
        <!-- 父组织行 -->
        <div
          class="flex items-center justify-between px-4 py-3 bg-white dark:bg-gray-800 hover:bg-gray-50 dark:hover:bg-gray-700/50 transition-colors cursor-pointer"
          @click="toggleExpand(org.id!)"
          @contextmenu.prevent="handleRightClick(org)"
        >
          <div class="flex items-center gap-3 flex-1 min-w-0">
            <UIcon
              :name="
                isExpanded(org.id!)
                  ? 'i-lucide-chevron-down'
                  : 'i-lucide-chevron-right'
              "
              class="w-4 h-4 text-gray-400 transition-transform flex-shrink-0"
            />
            <UIcon
              name="iconoir:city"
              class="w-4 h-4 text-primary-500 flex-shrink-0"
            />
            <NuxtLink
              :to="`/tenant/${tenantId}/dash/organization/${org.id}`"
              class="text-primary-600 hover:text-primary-700 hover:underline cursor-pointer truncate"
              @click.stop
            >
              {{ org.name }}
            </NuxtLink>
            <span class="text-xs text-gray-400 flex-shrink-0">{{
              org.id
            }}</span>
            <UBadge
              v-if="getChildrenCount(org.id!) > 0"
              :label="getChildrenCount(org.id!).toString()"
              color="info"
              size="xs"
              variant="subtle"
              class="flex-shrink-0"
            />
          </div>
          <div class="flex items-center gap-2 flex-shrink-0">
            <span class="text-xs text-gray-400">{{
              formatDate(org.createdAt)
            }}</span>
          </div>
        </div>

        <!-- 子组织列表 -->
        <div
          v-if="isExpanded(org.id!)"
          class="border-t border-gray-200 dark:border-gray-700 bg-gray-50/50 dark:bg-gray-800/30"
        >
          <!-- 加载中 -->
          <div v-if="isLoading(org.id!)" class="flex justify-center py-6">
            <USpinner class="w-5 h-5" />
          </div>

          <!-- 空状态 -->
          <div
            v-else-if="getChildren(org.id!).length === 0"
            class="text-sm text-gray-400 italic px-6 py-4"
          >
            暂无子组织
          </div>

          <!-- 子组织列表 -->
          <div
            v-else
            class="divide-y divide-gray-200/50 dark:divide-gray-700/50"
          >
            <div
              v-for="child in getChildren(org.id!)"
              :key="child.id || ''"
              class="flex items-center justify-between px-6 py-2.5 hover:bg-gray-100/50 dark:hover:bg-gray-700/30 transition-colors"
              @contextmenu.prevent="handleChildRightClick(child)"
            >
              <div class="flex items-center gap-3 flex-1 min-w-0">
                <UIcon
                  name="iconoir:city"
                  class="w-4 h-4 text-gray-400 flex-shrink-0"
                />
                <NuxtLink
                  :to="`/tenant/${tenantId}/dash/organization/${child.id}`"
                  class="text-primary-600 hover:text-primary-700 hover:underline cursor-pointer truncate"
                  @click.stop
                >
                  {{ child.name }}
                </NuxtLink>
                <span class="text-xs text-gray-400 flex-shrink-0">{{
                  child.id
                }}</span>
              </div>
              <div class="flex items-center gap-2 flex-shrink-0">
                <span class="text-xs text-gray-400">{{
                  formatDate(child.createdAt)
                }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 空状态 -->
    <div v-if="orgs.length === 0" class="text-center py-12">
      <UIcon name="iconoir:city" class="w-12 h-12 text-gray-300 mx-auto mb-3" />
      <p class="text-gray-500">暂无组织</p>
    </div>

    <CommonCursorPageManager @change="fetchList" :current-size="orgs.length">
    </CommonCursorPageManager>

    <!-- 模态框 -->
    <OrganizationUpdateOrganizationModal
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
const router = useRouter();

const tenantId = ref("");
const orgs = ref<OrganizationDto[]>([]);
const pageSize = ref(10);
const currentId = ref("");
const toast = useToast();

// 独立维护子组织数据
// key: 父组织ID, value: 子组织列表
const childrenMap = ref<Map<string, OrganizationDto[]>>(new Map());
// 加载状态
const loadingMap = ref<Map<string, boolean>>(new Map());
// 展开状态
const expandedMap = ref<Map<string, boolean>>(new Map());

// 获取子组织列表
const getChildren = (parentId: string): OrganizationDto[] => {
  return childrenMap.value.get(parentId) || [];
};

// 获取子组织数量
const getChildrenCount = (parentId: string): number => {
  return childrenMap.value.get(parentId)?.length || 0;
};

// 获取加载状态
const isLoading = (parentId: string): boolean => {
  return loadingMap.value.get(parentId) || false;
};

// 获取展开状态
const isExpanded = (parentId: string): boolean => {
  return expandedMap.value.get(parentId) || false;
};

// 切换展开/折叠
const toggleExpand = (parentId: string) => {
  const current = expandedMap.value.get(parentId) || false;
  expandedMap.value.set(parentId, !current);

  // 如果是展开，且没有子组织数据，触发加载
  if (!current) {
    const children = childrenMap.value.get(parentId);
    if (!children || children.length === 0) {
      loadChildren(parentId);
    }
  }
};

// 加载子组织
const loadChildren = async (parentId: string) => {
  // 如果正在加载，直接返回
  if (loadingMap.value.get(parentId)) {
    return;
  }

  // 如果已经有数据，直接返回
  if (
    childrenMap.value.get(parentId) &&
    childrenMap.value.get(parentId)!.length > 0
  ) {
    return;
  }

  loadingMap.value.set(parentId, true);

  try {
    const result = await $api.tenant.organizationDetail(
      tenantId.value,
      parentId,
      { Cursor: undefined, Count: 200, IsPrevious: false },
    );
    childrenMap.value.set(parentId, result.data || []);
  } catch (e) {
    const error = e as any;
    toast.add({
      title:
        "加载子组织失败：" + (error.response?.data?.title || error.message),
      color: "error",
    });
    childrenMap.value.set(parentId, []);
  } finally {
    loadingMap.value.set(parentId, false);
  }
};

// 刷新子组织数据（在创建、更新、删除后调用）
const refreshChildren = async (parentId?: string) => {
  if (!parentId) return;

  // 如果该组织已展开，重新加载数据
  if (expandedMap.value.get(parentId)) {
    // 清空旧数据，重新加载
    childrenMap.value.delete(parentId);
    await loadChildren(parentId);
  }
};

// 上下文菜单
const items = ref<ContextMenuItem[][]>([
  [
    {
      label: "进入",
      icon: "i-lucide-star",
      onSelect: () =>
        router.push(
          `/tenant/${tenantId.value}/dash/organization/${currentId.value}`,
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

const showCreateModal = ref(false);
const showEditModal = ref(false);

onMounted(async () => {
  tenantId.value = route.params.tenantId as string;
  await fetchList(false, pageSize.value, true);
});

// 获取组织列表
const fetchList = async (
  isPre: boolean,
  pageSize: number,
  refresh: boolean = false,
) => {
  let cursor = getCursor(isPre);
  if (refresh) {
    cursor = undefined;
    // 刷新时清空所有缓存
    childrenMap.value.clear();
    loadingMap.value.clear();
    expandedMap.value.clear();
  }

  let result;
  try {
    result = await $api.tenant.organizationList(tenantId.value, {
      Cursor: cursor,
      Count: pageSize,
      IsPrevious: isPre,
    });
  } catch (e) {
    const error = e as any;
    const title = error.response?.data?.title || error.message;
    toast.add({ title: "获取组织列表失败：" + title, color: "error" });
    return;
  }

  orgs.value = result.data || [];
};

const getCursor = (isPre: boolean) => {
  if (orgs.value.length == 0) {
    return undefined;
  }
  if (isPre) {
    return orgs.value[0]?.id as string;
  } else {
    return orgs.value[orgs.value.length - 1]?.id as string;
  }
};

// 右键点击
const handleRightClick = (row: OrganizationDto) => {
  currentId.value = row.id || "";
};

const handleChildRightClick = (child: OrganizationDto) => {
  currentId.value = child.id || "";
};

// 主组织操作
const handleCreateOrg = async (command: CreateOrganizationCommand) => {
  try {
    await $api.tenant.organizationCreate(tenantId.value, command);
    toast.add({ title: "新增组织成功" });
    showCreateModal.value = false;
    await fetchList(false, pageSize.value, true);
  } catch (e) {
    const error = e as any;
    const title = error.response?.data?.title || error.message;
    toast.add({ title: "新增组织失败：" + title, color: "error" });
  }
};

const handleEdit = () => {
  showEditModal.value = true;
};

const handleUpdate = async (command: UpdateOrganizationCommand) => {
  try {
    await $api.tenant.organizationUpdate(currentId.value, command);
    toast.add({ title: "修改组织成功" });
    showEditModal.value = false;
    await fetchList(false, pageSize.value, true);
  } catch (e) {
    const error = e as any;
    const title = error.response?.data?.title || error.message;
    toast.add({ title: "修改组织失败：" + title, color: "error" });
  }
};

const handleDelete = async () => {
  try {
    await $api.tenant.organizationDelete(currentId.value);
    toast.add({ title: "删除组织成功" });
    await fetchList(false, pageSize.value, true);
  } catch (e) {
    const error = e as any;
    const title = error.response?.data?.title || error.message;
    toast.add({ title: "删除组织失败：" + title, color: "error" });
  }
};

const formatDate = (date?: string) => {
  if (!date) return "-";
  return new Date(date).toLocaleString();
};
</script>

<style scoped>
/* 可以添加一些自定义样式 */
</style>
