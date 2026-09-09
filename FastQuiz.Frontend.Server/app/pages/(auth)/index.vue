<script setup lang="ts">
definePageMeta({ layout: "auth" });

const { $api } = useNuxtApp();
const router = useRouter();
import { useUserStore } from "~/stores/user";

const state = reactive({
  username: "",
  password: "",
});

const userStore = useUserStore();

const loading = ref(false);

async function onSubmit() {
  loading.value = true;
  try {
    // 调用 swagger-typescript-api 生成的接口
    const res = await $api.auth.loginCreate(state);

    if (res.data.accessToken) {
      userStore.setToken(res.data.accessToken);
      router.push("/tenant");
      useToast().add({
        title: `欢迎回来, ${res.data.user?.username}!`,
        color: "primary",
      });
    }
  } catch (err: any) {
    useToast().add({
      title: "登录失败",
      description: err.response?.data?.message || "请检查账号密码",
    });
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
            账户登录
          </h1>
        </div>
      </template>

      <UForm :state="state" @submit="onSubmit" class="space-y-4">
        <UFormField label="用户名" name="username" required>
          <UInput
            class="w-full"
            v-model="state.username"
            placeholder="输入用户名"
            icon="i-heroicons-user"
          />
        </UFormField>

        <UFormField label="密码" name="password" required>
          <UInput
            class="w-full"
            v-model="state.password"
            type="password"
            placeholder="输入密码"
            icon="i-heroicons-lock-closed"
          />
        </UFormField>

        <UButton
          type="submit"
          block
          :loading="loading"
          color="primary"
          variant="solid"
        >
          登录
        </UButton>
      </UForm>

      <template #footer>
        <p class="text-sm text-center text-gray-600 dark:text-gray-400">
          还没有账号？
          <ULink to="/register" class="text-primary font-medium hover:underline"
            >立即注册</ULink
          >
        </p>
      </template>
    </UCard>
  </div>
</template>
