<template>
  <UModal v-model:open="open" title="添加已有账号" :dismissible="!busy">
    <template #body>
      <form class="space-y-5" @submit.prevent="submit">
        <p class="text-sm text-muted">
          将已有账号加入「{{ tenantName }}」。不会重建账号、修改密码或授予管理权限。
        </p>
        <UFormField label="完整用户名" required>
          <UInput v-model="username" class="w-full" autocomplete="off" :maxlength="20"
            placeholder="输入已注册账号的用户名" :disabled="busy" />
        </UFormField>
        <p v-if="error" role="alert" class="text-sm text-error">{{ error }}</p>
        <div class="flex justify-end gap-2">
          <UButton color="neutral" variant="ghost" :disabled="busy" @click="open = false">取消</UButton>
          <UButton type="submit" :loading="busy" :disabled="username.trim().length < 3">确认加入</UButton>
        </div>
      </form>
    </template>
  </UModal>
</template>

<script setup lang="ts">
const open = defineModel<boolean>({ default: false });
const props = defineProps<{ tenantId: string; tenantName: string }>();
const emit = defineEmits<{ added: [] }>();
const { $api } = useNuxtApp();
const toast = useToast();
const username = ref("");
const busy = ref(false);
const error = ref("");
watch(open, (value) => {
  if (value) { username.value = ""; error.value = ""; }
});
const submit = async () => {
  if (busy.value || !props.tenantId || username.value.trim().length < 3) return;
  busy.value = true;
  error.value = "";
  try {
    const result = await $api.tenant.membersCreate(props.tenantId, { username: username.value.trim() });
    toast.add({ title: `${result.data.username} 已加入租户`, color: "success" });
    open.value = false;
    emit("added");
  } catch (e: any) {
    error.value = e?.response?.data?.title ?? "加入失败，请稍后重试";
  } finally { busy.value = false; }
};
</script>
