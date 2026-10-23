<script setup lang="ts">
definePageMeta({ layout: 'auth' });
useHead({ title: '登录 · FastQuiz' });
const { $api } = useNuxtApp();
const router = useRouter();
const userStore = useUserStore();
const state = reactive({ username: '', password: '' });
const loading = ref(false);
const error = ref('');
const showPassword = ref(false);
async function onSubmit() {
  if (loading.value) return;
  error.value = '';
  if (!state.username.trim() || !state.password) {
    error.value = '请输入用户名和密码。';
    return;
  }
  loading.value = true;
  try {
    const res = await $api.auth.loginCreate({ username: state.username.trim(), password: state.password });
    if (res.data.accessToken) {
      await userStore.setToken(res.data.accessToken);
      await router.push('/tenant');
    }
  } catch (err: any) {
    error.value = err.response?.status === 401 ? '用户名或密码不正确，请重试。' : (err.response?.data?.title || '暂时无法登录，请稍后重试。');
  } finally { loading.value = false; }
}
</script>
<template>
  <section class="auth-panel" aria-labelledby="login-title">
    <div class="auth-heading">
      <h1 id="login-title">登录 FastQuiz</h1>
    </div>
    <UForm :state="state" @submit="onSubmit" class="auth-form">
      <UFormField label="用户名" name="username" required>
        <UInput v-model="state.username" class="w-full" placeholder="输入你的用户名" autocomplete="username" name="username" required :disabled="loading" />
      </UFormField>
      <UFormField label="密码" name="password" required>
        <UInput v-model="state.password" class="w-full" :type="showPassword ? 'text' : 'password'" placeholder="输入密码" autocomplete="current-password" name="password" required :disabled="loading" :ui="{ trailing: 'pe-2' }">
          <template #trailing>
            <UButton color="neutral" variant="ghost" size="sm" :icon="showPassword ? 'i-lucide-eye-off' : 'i-lucide-eye'" :aria-label="showPassword ? '隐藏密码' : '显示密码'" :aria-pressed="showPassword" @click="showPassword = !showPassword" />
          </template>
        </UInput>
      </UFormField>
      <p v-if="error" role="alert" class="text-sm text-error leading-relaxed">{{ error }}</p>
      <UButton type="submit" block :loading="loading" :disabled="loading">登录</UButton>
    </UForm>
    <div class="auth-divider" />
    <p class="auth-switch">还没有账号？<NuxtLink to="/register">创建账号</NuxtLink></p>
  </section>
</template>
