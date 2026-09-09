<template>
  <UButton
    :disabled="disabled"
    :loading="isLoading"
    class="px-6 py-2 font-medium transition-all duration-200 w-full justify-center"
    :class="{
      'bg-primary-500 hover:bg-primary-600 text-white shadow-sm':
        !disabled && !isLoading,
      'bg-gray-100 text-gray-400 cursor-not-allowed': disabled && !isLoading,
      'bg-primary-400 text-white cursor-wait': isLoading,
    }"
    @click="handleClick"
  >
    <template #leading v-if="!isLoading && !disabled">
      <UIcon name="i-heroicons-arrow-down-circle" class="w-5 h-5" />
    </template>

    {{ buttonText }}

    <template #trailing v-if="isLoading">
      <UIcon name="i-heroicons-arrow-path" class="w-5 h-5 animate-spin" />
    </template>
  </UButton>
</template>

<script setup lang="ts">
import { ref, computed } from "vue";

const props = defineProps<{
  disabled?: boolean;
  loadingText?: string;
  defaultText?: string;
}>();

const emit = defineEmits<{
  (e: "click"): void;
}>();

// 内部加载状态
const isLoading = ref(false);

// 按钮文本计算属性
const buttonText = computed(() => {
  if (isLoading.value) {
    return props.loadingText || "加载中...";
  }
  return props.defaultText || "加载更多";
});

// 处理点击事件
const handleClick = async () => {
  if (props.disabled || isLoading.value) {
    return;
  }

  try {
    isLoading.value = true;
    emit("click");
  } catch (error) {
    console.error("加载更多数据失败:", error);
  } finally {
    isLoading.value = false;
  }
};
</script>
