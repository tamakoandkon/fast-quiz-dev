import { defineStore } from "pinia";
import { ref, computed } from "vue";

interface User {
  id: string;
  username: string;
}

export const useUserStore = defineStore("user", () => {
  const { $api } = useNuxtApp(); // 获取插件

  // Request state belongs to this store/session, never to a module-global cache.
  let loadedToken: string | null = null;
  let generation = 0;
  let profileRequest: { token: string; promise: Promise<void> } | null = null;
  const permissions = useState<string[]>("workspace-permissions", () => []);
  const permissionsTenant = useState<string>("workspace-permissions-tenant", () => "");

  // --- State ---
  const user = ref<User | null>(null);

  // 使用 Nuxt 的 useCookie，它会自动同步到浏览器 Cookie
  // 设置 max-age 等选项确保在关闭浏览器后保持登录状态
  const token = useCookie<string | null>("cw_auth_token", {
    maxAge: 60 * 60 * 24 * 7, // 7天有效期
    sameSite: "lax",
  });

  // --- Getters ---
  const isAuthenticated = computed(() => !!user.value);
  const userName = computed(() => user.value?.username || "Guest");

  // --- Actions ---

  // 2. 从后端拉取用户信息
  async function fetchCurrentUser(force = false) {
    const requestedToken = token.value;
    if (!requestedToken) return;
    if (profileRequest?.token === requestedToken) return profileRequest.promise;
    if (!force && loadedToken === requestedToken && user.value) return;
    const requestedGeneration = generation;
    const promise = (async () => {
      try {
        const response = await $api.auth.profileList();
        // A delayed response must not restore a logged-out or different account.
        if (token.value !== requestedToken || generation !== requestedGeneration) return;
        if (response?.data) {
          user.value = {
            id: String(response.data.id ?? ""),
            username: String(response.data.username ?? "Unknown"),
          };
          loadedToken = requestedToken;
        }
      } catch (error) {
        if (token.value !== requestedToken || generation !== requestedGeneration) return;
        console.error("Failed to fetch user profile:", error);
        logout();
      }
    })();
    profileRequest = { token: requestedToken, promise };
    try {
      await promise;
    } finally {
      if (profileRequest?.promise === promise) profileRequest = null;
    }
  }

  // 显式初始化方法
  async function init() {
    await fetchCurrentUser();
  }

  // 1. 设置 token 并尝试自动获取用户信息
  function setToken(newToken: string) {
    resetSession();
    token.value = newToken;
    return fetchCurrentUser();
  }

  function resetSession() {
    generation += 1;
    loadedToken = null;
    profileRequest = null;
    user.value = null;
    permissions.value = [];
    permissionsTenant.value = "";
  }

  // 3. 登出
  function logout() {
    // 1. 清除本地响应式状态
    resetSession();

    // 2. 清除 Cookie
    token.value = null;

    const cookie = useCookie("cw_auth_token");
    cookie.value = null;
  }

  return {
    user,
    token,
    isAuthenticated,
    userName,
    setToken,
    fetchCurrentUser,
    logout,
    init,
  };
});
