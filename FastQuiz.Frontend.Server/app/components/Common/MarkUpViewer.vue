<template>
  <UEditor v-model="props.content" content-type="markdown" :editable="false" />
</template>

<script setup lang="ts">
const props = defineProps<{ content: string }>();
</script>

<style scoped></style>
