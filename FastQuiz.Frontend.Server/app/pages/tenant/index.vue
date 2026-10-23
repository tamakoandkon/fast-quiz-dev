<template>
  <CommonManagePageContainer>
    <header class="workspace-hero">
      <h1 class="workspace-title">我的租户</h1>
    </header>
    <div class="workspace-grid">
      <NuxtLink v-for="tenant in tenants" :key="tenant.id || ''" :to="`/tenant/${tenant.id}`" class="workspace-card tenant-card">
        <h2>{{ tenant.name || "未命名租户" }}</h2>
        <UIcon name="i-lucide-arrow-up-right" class="workspace-card-arrow" aria-hidden="true" />
      </NuxtLink>
    </div>
    <div v-if="!loaded" class="exam-empty" role="status">正在加载租户…</div>
    <div v-else-if="loadError && tenants.length === 0" class="exam-empty">
      <p>租户列表暂时无法加载。</p>
      <UButton color="neutral" variant="outline" @click="fetchList(false, pageSize, true)">重试</UButton>
    </div>
    <div v-else-if="tenants.length === 0" class="exam-empty">暂无可进入的租户。</div>
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
const router = useRouter();
const toast = useToast();

const tenants = ref<TenantDto[]>([]);
const pageSize = ref(30);
const loaded = ref(false);
const loadError = ref(false);

onMounted(async () => {
  await fetchList(false, pageSize.value, true);
  loaded.value = true;
  if (tenants.value.length === 1 && tenants.value[0]?.id) {
    await router.replace(`/tenant/${tenants.value[0].id}`);
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
    loadError.value = true;
    const title = e?.response?.data?.title ?? "请稍后重试";
    toast.add({ title: "获取租户列表失败：" + title });
    return;
  }

  loadError.value = false;

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
