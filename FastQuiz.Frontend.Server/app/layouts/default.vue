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
        <div class="border-t border-default p-3 space-y-2" v-show="open">
          <div class="flex items-center gap-3">
            <!-- <UAvatar size="sm" /> -->
            <div class="flex-1 min-w-0">
              <p class="text-sm font-medium truncate">{{ user.username }}</p>
              <!-- <p class="text-xs text-gray-500 dark:text-gray-400 truncate">
                
              </p> -->
            </div>
          </div>
          <div class="flex gap-2">
            <UButton
              size="sm"
              color="neutral"
              variant="outline"
              class="flex-1"
              @click="
                () => {
                  showEditMeModal = true;
                }
              "
            >
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
    <UserEditMeProfileModal v-model="showEditMeModal"></UserEditMeProfileModal>
  </div>
</template>

<script setup lang="ts">
import type { NavigationMenuItem } from "@nuxt/ui";

const { $api } = useNuxtApp();
const toast = useToast();

const route = useRoute();
const router = useRouter();
const open = ref(true);
const showEditMeModal = ref(false);

const tenantId = computed(() => route.params.tenantId as string);

const permissions = ref<string[]>([]);

// ============ 新增代码开始 ============
// 定义权限常量
const PERMISSIONS = {
  SYSTEM: "system",
  TENANT: "tenant",
  ORGANIZATION: "organization",
  ROLE: "role",
  USER: "user",
  QUESTION: "question",
  QUIZ_TEMPLATE: "quiz-template",
  QUIZ_SNAPSHOT: "quiz-snapshot",
  QUIZ_ATTEMPT: "quiz-attempt",
} as const;

// 定义菜单项与权限的映射关系
const menuPermissionMap: Record<number, string> = {
  2: PERMISSIONS.QUIZ_TEMPLATE, // 试卷管理
  8: PERMISSIONS.TENANT, // 租户管理
  9: PERMISSIONS.ORGANIZATION, // 组织管理
  10: PERMISSIONS.USER, // 用户管理
  11: PERMISSIONS.ROLE, // 角色管理
  12: PERMISSIONS.QUESTION, // 题库管理
};

// 检查用户是否有指定权限
const hasPermission = (permission?: string): boolean => {
  if (!permission) return true;
  return permissions.value.includes(permission);
};
// ============ 新增代码结束 ============

const navigationMenuItems = computed<NavigationMenuItem[]>(() => {
  const rawMenus = [
    {
      id: 1,
      title: "首页",
      link: `/tenant/${tenantId.value}`,
      icon: "iconoir:home-simple",
    },
    {
      id: 2,
      title: "试卷管理",
      link: `/tenant/${tenantId.value}/dash/quiz-template`,
      icon: "iconoir:atom",
    },
    {
      id: 8,
      title: "租户管理",
      link: `/tenant/manage`,
      icon: "iconoir:city",
    },
    {
      id: 9,
      title: "组织管理",
      link: `/tenant/${tenantId.value}/dash/organization`,
      icon: "iconoir:group",
    },
    {
      id: 10,
      title: "用户管理",
      link: `/tenant/${tenantId.value}/dash/user`,
      icon: "iconoir:user",
    },
    {
      id: 11,
      title: "角色管理",
      link: `/tenant/${tenantId.value}/dash/role`,
      icon: "iconoir:user-circle",
    },
    {
      id: 12,
      title: "题库管理",
      link: `/tenant/${tenantId.value}/dash/question-group`,
      icon: "iconoir:question-mark-circle",
    },
    { id: 13, title: "我的租户", link: "/tenant", icon: "iconoir:user-crown" },
    {
      id: 14,
      title: "我的考试",
      link: `/tenant/${tenantId.value}/my-quiz-attempt`,
      icon: "iconoir:fire-flame",
    },
    {
      id: 15,
      title: "考试中心",
      link: `/tenant/${tenantId.value}/quiz-attempt`,
      icon: "iconoir:database-script",
    },
  ];

  // ============ 新增：过滤菜单 ============
  const filteredMenus = rawMenus.filter(
    (item) =>
      !menuPermissionMap[item.id] || hasPermission(menuPermissionMap[item.id]),
  );
  // ============ 新增结束 ============

  return filteredMenus.map((item) => ({
    label: item.title,
    icon: item.icon,
    to: item.link,
    active: route.path === item.link,
  }));
});
// const navigationMenuItems = computed<NavigationMenuItem[]>(() => {
//   const rawMenus = [
//     {
//       id: 1,
//       title: "首页",
//       link: `/tenant/${tenantId.value}`,
//       icon: "iconoir:home-simple",
//     },
//     {
//       id: 2,
//       title: "试卷管理",
//       link: `/tenant/${tenantId.value}/dash/quiz-template`,
//       icon: "iconoir:atom",
//     },
//     {
//       id: 8,
//       title: "租户管理",
//       link: `/tenant/manage`,
//       icon: "iconoir:city",
//     },
//     {
//       id: 9,
//       title: "组织管理",
//       link: `/tenant/${tenantId.value}/dash/organization`,
//       icon: "iconoir:group",
//     },
//     {
//       id: 10,
//       title: "用户管理",
//       link: `/tenant/${tenantId.value}/dash/user`,
//       icon: "iconoir:user",
//     },
//     {
//       id: 11,
//       title: "角色管理",
//       link: `/tenant/${tenantId.value}/dash/role`,
//       icon: "iconoir:user-circle",
//     },
//     {
//       id: 2,
//       title: "题库管理",
//       link: `/tenant/${tenantId.value}/dash/question-group`,
//       icon: "iconoir:question-mark-circle",
//     },
//     { id: 12, title: "我的租户", link: "/tenant", icon: "iconoir:user-crown" },
//     {
//       id: 13,
//       title: "我的考试",
//       link: `/tenant/${tenantId.value}/my-quiz-attempt`,
//       icon: "iconoir:fire-flame",
//     },
//     {
//       id: 14,
//       title: "考试中心",
//       link: `/tenant/${tenantId.value}/quiz-attempt`,
//       icon: "iconoir:database-script",
//     },
//   ];

//   return rawMenus.map((item) => ({
//     label: item.title,
//     icon: item.icon,
//     to: item.link,
//     active: route.path === item.link,
//   }));
// });

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

  await fetchPermissions();
});

const Store = useUserStore();

const logOut = () => {
  Store.logout();
  router.push("/");
};

const fetchPermissions = async () => {
  let tenantId = route.params.tenantId as string;
  let result;
  try {
    result = await $api.tenant.rolePermissionMeList(tenantId);
    permissions.value = result.data;
    console.log(permissions.value);
  } catch (ex: any) {
    let title = ex.response.data.title;
    toast.add({ title: "获取用户权限失败", color: "error" });
  }
};
</script>

<style scoped></style>
