<template>
  <footer class="data-list-footer">
    <p class="data-list-page-note">
      第 {{ pageIndex + 1 }} 页 · 本页 {{ itemCount }} {{ unit }}
    </p>
    <div class="data-list-page-controls">
      <label class="data-list-size">
        <span>每页</span>
        <USelect
          :model-value="pageSize"
          :items="sizeOptions"
          size="sm"
          class="w-20"
          :aria-label="`每页显示${unit}数`"
          @update:model-value="onSizeChange"
        />
        <span>{{ unit }}</span>
      </label>
      <div class="data-list-page-buttons">
        <UButton
          color="neutral"
          variant="outline"
          size="sm"
          icon="i-lucide-chevron-left"
          :disabled="!canPrev || busy"
          @click="emit('prev')"
        >
          上一页
        </UButton>
        <UButton
          color="neutral"
          variant="outline"
          size="sm"
          trailing-icon="i-lucide-chevron-right"
          :disabled="!canNext || busy"
          @click="emit('next')"
        >
          下一页
        </UButton>
      </div>
    </div>
  </footer>
</template>

<script setup lang="ts">
// 游标分页拿不到总数，因此只报「当前第几页 / 本页多少条」。
const props = defineProps({
  pageIndex: { type: Number, default: 0 },
  pageSize: { type: Number, default: 10 },
  itemCount: { type: Number, default: 0 },
  unit: { type: String, default: "条" },
  canPrev: { type: Boolean, default: false },
  canNext: { type: Boolean, default: false },
  busy: { type: Boolean, default: false },
  sizeOptions: { type: Array as () => number[], default: () => [10, 20, 50] },
});

const emit = defineEmits<{
  (e: "prev"): void;
  (e: "next"): void;
  (e: "update:pageSize", value: number): void;
}>();

const onSizeChange = (value: unknown) => {
  const next = Number(value);
  if (!Number.isFinite(next) || next === props.pageSize) return;
  emit("update:pageSize", next);
};
</script>
