<template>
  <UModal title="编辑角色" v-model:open="show">
    <template #body>
      <CommonFormContainer>
        <CommonFormInput label="名称" v-model="name"></CommonFormInput>
        <CommonFormInput label="详情" v-model="description"></CommonFormInput>
        <UFormField label="是否启用" class="w-full">
          <USwitch v-model="isEnable" />
        </UFormField>

        <!-- 权限列表 -->
        <UFormField label="权限列表" class="w-full">
          <div class="grid grid-cols-2 md:grid-cols-3 gap-2 w-full">
            <UCheckbox
              v-for="perm in permissions"
              :key="perm"
              :label="perm"
              :value="perm"
              :model-value="selectedPermissions.includes(perm)"
              @update:model-value="(checked) => togglePermission(perm, checked)"
            />
          </div>
        </UFormField>

        <UButton
          label="确认"
          color="primary"
          variant="subtle"
          @click="updateRole"
          class="w-full justify-center"
        />
      </CommonFormContainer>
    </template>
  </UModal>
</template>
<script setup lang="ts">
const { $api } = useNuxtApp();
const toast = useToast();
const show = defineModel({ required: true, default: true });
const name = ref<string>("");
const description = ref<string>("");
const isEnable = ref(true);

onMounted(async () => {
  await init(props.id);
  await getPermissions();
});

const emit = defineEmits<{
  (e: "update", payLoad: UpdateRoleCommand): void;
}>();

const props = defineProps({
  id: {
    type: String,
    default: "",
  },
});

watch(
  () => props.id,
  async (newId) => {
    if (newId) {
      await init(newId);
    }
  },
  { immediate: true },
);

const updateRole = function () {
  emit("update", {
    name: name.value,
    description: description.value,
    isEnable: isEnable.value,
    permissions: selectedPermissions.value,
  });
  name.value = "";
  description.value = "";
  isEnable.value = true;
};

const init = async (id: string) => {
  if (!id || id == "") {
    return;
  }

  let result;
  try {
    result = await $api.tenant.roleDetail(id);
  } catch (e) {
    const error = e as any;
    const title = error.response.data.title;
    toast.add({ title: "获取角色失败：" + title });
    return;
  }
  name.value = result.data.name || "";
  description.value = result.data.description || "";
  isEnable.value = result.data.isEnable || true;
  selectedPermissions.value = result.data.permissions ?? [];
};

const permissions = ref<string[]>([]);
const selectedPermissions = ref<string[]>([]);

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
