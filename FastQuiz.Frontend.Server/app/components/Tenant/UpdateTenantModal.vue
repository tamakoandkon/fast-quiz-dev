<template>
  <UModal title="编辑租户" v-model:open="show">
    <template #body>
      <CommonFormContainer>
        <CommonFormInput label="名称" v-model="name"></CommonFormInput>
        <CommonFormInput label="详情" v-model="description"></CommonFormInput>
        <UButton
          label="确认"
          color="primary"
          variant="subtle"
          @click="createTenant"
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
const description = ref("");

onMounted(async () => {
  await init(props.id);
});

const emit = defineEmits<{
  (e: "update", payLoad: UpdateTenantCommand): void;
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

const createTenant = function () {
  emit("update", { name: name.value, description: description.value });
  name.value = "";
  description.value = "";
};

const init = async (id: string) => {
  let result;
  try {
    result = await $api.tenant.tenantDetail(id);
  } catch (e) {
    const error = e as any;
    const title = error.response.data.title;
    toast.add({ title: "获取租户失败：" + title });
    return;
  }
  name.value = result.data.name || "";
  description.value = result.data.description || "";
};
</script>

<style scoped></style>
