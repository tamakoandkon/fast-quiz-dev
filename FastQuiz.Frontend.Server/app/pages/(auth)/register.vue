<script setup lang="ts">
definePageMeta({ layout: 'auth' });
useHead({ title: '创建账号 · FastQuiz' });
const { $api } = useNuxtApp();
const router = useRouter();
const state = reactive({ username: '', password: '', confirmPassword: '' });
const loading = ref(false);
const error = ref('');
async function onRegister() {
  if (loading.value) return;
  error.value = '';
  if (state.password !== state.confirmPassword) {
    error.value = '两次输入的密码不一致。';
    return;
  }
  loading.value = true;
  try {
    await $api.auth.registerCreate({ username: state.username.trim(), password: state.password, email: '' });
    useToast().add({ title: '账号创建成功', description: '使用新账号登录，开始你的学习。', color: 'success' });
    await router.push('/');
  } catch (err: any) {
    error.value = err.response?.data?.title || '创建账号失败，请稍后重试。';
  } finally { loading.value = false; }
}
</script>
<template>
  <section class="auth-panel" aria-labelledby="register-title">
    <div class="auth-heading">
      <CommonBrandMark />
      <h1 id="register-title">新的开始。</h1>
      <p>创建账号，开启更有条理的学习。</p>
    </div>
    <UForm :state="state" @submit="onRegister" class="auth-form">
      <UFormField label="用户名" name="username" required>
        <UInput class="w-full" v-model="state.username" placeholder="3–20 位字母、数字或下划线" name="username" autocomplete="username" required minlength="3" maxlength="20" pattern="[a-zA-Z0-9_]+" :disabled="loading" />
      </UFormField>
      <UFormField label="密码" name="password" required>
        <UInput class="w-full" v-model="state.password" type="password" placeholder="设置密码，至少 6 位" name="password" autocomplete="new-password" required minlength="6" :disabled="loading" />
      </UFormField>
      <UFormField label="确认密码" name="confirmPassword" required>
        <UInput class="w-full" v-model="state.confirmPassword" type="password" placeholder="再次输入密码" name="confirmPassword" autocomplete="new-password" required :disabled="loading" />
      </UFormField>
      <p v-if="error" role="alert" class="text-sm text-error leading-relaxed">{{ error }}</p>
      <UButton type="submit" block :loading="loading" :disabled="loading">创建账号</UButton>
    </UForm>
    <div class="auth-divider" />
    <p class="auth-switch">已有账号？<NuxtLink to="/">返回登录</NuxtLink></p>
  </section>
</template>
