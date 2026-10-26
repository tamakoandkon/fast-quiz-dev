<template>
  <UInput
    v-model="input"
    :size="size"
    class="w-full"
    :placeholder="placeholder"
    :aria-label="placeholder"
    @keydown.enter="handleClick"
  >
    <template #trailing>
      <UButton
        variant="link"
        size="sm"
        icon="i-lucide-search"
        :aria-label="placeholder"
        @click="handleClick"
      />
    </template>
  </UInput>
</template>

<script setup lang="ts">
defineProps({
  placeholder: { type: String, default: "搜索题库名称" },
  // 标题栏里用 md；独立成行时可以用 xl
  size: { type: String, default: "md" },
});

const input = defineModel({ default: "" });

const emit = defineEmits<{ (e: "search", payload: string): void }>();

const handleClick = function () {
  emit("search", input.value);
};
</script>
