<template>
  <UFormField :label="label" class="w-full">
    <UInput
      v-model="value"
      type="number"
      class="w-full"
      :min="min"
      :max="max"
      :step="step"
      :placeholder="placeholder"
      :disabled="disabled"
    />
  </UFormField>
</template>

<script setup lang="ts">
defineProps({
  label: {
    type: String,
    default: "",
  },
  min: {
    type: Number,
    default: undefined,
  },
  max: {
    type: Number,
    default: undefined,
  },
  step: {
    type: Number,
    default: 1,
  },
  placeholder: {
    type: String,
    default: "",
  },
  disabled: {
    type: Boolean,
    default: false,
  },
});

const value = defineModel({ type: Number });
</script>
