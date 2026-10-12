<template>
  <UModal title="新增用户" v-model:open="show">
    <UButton label="新增用户" color="primary" variant="solid" />

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
          label="密码"
          v-model="password"
          type="password"
          placeholder="6-20 位"
          :error="errors.password"
          autocomplete="new-password"
          required
        ></CommonFormInput>

        <UButton
          label="确认"
          color="primary"
          variant="subtle"
          @click="createUser"
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

const errors = ref<{ username?: string; email?: string; password?: string }>({});

const emit = defineEmits<{
  (e: "create", payLoad: UserRegisterCommand): void;
}>();

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

  if (!password.value) {
    next.password = "密码不能为空";
  } else if (password.value.length < 6 || password.value.length > 20) {
    next.password = "密码长度必须在6-20个字符之间";
  }

  errors.value = next;
  return Object.keys(next).length === 0;
};

const createUser = function () {
  if (!validate()) {
    return;
  }

  emit("create", {
    username: username.value.trim(),
    password: password.value,
    email: email.value.trim(),
  });
};

// 父组件创建成功后关闭弹窗时再清空，避免失败后用户已填内容丢失
watch(show, (opened) => {
  if (!opened) {
    username.value = "";
    password.value = "";
    email.value = "";
    errors.value = {};
  }
});
</script>

<style scoped></style>
