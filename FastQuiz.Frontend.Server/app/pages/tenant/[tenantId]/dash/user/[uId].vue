<template>
  <div>
    <UBreadcrumb :items="links" class="py-2" />
    <USeparator class="my-2" />
    <!-- 用户信息卡片 -->
    <UCard>
      <template #header>
        <div class="flex items-center justify-between">
          <div class="flex items-center gap-3">
            <UAvatar :text="user?.username?.charAt(0) ?? '?'" size="lg" />
            <div>
              <h2 class="text-lg font-semibold">
                {{ user?.username ?? "未知用户" }}
              </h2>
              <!-- <p class="text-sm text-gray-500">{{ user?.email ?? '暂无邮箱' }}</p> -->
            </div>
          </div>
          <UBadge color="primary">正常</UBadge>
        </div>
      </template>

      <!-- 信息列表 -->
      <div class="grid grid-cols-2 gap-4">
        <div class="flex flex-col gap-1">
          <span class="text-xs text-gray-500">用户 ID</span>
          <span class="font-mono text-sm">{{ user?.id ?? "-" }}</span>
        </div>
        <div class="flex flex-col gap-1">
          <span class="text-xs text-gray-500">用户名</span>
          <span class="text-sm">{{ user?.username ?? "-" }}</span>
        </div>
        <!-- <div class="flex flex-col gap-1">
          <span class="text-xs text-gray-500">邮箱</span>
          <span class="text-sm">{{ user?.email ?? '-' }}</span>
        </div>
        <div class="flex flex-col gap-1">
          <span class="text-xs text-gray-500">创建时间</span>
          <span class="text-sm">{{ user?.createdAt ?? '-' }}</span>
        </div> -->
      </div>
    </UCard>
  </div>
</template>

<script setup lang="ts">
const { $api } = useNuxtApp();
import type { BreadcrumbItem } from "@nuxt/ui";
const route = useRoute();

const tenantId = ref("");
const uId = ref("");

const user = ref<UserDto>();

const links = ref<BreadcrumbItem[]>([]);

const toast = useToast();

onMounted(async () => {
  tenantId.value = route.params.tenantId as string;
  uId.value = route.params.uId as string;

  await fetchUserAsync(uId.value);

  links.value.push({
    label: "用户管理",
    icon: "iconoir:user",
    to: `/tenant/${tenantId.value}/dash/user`,
  });

  links.value.push({
    label: user.value?.username ?? "未知用户",
    icon: "iconoir:user",
    to: `/tenant/${tenantId.value}/dash/user/${user.value?.id}`,
  });
});

const fetchUserAsync = async (id: string) => {
  let result;
  try {
    result = await $api.tenant.userDetail(tenantId.value, uId.value);
  } catch (e) {
    const error = e as any;
    const title = error.response.data.title;
    toast.add({ title: "获用户列表失败：" + title });
    return;
  }

  user.value = result.data;
};
</script>

<style scoped></style>
