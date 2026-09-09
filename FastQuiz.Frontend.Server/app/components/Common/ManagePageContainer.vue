<template>
  <div class="mx-auto w-full" :class="customClass">
    <slot />
  </div>
</template>

<script setup>
defineProps({
  customClass: {
    type: [String, Array, Object],
    default: "",
  },
});
</script>
