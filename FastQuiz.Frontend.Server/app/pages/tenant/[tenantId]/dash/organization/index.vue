<template>
  <CommonPageLayout>
    <template #actions>
      <OrganizationCreateOrganizationModal v-model="showCreateModal" @create="handleCreateOrg" />
    </template>

    <section class="data-list" aria-label="组织列表">
      <div v-if="!orgs.length" class="data-list-state">
        <UIcon name="i-lucide-building-2" aria-hidden="true" />
        <p>暂无组织。</p>
      </div>
      <div v-else class="data-list-body">
        <article v-for="(org, index) in orgs" :key="org.id ?? index" class="organization-entry">
          <div class="organization-row">
            <div class="flex items-center gap-2 min-w-0">
              <UButton
                v-if="org.id"
                class="organization-expand shrink-0"
                color="neutral"
                variant="ghost"
                size="sm"
                icon="i-lucide-chevron-right"
                :aria-label="'子组织 · ' + (org.name || '未命名组织')"
                :aria-expanded="isExpanded(org.id)"
                @click="toggleExpand(org.id)"
              />
              <div class="min-w-0">
                <NuxtLink v-if="org.id" :to="organizationPath(org.id)" class="data-list-title">
                  {{ org.name || "未命名组织" }}
                </NuxtLink>
                <span v-else class="data-list-title">{{ org.name || "未命名组织" }}</span>
                <p v-if="org.id && getChildrenCount(org.id)" class="data-list-note">
                  {{ getChildrenCount(org.id) }} 个子组织
                </p>
              </div>
            </div>

            <div v-if="org.id" class="data-list-actions">
              <UButton :to="organizationPath(org.id)" color="neutral" variant="ghost" size="sm">查看成员</UButton>
              <UDropdownMenu :items="organizationActions(org.id)" :content="{ align: 'end' }">
                <UButton icon="i-lucide-ellipsis" color="neutral" variant="ghost" size="sm"
                  :aria-label="'管理 ' + (org.name || '未命名组织')" />
              </UDropdownMenu>
            </div>
          </div>

          <Transition name="workspace-disclosure">
            <div v-if="org.id && isExpanded(org.id)" class="workspace-disclosure">
              <div class="organization-children min-h-0 overflow-hidden">
                <div v-if="isLoading(org.id)" class="organization-child-note" role="status">正在加载子组织…</div>
                <div v-else-if="!getChildren(org.id).length" class="organization-child-note">暂无子组织。</div>
                <template v-else>
                  <div v-for="(child, index) in getChildren(org.id)" :key="child.id ?? index" class="organization-row">
                    <NuxtLink v-if="child.id" :to="organizationPath(child.id)" class="data-list-title">
                      {{ child.name || "未命名组织" }}
                    </NuxtLink>
                    <span v-else class="data-list-title">{{ child.name || "未命名组织" }}</span>
                    <div v-if="child.id" class="data-list-actions">
                      <UButton :to="organizationPath(child.id)" color="neutral" variant="ghost" size="sm">查看成员</UButton>
                      <UDropdownMenu :items="organizationActions(child.id)" :content="{ align: 'end' }">
                        <UButton icon="i-lucide-ellipsis" color="neutral" variant="ghost" size="sm"
                          :aria-label="'管理 ' + (child.name || '未命名组织')" />
                      </UDropdownMenu>
                    </div>
                  </div>
                </template>
              </div>
            </div>
          </Transition>
        </article>
      </div>
      <CommonCursorPageManager v-model="pageSize" @change="fetchList" :current-size="orgs.length" />
    </section>

    <OrganizationUpdateOrganizationModal v-model="showEditModal" @update="handleUpdate" :id="currentId" />
  </CommonPageLayout>
</template>

<script setup lang="ts">
const { $api } = useNuxtApp();
import type { DropdownMenuItem } from "@nuxt/ui";

const route = useRoute();

const tenantId = ref("");
const organizationPath = (id: string) => "/tenant/" + tenantId.value + "/dash/organization/" + id;
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

const organizationActions = (id: string): DropdownMenuItem[][] => [
  [{
    label: "编辑组织",
    icon: "i-lucide-edit",
    onSelect: () => {
      currentId.value = id;
      showEditModal.value = true;
    },
  }],
  [{
    label: "删除组织",
    color: "error",
    icon: "i-lucide-trash",
    onSelect: () => handleDelete(id),
  }],
];

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

const handleDelete = async (id: string) => {
  try {
    await $api.tenant.organizationDelete(id);
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
