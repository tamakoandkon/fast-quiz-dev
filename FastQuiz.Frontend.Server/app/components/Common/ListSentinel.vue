<template>
  <div ref="root" class="list-sentinel" role="status" :aria-busy="loading">
    <template v-if="loading">
      <span class="list-sentinel-dot" aria-hidden="true"></span>
      <span>正在加载</span>
    </template>
    <template v-else-if="end">
      <span class="list-sentinel-rule" aria-hidden="true"></span>
      <span>{{ endText }}</span>
      <span class="list-sentinel-rule" aria-hidden="true"></span>
    </template>
    <span v-else class="sr-only">继续向下滚动以加载更多</span>
  </div>
</template>

<script setup lang="ts">
// 观察列表尾部：进入视口就通知父级续页，替代常驻的「加载更多」按钮。
const props = defineProps({
  loading: { type: Boolean, default: false },
  end: { type: Boolean, default: false },
  endText: { type: String, default: "已显示全部内容" },
  disabled: { type: Boolean, default: false },
});

const emit = defineEmits<{ (e: "reach"): void }>();

const root = ref<HTMLElement | null>(null);
let observer: IntersectionObserver | null = null;

const notify = () => {
  if (props.disabled || props.loading || props.end) return;
  emit("reach");
};

onMounted(() => {
  if (typeof IntersectionObserver === "undefined") return;
  observer = new IntersectionObserver(
    (entries) => {
      if (entries.some((entry) => entry.isIntersecting)) notify();
    },
    { rootMargin: "0px 0px 320px 0px" },
  );
  if (root.value) observer.observe(root.value);
});

// 首屏数据回来后尾部可能已经在视口内，主动补一次判定。
watch(
  () => [props.disabled, props.loading, props.end],
  async () => {
    await nextTick();
    const el = root.value;
    if (!el || props.disabled || props.loading || props.end) return;
    const rect = el.getBoundingClientRect();
    if (rect.top <= window.innerHeight + 320) notify();
  },
);

onBeforeUnmount(() => {
  observer?.disconnect();
  observer = null;
});
</script>
