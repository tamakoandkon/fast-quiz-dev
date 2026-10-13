<template>
  <UInput v-model="input" class="w-full" placeholder="搜索题库名称" aria-label="搜索题库名称" size="xl" @keydown.enter="handleClick">
    <template #trailing>
      <UButton
        variant="link"
        size="sm"
        icon="i-lucide-search"
        @click="handleClick"
      >
        搜索
      </UButton>
    </template>
  </UInput>
</template>

<script setup lang="ts">
const input = defineModel({ default: "" });

const emit = defineEmits<{ (e: "search", payload: string): void }>();

const handleClick = function () {
  emit("search", input.value);
};
</script>
