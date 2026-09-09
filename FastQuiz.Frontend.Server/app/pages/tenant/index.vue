<template>
  <CommonManagePageContainer>
    <UBreadcrumb :items="links" class="py-2" />
    <USeparator class="my-2" />
    <!-- 卡片网格 -->
    <div class="relative">
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        <UCard
          v-for="tenant in tenants"
          :key="tenant.id || ''"
          class="cursor-pointer hover:shadow-md transition-shadow duration-200"
          @click="() => router.push(`/tenant/${tenant.id}`)"
        >
          <template #header>
            <div class="flex items-center justify-between">
              <span class="font-medium text-gray-900 truncate">
                {{ tenant.name || "未命名租户" }}
              </span>
              <UTooltip v-if="tenant.id" text="点击查看详情">
                <UIcon name="i-lucide-chevron-right" class="text-gray-400" />
              </UTooltip>
            </div>
          </template>

          <!-- 卡片内容：显示租户基本信息 -->
          <div class="text-sm text-gray-500 space-y-1">
            {{ tenant.description }}
          </div>
        </UCard>
      </div>
    </div>
    <CommonLoadMoreButton
      v-if="haveMore"
      @click="() => fetchList(false, pageSize, false)"
      :disabled="!haveMore"
    />
  </CommonManagePageContainer>
</template>

<script setup lang="ts">
definePageMeta({ layout: "tenant" });
const { $api } = useNuxtApp();
import type { BreadcrumbItem } from "@nuxt/ui";

const router = useRouter();

const links = ref<BreadcrumbItem[]>([
  {
    label: "我的租户",
    icon: "iconoir:home-simple",
    to: "/tenant",
  },
]);

const toast = useToast();

const tenants = ref<TenantDto[]>([]);
const pageSize = ref(30);

onMounted(async () => {
  await fetchList(false, pageSize.value, true);
  // 检查租户数量，如果只有一个则自动跳转
  if (tenants.value.length === 1) {
    const tenantId = tenants.value[0]?.id;
    if (tenantId) {
      router.push(`/tenant/${tenantId}`);
    }
  }
});

const haveMore = ref(true);

const fetchList = async (
  isPre: boolean,
  pageSize: number,
  refresh: boolean = false,
) => {
  let cusor = getCursor(isPre);
  if (refresh) {
    cusor = undefined;
  }
  let result;
  try {
    result = await $api.tenant.getTenant({
      Cursor: cusor,
      Count: pageSize,
      IsPrevious: isPre,
    });
  } catch (e: any) {
    const title = e.response.data.title;
    toast.add({ title: "获取租户列表失败：" + title });
    return;
  }

  if (refresh) {
    tenants.value = result.data;
  } else {
    if (isPre) {
      tenants.value = [...result.data, ...tenants.value];
    } else {
      tenants.value = [...tenants.value, ...result.data];
    }
  }

  haveMore.value = result.data.length == pageSize;
};

const getCursor = (isPre: boolean) => {
  if (tenants.value.length == 0) {
    return undefined;
  }
  if (isPre) {
    return tenants.value[0]?.id as string;
  } else {
    return tenants.value[tenants.value.length - 1]?.id as string;
  }
};
</script>
