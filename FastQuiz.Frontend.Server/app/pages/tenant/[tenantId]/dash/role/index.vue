<template>
  <CommonPageLayout>
    <template #actions>
      <RoleCreateRoleModal v-model="showCreateModal" @create="createRole" />
    </template>

    <section class="data-list role-list" :class="{ 'has-role-actions': hasActions }" :aria-busy="loading">
      <div v-if="loading && !roles.length" class="data-list-state" role="status">
        <USpinner />
        <p>正在加载角色…</p>
      </div>
      <div v-else-if="loadError && !roles.length" class="data-list-state">
        <p role="alert">暂时无法加载角色。</p>
        <UButton color="neutral" variant="outline" size="sm" @click="loadRoles()">重新加载</UButton>
      </div>
      <div v-else-if="!roles.length" class="data-list-state">
        <UIcon name="i-lucide-shield-check" aria-hidden="true" />
        <p>还没有角色。</p>
      </div>
      <template v-else>
        <div role="table" aria-label="角色列表">
          <div class="data-list-head" role="row">
            <span role="columnheader">角色与说明</span>
            <span role="columnheader">状态</span>
            <span v-if="hasActions" role="columnheader">操作</span>
          </div>
          <div class="data-list-body" role="rowgroup">
            <article v-for="role in roles" :key="role.id || getRoleDisplayName(role)" class="data-list-row" role="row">
              <div class="data-list-main" role="cell">
                <button v-if="role.id" type="button" class="data-list-title text-left" @click="openRole(role, false)">
                  {{ getRoleDisplayName(role) }}
                </button>
                <span v-else class="data-list-title">{{ getRoleDisplayName(role) }}</span>
                <p v-if="getRoleDisplayDescription(role, tenantName)" class="data-list-note role-description">
                  {{ getRoleDisplayDescription(role, tenantName) }}
                </p>
              </div>
              <div class="data-list-status-cell" role="cell">
                <span class="data-status" :class="{ 'is-published': role.isEnable }">
                  {{ role.isEnable ? "已启用" : "已停用" }}
                </span>
              </div>
              <div v-if="hasActions" class="data-list-actions" role="cell">
                <UButton v-if="canEdit(role)" color="neutral" variant="ghost" size="sm" icon="i-lucide-pencil" @click="openRole(role, true)">
                  编辑
                </UButton>
                <UDropdownMenu v-if="canDelete(role)" :items="deleteMenu(role)">
                  <UButton color="neutral" variant="ghost" size="sm" icon="i-lucide-ellipsis" :aria-label="'更多操作 · ' + getRoleDisplayName(role)" />
                </UDropdownMenu>
              </div>
            </article>
          </div>
        </div>
        <CommonListFooter
          :page-index="pageIndex"
          :page-size="pageSize"
          :item-count="roles.length"
          :can-prev="pageIndex > 0"
          :can-next="canNext"
          :busy="loading"
          unit="个"
          @prev="loadRoles('prev')"
          @next="loadRoles('next')"
          @update:page-size="changePageSize"
        />
      </template>
    </section>

    <RoleUpdateRoleModal
      v-model="showRoleModal"
      :id="currentRole?.id ?? ''"
      :editable="editingRole"
      :tenant-name="tenantName"
      @update="updateRole"
    />
    <CommonConfirmModal
      v-model="showDeleteModal"
      :title="'删除角色「' + (roleToDelete ? getRoleDisplayName(roleToDelete) : '') + '」'"
      descript="删除后，已分配给用户的该角色也会被移除。"
      @action="deleteRole"
    />
  </CommonPageLayout>
</template>

<script setup lang="ts">
import { getRoleDisplayDescription, getRoleDisplayName } from "~/utils/roleDisplay";

const { $api } = useNuxtApp();
const route = useRoute();
const toast = useToast();
const tenantId = computed(() => String(route.params.tenantId || ""));

type PageDirection = "first" | "prev" | "next";

const roles = ref<RoleDto[]>([]);
const tenantName = ref("");
const systemAdmin = ref(false);
const pageSize = ref(10);
const pageIndex = ref(0);
const canNext = ref(false);
const loading = ref(false);
const loadError = ref(false);
let listRequest = 0;

const currentRole = ref<RoleDto>();
const roleToDelete = ref<RoleDto>();
const showCreateModal = ref(false);
const showRoleModal = ref(false);
const showDeleteModal = ref(false);
const editingRole = ref(false);

const canEdit = (role: RoleDto) => Boolean(role.id && (!role.isSystem || systemAdmin.value));
const canDelete = (role: RoleDto) => role.id !== "default_admin" && canEdit(role);
const hasActions = computed(() => roles.value.some(canEdit));

