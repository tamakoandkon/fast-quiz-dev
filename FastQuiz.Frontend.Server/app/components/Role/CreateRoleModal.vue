<template>
  <UModal title="新增角色" v-model:open="show">
    <UButton label="新增角色" color="primary" variant="solid" />

    <template #body>
      <CommonFormContainer>
        <CommonFormInput label="角色名称" v-model="name" />
        <CommonFormInput label="角色说明" v-model="description" />

        <UFormField label="是否启用" class="w-full">
          <USwitch v-model="isEnable" />
        </UFormField>

        <!-- 权限列表 -->
        <UFormField label="权限列表" class="w-full">
          <div class="grid grid-cols-2 md:grid-cols-3 gap-2 w-full">
            <UCheckbox
              v-for="perm in permissions"
              :key="perm"
              :label="getPermissionDisplayName(perm)"
              :value="perm"
              :model-value="selectedPermissions.includes(perm)"
              @update:model-value="(checked) => togglePermission(perm, checked)"
            />
          </div>
        </UFormField>

        <UButton
          label="新增角色"
          color="primary"
          variant="subtle"
          @click="createRole"
          class="w-full justify-center"
        ></UButton>
      </CommonFormContainer>
    </template>
  </UModal>
</template>
<script setup lang="ts">
import { getPermissionDisplayName } from "~/utils/roleDisplay";

const { $api } = useNuxtApp();
const toast = useToast();

onMounted(async () => {
  await getPermissions();
});

const show = defineModel({ required: true, default: true });
const name = ref<string>("");
const description = ref<string>("");
const isEnable = ref(true);

const emit = defineEmits<{
  (e: "create", payLoad: CreateRoleCommand): void;
}>();

const createRole = () => {
  emit("create", {
    name: name.value,
    description: description.value,
    isEnable: isEnable.value,
    permissions: selectedPermissions.value,
  });

};

const permissions = ref<string[]>([]);
const selectedPermissions = ref<string[]>([]);

watch(show, (open) => {
  if (open) return;
  name.value = "";
  description.value = "";
  isEnable.value = true;
  selectedPermissions.value = [];
});

const getPermissions = async () => {
  let result;
  try {
    result = await $api.tenant.rolePermissionList();
  } catch (e) {
    const error = e as any;
    const title = error.response.data.title;
    toast.add({ title: "获取权限列表失败：" + title });
    return;
  }

  permissions.value = result.data;
};

const togglePermission = (perm: string, checked: boolean | "indeterminate") => {
  if (checked) {
    selectedPermissions.value.push(perm);
  } else {
    const index = selectedPermissions.value.indexOf(perm);
    if (index > -1) {
      selectedPermissions.value.splice(index, 1);
    }
  }
};
</script>

<style scoped></style>
