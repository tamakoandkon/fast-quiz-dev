<template>
  <UFormField :label="label" class="w-full">
    <UInput v-model="value" class="w-full"></UInput>
  </UFormField>
</template>

<script setup lang="ts">
defineProps({
  label: {
    type: String,
    default: "",
  },
});

const value = defineModel({ type: String });
</script>
