<script setup lang="ts">
const props = defineProps<{ tenantOnly?: boolean }>();
const route = useRoute();
const router = useRouter();
const userStore = useUserStore();
const { tenantId, can, loadPermissions } = useWorkspaceAccess();
const mobileOpen = ref(false);
const isMobile = ref(false);
let mediaQuery: MediaQueryList | undefined;
const syncViewport = () => {
  isMobile.value = !!mediaQuery?.matches;
  if (!isMobile.value) mobileOpen.value = false;
};
const showProfile = ref(false);
const isSystemAdmin = computed(() => userStore.user?.id === 'default_admin');
const base = computed(() => '/tenant/' + tenantId.value);

const location = computed(() => workspaceLocation(route.path));
const sections = computed(() => {
  const items = workspaceNavigation(base.value, !!props.tenantOnly, isSystemAdmin.value || location.value.root.to === "/tenant/manage")
    .filter(item => props.tenantOnly || can(item.permission));
  return [...new Set(items.map(item => item.section))].map(label => ({
    label,
    items: items.filter(item => item.section === label),
  }));
});
const isActive = (to: string) => location.value.root.to === to;
const username = computed(() => userStore.user?.username || '我的账号');
const avatar = computed(() => username.value.charAt(0).toUpperCase());
const logout = () => { userStore.logout(); router.push('/'); };
watch(() => route.path, () => { mobileOpen.value = false; });
watch([tenantId, () => userStore.token], loadPermissions);
watch(mobileOpen, async (open) => {
  document.body.style.overflow = open ? 'hidden' : '';
  await nextTick();
  const selector = open ? '.sidebar-close' : '.mobile-menu-toggle';
  if (isMobile.value) document.querySelector<HTMLButtonElement>(selector)?.focus();
});
onUnmounted(() => {
  document.body.style.overflow = '';
  mediaQuery?.removeEventListener('change', syncViewport);
});
onMounted(async () => {
  mediaQuery = window.matchMedia('(max-width: 767px)');
  syncViewport();
  mediaQuery.addEventListener('change', syncViewport);
  await Promise.all([userStore.init(), loadPermissions()]);
});
</script>

<template>
  <div class="workspace" @keydown.esc="mobileOpen = false">
    <button v-if="mobileOpen" class="sidebar-backdrop" aria-label="关闭导航菜单" @click="mobileOpen = false" />
    <aside id="workspace-navigation" class="workspace-sidebar" :class="{ 'is-open': mobileOpen }" :inert="isMobile && !mobileOpen" aria-label="主导航" @keydown.esc="mobileOpen = false">
      <UButton class="sidebar-close" icon="i-lucide-x" color="neutral" variant="ghost" aria-label="关闭导航" @click="mobileOpen = false" />
      <NuxtLink to="/tenant" class="workspace-brand">
        <CommonBrandMark />
        <strong>FastQuiz</strong>
      </NuxtLink>
      <nav class="workspace-navigation">
        <section v-for="section in sections" :key="section.label">
          <p class="nav-section">{{ section.label }}</p>
          <NuxtLink v-for="item in section.items" :key="item.to" :to="item.to" class="workspace-link" :class="{ 'is-active': isActive(item.to) }" :aria-current="isActive(item.to) ? (location.parents.length ? 'location' : 'page') : undefined">
            <UIcon :name="item.icon" /><span>{{ item.label }}</span>
          </NuxtLink>
        </section>
      </nav>
      <div class="workspace-user">
        <div class="user-identity">
          <span class="user-avatar">{{ avatar }}</span>
          <div class="min-w-0"><strong>{{ username }}</strong><small>{{ isSystemAdmin ? '系统管理员' : '个人账号' }}</small></div>
        </div>
        <div class="flex gap-2">
          <UButton class="flex-1 justify-center" size="sm" color="neutral" variant="ghost" icon="i-lucide-sliders-horizontal" @click="showProfile = true">个人设置</UButton>
          <UButton size="sm" color="neutral" variant="ghost" icon="i-lucide-log-out" aria-label="退出登录" @click="logout" />
        </div>
      </div>
    </aside>
    <div class="workspace-main" :inert="isMobile && mobileOpen">
      <header class="workspace-topbar">
        <div class="workspace-topbar-inner">
          <div class="workspace-context">
            <UButton class="mobile-menu-toggle" color="neutral" variant="ghost" icon="i-lucide-menu" aria-label="打开导航" aria-controls="workspace-navigation" :aria-expanded="mobileOpen" @click="mobileOpen = true" />
            <nav v-if="location.parents.length" class="workspace-crumbs" aria-label="当前位置">
              <template v-for="(crumb, index) in location.parents" :key="crumb.to">
                <UIcon v-if="index" name="i-lucide-chevron-right" aria-hidden="true" />
                <NuxtLink :to="crumb.to">{{ crumb.label }}</NuxtLink>
              </template>
              <UIcon name="i-lucide-chevron-right" aria-hidden="true" />
              <span aria-current="page">{{ location.title }}</span>
            </nav>
            <strong v-else>{{ location.root.section }}</strong>
          </div>
          <UButton size="sm" color="neutral" variant="ghost" icon="i-lucide-circle-user-round" aria-label="账户设置" @click="showProfile = true"><span class="hidden sm:inline">{{ username }}</span></UButton>
        </div>
      </header>
      <main class="workspace-content"><slot /></main>
    </div>
    <LazyUserEditMeProfileModal v-if="showProfile" v-model="showProfile" />
  </div>
</template>
