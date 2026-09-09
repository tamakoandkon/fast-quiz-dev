// plugins/api.ts
import { Api } from "~/utils/backEnd";

export default defineNuxtPlugin(async () => {
  console.log("--- API Plugin Loading ---");

  let baseURL = "https://localhost:7040"; // 默认值

  try {
    const response = await fetch("/config.json");
    const config = await response.json();
    baseURL = config.apiBaseUrl;
  } catch (error) {
    console.error("Failed to load config, using default:", error);
  }

  // 1. 直接实例化（它内部会自建一个 axios 实例，这样 this 绑定最稳固）
  const api = new Api({
    baseURL: baseURL,
    timeout: 10000,
  });

  // 2. 获取它内部的 axios 实例进行配置
  const instance = api.instance;

  // 3. 请求拦截器：增加客户端判断（防止 SSR 报错）
  instance.interceptors.request.use((config) => {
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
        return navigateTo("/");
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
