<template>
  <CommonSafeMarkup :content="props.content" />
</template>

<script setup lang="ts">
const props = defineProps<{ content: string }>();
</script>

<style scoped></style>
