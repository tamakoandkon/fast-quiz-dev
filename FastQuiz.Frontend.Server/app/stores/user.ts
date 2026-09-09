import { defineStore } from "pinia";
import { ref, computed } from "vue";

interface User {
  id: string;
  username: string;
}

export const useUserStore = defineStore("user", () => {
  const { $api } = useNuxtApp(); // 获取插件

  // 使用标志位防止重复初始化
  const isInitialized = ref(false);

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
  async function fetchCurrentUser() {
    if (!token.value) return;
    try {
      const response = await $api.auth.profileList();
      // 安全赋值
      if (response?.data) {
        user.value = {
          id: String(response.data.id ?? ""),
          username: String(response.data.username ?? "Unknown"),
        };
      }
    } catch (error) {
      console.error("Failed to fetch user profile:", error);
      logout();
    }
  }

  // 显式初始化方法
  async function init() {
    if (isInitialized.value) return;
    isInitialized.value = true;
    if (token.value) {
      await fetchCurrentUser();
    }
  }

  // 1. 设置 token 并尝试自动获取用户信息
  function setToken(newToken: string) {
    token.value = newToken;
    fetchCurrentUser();
  }

  // 3. 登出
  function logout() {
    // 1. 清除本地响应式状态
    user.value = null;

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
