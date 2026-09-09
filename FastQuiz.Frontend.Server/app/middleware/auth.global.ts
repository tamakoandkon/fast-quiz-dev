export default defineNuxtRouteMiddleware(async (to, from) => {
  // 1. 只在客户端执行（安全起见，防止服务端报错）
  if (import.meta.server) return;

  // 2. 获取 Pinia 的 userStore 实例
  const userStore = useUserStore(); // Nuxt 会自动导入 stores 目录下的 useUserStore

  // 3. 如果 store 里没有用户信息，尝试从持久化存储（如 cookie/localStorage）恢复
  // 这一步需要根据你实际存储 token 的方式调整
  if (!userStore.isAuthenticated) {
    // 假设你用 useCookie 存储 token
    await userStore.fetchCurrentUser();
  }

  // 4. 如果用户已登录，但访问的是登录页，重定向到首页
  if (
    userStore.isAuthenticated &&
    (to.path === "/" || to.path === "/register")
  ) {
    return navigateTo("/tenant");
  }
});
