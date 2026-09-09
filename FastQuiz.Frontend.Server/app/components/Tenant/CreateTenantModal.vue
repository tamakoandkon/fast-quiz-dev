<template>
  <UModal title="新增租户" v-model:open="show">
    <UButton label="新增租户" color="primary" variant="solid" />

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
const show = defineModel({ required: true, default: true });
const name = ref<string>("");
const description = ref<string>("");

const emit = defineEmits<{
  (e: "create", payLoad: CreateTenantCommand): void;
}>();

const createTenant = function () {
  emit("create", { name: name.value, description: description.value });
  name.value = "";
  description.value = "";
};
</script>

<style scoped></style>
