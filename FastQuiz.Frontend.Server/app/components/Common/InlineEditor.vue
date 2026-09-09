<template>
  <div class="w-full">
    <!-- 展示模式 -->
    <div
      v-if="!isEditing"
      class="cursor-pointer rounded px-1 -mx-1 py-0.5 hover:bg-gray-50 break-words"
      @click="isEditing = true"
    >
      <slot name="display">
        <span :class="{ 'text-gray-400': !modelValue }">
          {{ modelValue || placeholder }}
        </span>
      </slot>
    </div>

    <!-- 编辑模式 -->
    <UInput
      v-else
      v-model="modelValue"
      class="w-full"
      autofocus
      @blur="isEditing = false"
      @keydown.enter="isEditing = false"
    />
  </div>
</template>

<script setup lang="ts">
const modelValue = defineModel({ default: "", required: true });

defineProps<{
  modelValue: string;
  placeholder?: string;
}>();

const isEditing = ref(false);
</script>
