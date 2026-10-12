<template>
  <UModal title="修改个人资料" v-model:open="show">
    <template #body>
      <CommonFormContainer>
        <CommonFormInput
          label="用户名"
          v-model="username"
          placeholder="3-20 位，仅限字母、数字和下划线"
          :error="errors.username"
          required
        ></CommonFormInput>
        <CommonFormInput
          label="邮箱"
          v-model="email"
          type="email"
          placeholder="选填，用于接收通知"
          :error="errors.email"
        ></CommonFormInput>
        <CommonFormInput
          label="新密码"
          v-model="password"
          type="password"
          placeholder="留空则不修改密码"
          :error="errors.password"
          description="如需修改密码，请输入 6-20 位新密码"
          autocomplete="new-password"
        ></CommonFormInput>

        <UButton
          label="保存"
          color="primary"
          variant="subtle"
          :loading="isSubmitting"
          :disabled="isSubmitting"
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
const isSubmitting = ref(false);

const errors = ref<{ username?: string; email?: string; password?: string }>({});

const validate = () => {
  const next: typeof errors.value = {};

  const name = username.value.trim();
  if (!name) {
    next.username = "用户名不能为空";
  } else if (name.length < 3 || name.length > 20) {
    next.username = "用户名长度必须在3-20个字符之间";
  } else if (!/^[a-zA-Z0-9_]+$/.test(name)) {
    next.username = "用户名只能包含字母、数字和下划线";
  }

  if (email.value.trim() && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.value.trim())) {
    next.email = "邮箱格式不正确";
  }

  // 留空表示不修改密码
  if (password.value && (password.value.length < 6 || password.value.length > 20)) {
    next.password = "密码长度必须在6-20个字符之间";
  }

  errors.value = next;
  return Object.keys(next).length === 0;
};

const updateUser = async function () {
  if (!validate()) {
    return;
  }

  isSubmitting.value = true;
  try {
    await $api.auth.putAuth({
      username: username.value.trim(),
      email: email.value.trim(),
      password: password.value,
    });
  } catch (e: any) {
    const title = e.response?.data?.title ?? "未知错误";
    toast.add({ title: "更新用户失败：" + title, color: "error" });
    return;
  } finally {
    isSubmitting.value = false;
  }

  toast.add({ title: "更新用户成功", color: "success" });
  password.value = "";
  show.value = false;
};

const init = async () => {
  let result;
  try {
    result = await $api.auth.profileList();
  } catch (e: any) {
    const title = e.response?.data?.title ?? "未知错误";
    toast.add({ title: "获取用户失败：" + title, color: "error" });
    return;
  }
  username.value = result.data.username ?? "";
  email.value = result.data.email ?? "";
  password.value = "";
  errors.value = {};
};

// 每次打开重新拉取，避免显示上一次编辑的残留值
watch(show, async (opened) => {
  if (opened) {
    await init();
  }
});
</script>

<style scoped></style>
