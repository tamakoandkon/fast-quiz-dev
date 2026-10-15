<template>
  <div class="safe-markup" v-html="html"></div>
</template>

<script setup lang="ts">
import { safeMarkup } from "~/utils/safeMarkup";
const props = defineProps<{ content: string }>();
const html = computed(() => safeMarkup(props.content));
</script>

<style scoped>
.safe-markup { overflow-wrap: anywhere; }
.safe-markup :deep(img) { max-width: 100%; }
.safe-markup :deep(pre) { overflow-x: auto; white-space: pre; }
.safe-markup :deep(table) { border-collapse: collapse; }
.safe-markup :deep(th), .safe-markup :deep(td) { border: 1px solid #d1d5db; padding: .25rem .5rem; }
</style>
