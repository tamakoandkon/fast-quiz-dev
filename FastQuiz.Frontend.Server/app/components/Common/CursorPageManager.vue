<template>
  <div class="flex items-center gap-4 p-4 w-full justify-center">
    <UButton
      @click="handlePrevPage"
      :disabled="!nextAble"
      :color="nextAble ? 'primary' : 'warning'"
    >
      上一页
    </UButton>

    <UButton
      @click="handleNextPage"
      :disabled="!preAble"
      :color="preAble ? 'primary' : 'warning'"
    >
      下一页
    </UButton>

    <USelect
      v-model="pageSize"
      :items="pageSizeOptions"
      @change="handlePageSizeChange"
    />
  </div>
</template>

<script setup lang="ts">
const props = defineProps({
  total: { default: 0, type: Number },
  pageSizeOptions: { default: [10, 20, 50, 100] },
  currentSize: { default: 0, type: Number },
});
const pageIndex = ref(0);
const pageSize = defineModel({ default: 10 });

const nextAble = computed(() => pageIndex.value != 0);
const preAble = computed(() => props.currentSize == pageSize.value);

const emit = defineEmits<{
  (e: "change", isPrevious: boolean, pageSize: number, refresh: boolean): void;
}>();

const handlePrevPage = () => {
  pageIndex.value--;
  emit("change", true, pageSize.value, false);
};

const handleNextPage = () => {
  pageIndex.value++;
  emit("change", false, pageSize.value, false);
};

const handlePageSizeChange = () => {
  pageIndex.value = 0;
  emit("change", false, pageSize.value, true);
};
</script>
