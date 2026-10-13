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

  compatibilityDate: "2026-10-14",
  // Stable DevTools still imports simple-git's removed default export.
  // Keep it disabled until it supports the patched simple-git 4 release.
  devtools: { enabled: false },
  colorMode: { preference: "light", fallback: "light" },
  icon: { clientBundle: { scan: true } },
  app: {
    head: {
      title: "FastQuiz · 考试与学习",
      htmlAttrs: { lang: "zh-CN" },
      meta: [{ name: "theme-color", content: "#f5f5f7" }],
    },
  },
  modules: ["@pinia/nuxt", "@nuxt/ui"],
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
