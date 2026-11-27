<script setup lang="ts">
const props = withDefaults(defineProps<{
  pageSizeOptions?: number[];
  currentSize?: number;
  busy?: boolean;
}>(), {
  pageSizeOptions: () => [10, 20, 50, 100],
  currentSize: 0,
  busy: false,
});
const pageIndex = ref(0);
const pageSize = defineModel<number>({ default: 10 });
const emit = defineEmits<{
  change: [isPrevious: boolean, pageSize: number, refresh: boolean];
}>();

function changePage(offset: number) {
  pageIndex.value += offset;
  emit("change", offset < 0, pageSize.value, false);
}

function changeSize(size: number) {
  pageSize.value = size;
  pageIndex.value = 0;
  emit("change", false, size, true);
}
</script>

<template>
  <CommonListFooter
    :page-index="pageIndex"
    :page-size="pageSize"
    :item-count="props.currentSize"
    :can-prev="pageIndex > 0"
    :can-next="props.currentSize === pageSize"
    :busy="props.busy"
    :size-options="props.pageSizeOptions"
    @prev="changePage(-1)"
    @next="changePage(1)"
    @update:page-size="changeSize"
  />
</template>
