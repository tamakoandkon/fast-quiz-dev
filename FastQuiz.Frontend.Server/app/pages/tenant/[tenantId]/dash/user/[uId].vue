<template>
  <CommonPageLayout :title="user?.username || '用户资料'" :description="user?.username ? '用户资料' : undefined">
    <template #actions>
      <UButton
        v-if="user?.id"
        color="neutral"
        variant="outline"
        icon="i-lucide-clipboard-list"
        :to="'/tenant/' + tenantId + '/dash/user-records/' + user.id"
      >查看成绩</UButton>
    </template>
    <section v-if="user" class="workspace-panel p-5" aria-label="账户信息">
      <dl class="grid gap-5 sm:grid-cols-2">
        <div>
          <dt class="text-sm text-muted">邮箱</dt>
          <dd class="mt-1 text-sm break-all">{{ user.email || "未填写邮箱" }}</dd>
        </div>
        <div>
          <dt class="text-sm text-muted">创建时间</dt>
          <dd class="mt-1 text-sm">{{ user.createdAt ? formatDate(user.createdAt) : "—" }}</dd>
        </div>
      </dl>
    </section>
  </CommonPageLayout>
</template>

<script setup lang="ts">
const { $api } = useNuxtApp();
const route = useRoute();

const tenantId = ref("");
const uId = ref("");

const user = ref<UserDto>();


const toast = useToast();

onMounted(async () => {
  tenantId.value = route.params.tenantId as string;
  uId.value = route.params.uId as string;

  await fetchUserAsync(uId.value);


});

const fetchUserAsync = async (id: string) => {
  let result;
  try {
    result = await $api.tenant.userDetail(tenantId.value, id);
  } catch (e) {
    const error = e as any;
    const title = error.response?.data?.title ?? "未知错误";
    toast.add({ title: "获取用户信息失败：" + title, color: "error" });
    return;
  }

  user.value = result.data;
};
</script>

<style scoped></style>
