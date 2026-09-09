<template>
  <UFormField :label="label" class="w-full">
    <div class="flex items-center justify-between w-full">
      <span v-if="description" class="text-sm text-gray-500">{{
        description
      }}</span>
      <USwitch v-model="value" :disabled="disabled" color="primary" size="md" />
    </div>
  </UFormField>
</template>

<script setup lang="ts">
defineProps({
  label: {
    type: String,
    default: "",
  },
  description: {
    type: String,
    default: "",
  },
  disabled: {
    type: Boolean,
    default: false,
  },
  color: {
    type: String,
    default: "primary",
  },
  size: {
    type: String,
    default: "md",
  },
});

const value = defineModel({ type: Boolean, default: false });
</script>
