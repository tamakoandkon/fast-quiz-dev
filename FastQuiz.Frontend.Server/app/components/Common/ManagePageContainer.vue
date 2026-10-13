<template>
  <div class="manage-page mx-auto w-full" :class="customClass">
    <CommonPageHeading />
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
