<template>
  <UModal :title="editable ? '编辑角色' : '角色详情'" v-model:open="show">
    <template #body>
      <div v-if="loading" class="flex justify-center py-8" role="status" aria-label="正在加载角色">
        <USpinner />
      </div>
      <div v-else-if="loadError" class="grid justify-items-center gap-3 py-6">
        <p role="alert">暂时无法加载角色。</p>
        <UButton color="neutral" variant="outline" @click="loadRole">重新加载</UButton>
      </div>
      <CommonFormContainer v-else-if="role && editable">
        <CommonFormInput label="角色名称" v-model="name" />
        <CommonFormInput label="角色说明" v-model="description" />
        <UFormField label="是否启用" class="w-full">
          <USwitch v-model="isEnable" />
        </UFormField>
        <UFormField label="权限" class="w-full">
          <div class="grid grid-cols-1 gap-2 sm:grid-cols-2">
            <UCheckbox
              v-for="permission in permissions"
              :key="permission"
              :label="getPermissionDisplayName(permission)"
              :model-value="selectedPermissions.includes(permission)"
              @update:model-value="checked => togglePermission(permission, checked === true)"
            />
          </div>
        </UFormField>
        <UButton label="保存角色" color="primary" class="w-full justify-center" @click="saveRole" />
      </CommonFormContainer>
      <div v-else-if="role" class="space-y-5">
        <div class="space-y-2">
          <h2 class="text-lg font-semibold">{{ getRoleDisplayName(role) }}</h2>
          <p v-if="getRoleDisplayDescription(role, tenantName)" class="break-words text-sm text-muted">
            {{ getRoleDisplayDescription(role, tenantName) }}
          </p>
          <UBadge :color="role.isEnable ? 'success' : 'neutral'" variant="soft">
            {{ role.isEnable ? "已启用" : "已停用" }}
          </UBadge>
        </div>
        <div v-if="role.createdAt || role.updatedAt" class="grid gap-4 text-sm sm:grid-cols-2">
          <div v-if="role.createdAt">
            <p class="text-muted">创建时间</p>
            <p><time :datetime="role.createdAt">{{ formatDate(role.createdAt) }}</time></p>
          </div>
          <div v-if="role.updatedAt">
            <p class="text-muted">最近更新</p>
            <p><time :datetime="role.updatedAt">{{ formatDate(role.updatedAt) }}</time></p>
          </div>
        </div>
        <div class="space-y-2">
          <h3 class="text-sm font-semibold">权限</h3>
          <ul v-if="role.permissions?.length" class="flex flex-wrap gap-2">
            <li v-for="permission in role.permissions" :key="permission">
              <UBadge color="neutral" variant="subtle">{{ getPermissionDisplayName(permission) }}</UBadge>
            </li>
          </ul>
          <p v-else class="text-sm text-muted">该角色尚未分配权限。</p>
        </div>
      </div>
    </template>
  </UModal>
</template>

<script setup lang="ts">
import { getPermissionDisplayName, getRoleDisplayDescription, getRoleDisplayName } from "~/utils/roleDisplay";

const { $api } = useNuxtApp();
const show = defineModel({ type: Boolean, required: true });
const props = defineProps<{
  id: string;
  editable: boolean;
  tenantName?: string;
}>();
const emit = defineEmits<{
  update: [command: UpdateRoleCommand];
}>();

const loading = ref(false);
const loadError = ref(false);
const role = ref<RoleDto>();
const permissions = ref<string[]>([]);
const selectedPermissions = ref<string[]>([]);
const name = ref("");
const description = ref("");
const isEnable = ref(true);
let initialName = "";
let initialDescription = "";
let requestNumber = 0;

async function loadRole() {
  const request = ++requestNumber;
  if (!show.value || !props.id) return;
  loading.value = true;
  loadError.value = false;
  role.value = undefined;
  try {
    const [details, catalog] = await Promise.all([
      $api.tenant.roleDetail(props.id),
      props.editable ? $api.tenant.rolePermissionList() : undefined,
    ]);
    if (request !== requestNumber) return;
    const item = details.data;
    role.value = item;
    initialName = getRoleDisplayName(item);
    initialDescription = getRoleDisplayDescription(item, props.tenantName);
    name.value = initialName;
    description.value = initialDescription;
    permissions.value = catalog?.data ?? [];
    selectedPermissions.value = [...(item.permissions ?? [])];
    isEnable.value = item.isEnable ?? true;
  } catch {
    if (request === requestNumber) loadError.value = true;
  } finally {
    if (request === requestNumber) loading.value = false;
  }
}

watch(() => [show.value, props.id, props.editable] as const, loadRole);

function togglePermission(permission: string, checked: boolean) {
  if (checked && !selectedPermissions.value.includes(permission)) {
    selectedPermissions.value.push(permission);
  }
  if (!checked) {
    selectedPermissions.value = selectedPermissions.value.filter(value => value !== permission);
  }
}

function saveRole() {
  if (!role.value) return;
  emit("update", {
    name: name.value === initialName ? role.value.name ?? "" : name.value,
    description: description.value === initialDescription ? role.value.description ?? "" : description.value,
    isEnable: isEnable.value,
    permissions: selectedPermissions.value,
  });
}
</script>
