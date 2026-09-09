setup
<template>
  <UModal title="新增用户" v-model:open="show">
    <UButton label="新增用户" color="primary" variant="solid" />

    <template #body>
      <CommonFormContainer>
        <CommonFormInput label="名称" v-model="username"></CommonFormInput>
        <CommonFormInput label="邮箱" v-model="email"></CommonFormInput>
        <UFormField label="密码" class="w-full">
          <UInput v-model="password" type="password"></UInput>
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
const username = ref<string>("");
const password = ref<string>("");
const email = ref<string>("");

const emit = defineEmits<{
  (e: "create", payLoad: UserRegisterCommand): void;
}>();

const createTenant = function () {
  emit("create", {
    username: username.value,
    password: password.value,
    email: email.value,
  });
  username.value = "";
  password.value = "";
  email.value = "";
};
</script>

<style scoped></style>
