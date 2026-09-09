<script setup lang="ts">
definePageMeta({ layout: "auth" });
const { $api } = useNuxtApp();
const router = useRouter();

const state = reactive({
  username: "",
  password: "",
  confirmPassword: "",
});

const loading = ref(false);

async function onRegister() {
  if (state.password !== state.confirmPassword) {
    useToast().add({ title: "两次密码不一致" });
    return;
  }

  loading.value = true;
  try {
    // 假设后端有一个 registerCreate 接口
    await $api.auth.registerCreate({
      username: state.username,
      password: state.password,
      email: "",
      // 传递后端需要的其他 DTO 字段
    });

    useToast().add({
      title: "注册成功！",
      description: "现在可以登录了",
      color: "success",
    });
    router.push("/");
  } catch (err: any) {
    console.log(err);
    console.log($api);

    useToast().add({ title: "注册失败", color: "error" });
  } finally {
    loading.value = false;
  }
}
</script>

<template>
  <div
    class="min-h-screen flex items-center justify-center bg-gray-50 dark:bg-gray-900 px-4"
  >
    <UCard class="w-full max-w-md">
      <template #header>
        <div class="text-center">
          <h1 class="text-2xl font-bold text-gray-900 dark:text-white">
            创建账号
          </h1>
        </div>
      </template>

      <UForm :state="state" @submit="onRegister" class="space-y-4">
        <UFormField label="用户名" name="username">
          <UInput
            class="w-full"
            v-model="state.username"
            icon="i-heroicons-user"
          />
        </UFormField>

        <UFormField label="设置密码" name="password">
          <UInput
            class="w-full"
            v-model="state.password"
            type="password"
            icon="i-heroicons-key"
          />
        </UFormField>

        <UFormField label="确认密码" name="confirmPassword">
          <UInput
            class="w-full"
            v-model="state.confirmPassword"
            type="password"
            icon="i-heroicons-check-badge"
          />
        </UFormField>

        <UButton type="submit" block :loading="loading"> 注册 </UButton>
      </UForm>

      <template #footer>
        <p class="text-sm text-center text-gray-600 dark:text-gray-400">
          已有账号？
          <ULink to="/" class="text-primary font-medium hover:underline"
            >返回登录</ULink
          >
        </p>
      </template>
    </UCard>
  </div>
</template>
