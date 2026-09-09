<template>
  <UModal title="修改用户" v-model:open="show">
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
          @click="updateUser"
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
const username = ref<string>("");
const password = ref<string>("");
const email = ref<string>("");

onMounted(async () => {
  await init();
});

const updateUser = async function () {
  let result;
  try {
    result = await $api.auth.putAuth({
      username: username.value,
      email: email.value,
      password: password.value,
    });
  } catch (e: any) {
    const title = e.response.data.title;
    toast.add({ title: "更新用户失败：" + title });
    return;
  }
};

const init = async () => {
  let result;
  try {
    result = await $api.auth.profileList();
  } catch (e: any) {
    const title = e.response.data.title;
    toast.add({ title: "获取用户失败：" + title });
    return;
  }
  username.value = result.data.username ?? "";
  password.value = "";
  email.value = "";
};
</script>

<style scoped></style>
