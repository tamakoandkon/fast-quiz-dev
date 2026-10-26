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

const sections = computed(() => {
  if (props.tenantOnly) {
    return [{
      label: '工作空间',
      items: [
        { label: '我的租户', to: '/tenant', icon: 'i-lucide-grid-2x2' },
        ...(isSystemAdmin.value ? [{ label: '租户管理', to: '/tenant/manage', icon: 'i-lucide-building-2' }] : []),
      ],
    }];
  }
  return [
    { label: '工作空间', items: [
      { label: '概览', to: base.value, icon: 'i-lucide-house' },
      { label: '考试中心', to: base.value + '/quiz-attempt', icon: 'i-lucide-graduation-cap' },
      { label: '我的考试', to: base.value + '/my-quiz-attempt', icon: 'i-lucide-chart-no-axes-column-increasing' },
    ] },
    { label: '内容管理', items: [
      { label: '试卷管理', to: base.value + '/dash/quiz-template', icon: 'i-lucide-file-text', permission: 'quiz-template' },
      { label: '题库管理', to: base.value + '/dash/question-group', icon: 'i-lucide-library', permission: 'question' },
    ].filter(item => can(item.permission)) },
    { label: '团队与设置', items: [
      { label: '组织管理', to: base.value + '/dash/organization', icon: 'i-lucide-building-2', permission: 'organization' },
      { label: '用户管理', to: base.value + '/dash/user', icon: 'i-lucide-users', permission: 'user' },
      { label: '角色管理', to: base.value + '/dash/role', icon: 'i-lucide-shield-check', permission: 'role' },
      { label: '租户管理', to: '/tenant/manage', icon: 'i-lucide-settings-2', permission: 'tenant' },
    ].filter(item => can(item.permission)) },
    { label: '切换空间', items: [{ label: '我的租户', to: '/tenant', icon: 'i-lucide-grid-2x2' }] },
  ].filter(section => section.items.length);
});

const isActive = (to: string) => {
  if (to === base.value || to === '/tenant') return route.path === to;
  return route.path === to || route.path.startsWith(to + '/');
};
const pageTitle = computed(() => {
  const items = sections.value.flatMap(section => section.items);
  return items.filter(item => isActive(item.to)).sort((a, b) => b.to.length - a.to.length)[0]?.label || '工作空间';
});
const username = computed(() => userStore.user?.username || '我的账号');
const avatar = computed(() => username.value.charAt(0).toUpperCase());
const logout = () => { userStore.logout(); router.push('/'); };
watch(() => route.path, () => { mobileOpen.value = false; });
watch(tenantId, loadPermissions);
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
          <NuxtLink v-for="item in section.items" :key="item.to" :to="item.to" class="workspace-link" :class="{ 'is-active': isActive(item.to) }" :aria-current="isActive(item.to) ? 'page' : undefined">
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
        <div class="workspace-context">
          <UButton class="mobile-menu-toggle" color="neutral" variant="ghost" icon="i-lucide-menu" aria-label="打开导航" aria-controls="workspace-navigation" :aria-expanded="mobileOpen" @click="mobileOpen = true" />
          <strong>{{ pageTitle }}</strong>
        </div>
        <UButton size="sm" color="neutral" variant="ghost" icon="i-lucide-circle-user-round" aria-label="账户设置" @click="showProfile = true"><span class="hidden sm:inline">{{ username }}</span></UButton>
      </header>
      <main class="workspace-content"><slot /></main>
    </div>
    <LazyUserEditMeProfileModal v-if="showProfile" v-model="showProfile" />
  </div>
</template>
