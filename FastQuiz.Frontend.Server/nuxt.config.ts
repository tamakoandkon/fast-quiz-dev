// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  ssr: false,

  nitro: {
    preset: "static",
    compressPublicAssets: {
      gzip: true,
      brotli: true,
    },
  },

  compatibilityDate: "2025-07-15",
  devtools: { enabled: true },
  colorMode: { preference: "light", fallback: "light" },
  icon: { clientBundle: { scan: true } },
  app: {
    head: {
      title: "FastQuiz · 考试与学习",
      htmlAttrs: { lang: "zh-CN" },
      meta: [{ name: "theme-color", content: "#f5f5f7" }],
    },
  },
  modules: [
    [
      "@pinia/nuxt",
      {
        autoImports: [
          // 自动引入 `defineStore()`
          "defineStore",
          // 自动引入 `defineStore()` 并重命名为 `definePiniaStore()`
          ["defineStore", "definePiniaStore"],
        ],
      },
    ],
    "@nuxt/ui",
  ],
  css: ["~/assets/css/main.css"],
  ui: {
    fonts: false,
  },
  telemetry: false,
  vite: {
    optimizeDeps: {
      include: [
        "@nuxt/ui > prosemirror-state",
        "@nuxt/ui > prosemirror-transform",
        "@nuxt/ui > prosemirror-model",
        "@nuxt/ui > prosemirror-view",
        "@nuxt/ui > prosemirror-gapcursor",
      ],
    },
  },
});
