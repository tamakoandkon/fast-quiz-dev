<template>
  <!-- h-screen 确保容器铺满屏幕高度 -->
  <div class="flex h-screen w-full overflow-hidden bg-gray-100">
    <!-- 侧边栏：使用 USidebar -->
    <USidebar
      v-model:open="open"
      variant="inset"
      collapsible="icon"
      :ui="{
        container: 'h-full',
      }"
    >
      <template #header>
        <div class="flex items-center gap-2 px-2" v-if="open">
          <img src="/favicon-2048x2048.png" alt="Logo" class="h-8 w-auto" />
          <span class="font-bold text-lg">考试系统</span>
        </div>
      </template>

      <!-- 菜单 -->
      <UNavigationMenu
        :items="navigationMenuItems"
        orientation="vertical"
        :ui="{
          link: 'p-1.5 overflow-hidden',
        }"
      />

      <!-- 底部用户状态 + 额外操作 -->
      <template #footer>
        <div class="border-t border-default p-3 space-y-2">
          <div class="flex items-center gap-3">
            <UAvatar size="sm" />
            <div class="flex-1 min-w-0">
              <p class="text-sm font-medium truncate">{{ user.username }}</p>
              <!-- <p class="text-xs text-gray-500 dark:text-gray-400 truncate">
                admin@example.com
              </p> -->
            </div>
          </div>
          <div class="flex gap-2">
            <UButton size="sm" color="neutral" variant="outline" class="flex-1">
              设置
            </UButton>
            <UButton
              size="sm"
              color="neutral"
              variant="ghost"
              class="flex-1"
              @click="logOut"
            >
              退出
            </UButton>
          </div>
        </div>
      </template>
    </USidebar>

    <!-- 主内容区：flex-1 自动占据剩余空间，y轴滚动 -->
    <main class="flex-1 overflow-y-auto bg-gray-50">
      <!-- 折叠开关 -->
      <div
        class="sticky top-0 z-10 bg-gray-50 border-b border-default px-4 py-2"
      >
        <UButton
          :icon="open ? 'iconoir:sidebar-collapse' : 'iconoir:sidebar-expand'"
          color="neutral"
          variant="ghost"
          aria-label="Toggle sidebar"
          @click="
            () => {
              open = !open;
            }
          "
        />
      </div>
      <div class="p-6">
        <!-- 无论你访问 /dash 还是 /dash/settings，这东西永远都在 -->
        <slot />
        <!-- 只有这里的内容会随着路由切换而改变 -->
      </div>
    </main>
  </div>
</template>

<script setup lang="ts">
import type { NavigationMenuItem } from "@nuxt/ui";

const { $api } = useNuxtApp();
const toast = useToast();

const route = useRoute();
const router = useRouter();
const open = ref(true);

const navigationMenuItems = computed<NavigationMenuItem[]>(() => {
  const rawMenus = [
    {
      id: 1,
      title: "我的租户",
      link: `/tenant`,
      icon: "iconoir:long-arrow-up-left",
    },
  ];

  return rawMenus.map((item) => ({
    label: item.title,
    icon: item.icon,
    to: item.link,
    active: route.path === item.link,
  }));
});

const user = ref<UserDto>({});
onMounted(async () => {
  let result;
  try {
    result = await $api.auth.profileList();
    user.value = result.data;
  } catch (e: any) {
    const title = e.response.data.title;
    toast.add({ title: "获用户失败：" + title });
    return;
  }
});

const Store = useUserStore();

let logOut = () => {
  Store.logout();
  router.push("/");
};
</script>

<style scoped></style>
