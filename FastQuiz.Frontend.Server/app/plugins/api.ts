// plugins/api.ts
import { Api } from "~/utils/backEnd";

export default defineNuxtPlugin(() => {
  // Load runtime overrides alongside route chunks, without delaying the login UI.
  // API requests still wait for the override so custom deployments keep working.
  const configuration = fetch("/config.json", { signal: AbortSignal.timeout(3000) })
    .then(async (response) => {
      if (!response.ok) throw new Error("Failed to load API configuration");
      const config = await response.json();
      return typeof config.apiBaseUrl === "string" ? config.apiBaseUrl : "/api";
    })
    .catch((error) => {
      console.error("Failed to load config, using default:", error);
      return "/api";
    });

  // 1. 直接实例化（它内部会自建一个 axios 实例，这样 this 绑定最稳固）
  const api = new Api({
    baseURL: "/api",
    timeout: 10000,
  });

  // 2. 获取它内部的 axios 实例进行配置
  const instance = api.instance;

  // 3. 请求拦截器：增加客户端判断（防止 SSR 报错）
  instance.interceptors.request.use(async (config) => {
    config.baseURL = await configuration;
    // 使用 Nuxt 的 useCookie 来统一管理 token
    const token = useCookie("cw_auth_token").value;
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  });

  // 4. 响应拦截器
  instance.interceptors.response.use(
    (response) => response, // 暂时不要在这里脱壳数据，先保证流程通畅
    (error) => {
      if (error.response?.status === 401) {
        const token = useCookie("cw_auth_token").value;
        const authorization = token ? `Bearer ${token}` : undefined;
        // An expired request from the previous account must not redirect a new session.
        if (error.config?.headers?.Authorization === authorization) void navigateTo("/");
      }
      return Promise.reject(error);
    },
  );

  return {
    provide: {
      api: api,
    },
  };
});
