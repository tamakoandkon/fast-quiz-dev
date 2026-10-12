<template>
  <UModal title="新增租户" v-model:open="show">
    <UButton label="新增租户" color="primary" variant="solid" />

    <template #body>
      <CommonFormContainer>
        <USeparator label="租户信息" />
        <CommonFormInput label="名称" v-model="name"></CommonFormInput>
        <CommonFormInput label="详情" v-model="description"></CommonFormInput>

        <USeparator label="租户管理员" />
        <CommonFormInput
          label="管理员用户名"
          v-model="adminUsername"
        ></CommonFormInput>
        <CommonFormInput label="管理员邮箱" v-model="adminEmail"></CommonFormInput>
        <UFormField label="管理员密码" class="w-full">
          <UInput v-model="adminPassword" type="password"></UInput>
        </UFormField>

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

// 创建租户时会同时创建该租户的管理员账号
const adminUsername = ref<string>("");
const adminPassword = ref<string>("");
const adminEmail = ref<string>("");

const emit = defineEmits<{
  (e: "create", payLoad: CreateTenantCommand): void;
}>();

const createTenant = function () {
  emit("create", {
    name: name.value,
    description: description.value,
    adminUsername: adminUsername.value,
    adminPassword: adminPassword.value,
    adminEmail: adminEmail.value,
  });
  name.value = "";
  description.value = "";
  adminUsername.value = "";
  adminPassword.value = "";
  adminEmail.value = "";
};
</script>

<style scoped></style>