const loadRoles = async (direction: PageDirection = "first") => {
  if ((loading.value && direction !== "first") || !tenantId.value) return;
  const request = ++listRequest;
  const cursor = direction === "prev" ? roles.value[0]?.id
    : direction === "next" ? roles.value.at(-1)?.id
    : undefined;
  const count = pageSize.value;
  loading.value = true;
  loadError.value = false;
  try {
    const { data } = await $api.tenant.roleList(tenantId.value, {
      Cursor: cursor ?? undefined,
      Count: count + 1,
      IsPrevious: direction === "prev",
    });
    if (request !== listRequest) return;
    if (!data.length && direction !== "first") {
      if (direction === "next") canNext.value = false;
      return;
    }
    roles.value = direction === "prev" ? data.slice(-count) : data.slice(0, count);
    if (direction === "first") pageIndex.value = 0;
    if (direction === "prev") pageIndex.value--;
    if (direction === "next") pageIndex.value++;
    canNext.value = direction === "prev" || data.length > count;
  } catch {
    if (request !== listRequest) return;
    loadError.value = true;
    if (roles.value.length) toast.add({ title: "暂时无法加载角色", color: "error" });
  } finally {
    if (request === listRequest) loading.value = false;
  }
};

const loadRoleContext = async () => {
  const currentTenantId = tenantId.value;
  tenantName.value = "";
  systemAdmin.value = false;
  if (!currentTenantId) return;
  const [tenantResult, permissionsResult] = await Promise.allSettled([
    $api.tenant.tenantDetail(currentTenantId),
    $api.tenant.rolePermissionMeList("system_tenant"),
  ]);
  if (tenantId.value !== currentTenantId) return;
  if (tenantResult.status === "fulfilled") tenantName.value = tenantResult.value.data.name ?? "";
  if (permissionsResult.status === "fulfilled") systemAdmin.value = permissionsResult.value.data.includes("system");
};

onMounted(() => {
  watch(tenantId, () => {
    roles.value = [];
    pageIndex.value = 0;
    canNext.value = false;
    showRoleModal.value = false;
    showDeleteModal.value = false;
    void Promise.all([loadRoles(), loadRoleContext()]);
  }, { immediate: true });
});

const changePageSize = (count: number) => {
  pageSize.value = count;
  void loadRoles();
};

const openRole = (role: RoleDto, editable: boolean) => {
  if (!role.id) return;
  currentRole.value = role;
  editingRole.value = editable;
  showRoleModal.value = true;
};

const deleteMenu = (role: RoleDto) => [[{
  label: "删除",
  icon: "i-lucide-trash",
  color: "error" as const,
  onSelect: () => {
    roleToDelete.value = role;
    showDeleteModal.value = true;
  },
}]];

const createRole = async (command: CreateRoleCommand) => {
  try {
    await $api.tenant.roleCreate(tenantId.value, command);
  } catch {
    toast.add({ title: "角色未创建，请确认名称和权限后重试", color: "error" });
    return;
  }
  toast.add({ title: "已新增角色" });
  showCreateModal.value = false;
  await loadRoles();
};

const updateRole = async (command: UpdateRoleCommand) => {
  const id = currentRole.value?.id;
  if (!id) return;
  try {
    await $api.tenant.roleUpdate(id, command);
  } catch {
    toast.add({ title: "角色未保存，请确认名称和权限后重试", color: "error" });
    return;
  }
  toast.add({ title: "已保存角色" });
  showRoleModal.value = false;
  await loadRoles();
};

const deleteRole = async () => {
  const id = roleToDelete.value?.id;
  if (!id) return;
  showDeleteModal.value = false;
  try {
    await $api.tenant.roleDelete(id);
  } catch {
    toast.add({ title: "暂时无法删除角色", color: "error" });
    return;
  }
  toast.add({ title: "已删除角色" });
  await loadRoles();
};
</script>

<style scoped>
.role-list {
  --list-cols: minmax(0, 1fr) 88px;
}
.role-list.has-role-actions {
  --list-cols: minmax(0, 1fr) 88px minmax(96px, auto);
}
.role-description {
  overflow-wrap: anywhere;
  white-space: normal;
}
@media (max-width: 1260px) {
  .role-list .data-list-row {
    grid-template-columns: minmax(0, 1fr) auto;
    grid-template-areas: "main status";
  }
  .role-list.has-role-actions .data-list-row {
    grid-template-areas: "main status" "main actions";
  }
}
</style>
