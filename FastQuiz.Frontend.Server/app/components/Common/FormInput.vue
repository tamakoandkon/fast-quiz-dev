<template>
  <UFormField
    :label="label"
    :description="fieldDescription"
    :error="error"
    :required="required"
    class="w-full"
  >
    <UInput
      v-model="value"
      class="w-full"
      :type="type"
      :placeholder="placeholder"
      :disabled="disabled"
      :autocomplete="autocomplete"
    />
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
  placeholder: {
    type: String,
    default: "",
  },
  type: {
    type: String,
    default: "text",
  },
  error: {
    type: String,
    default: "",
  },
  autocomplete: {
    type: String,
    default: undefined,
  },
  required: {
    type: Boolean,
    default: false,
  },
  disabled: {
    type: Boolean,
    default: false,
  },
});

const value = defineModel({ type: String });
</script>
