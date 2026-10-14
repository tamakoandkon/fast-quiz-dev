// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  ssr: false,

  experimental: {
    defaults: {
      nuxtLink: {
        // A workspace exposes many routes; prefetch only when a user intends to visit one.
        prefetchOn: { visibility: false, interaction: true },
      },
    },
  },

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
      link: [
        { rel: "icon", type: "image/svg+xml", href: "/favicon.svg?v=black-fq-20261014" },
        { rel: "icon", type: "image/x-icon", href: "/favicon.ico?v=black-fq-20261014", sizes: "16x16 32x32 48x48 64x64 128x128 256x256" },
        { rel: "apple-touch-icon", href: "/apple-touch-icon.png?v=black-fq-20261014", sizes: "180x180" },
      ],
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
