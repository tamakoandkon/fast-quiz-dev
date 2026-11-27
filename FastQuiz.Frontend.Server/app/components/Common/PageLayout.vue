<script setup lang="ts">
defineProps<{ title?: string; description?: string }>();
</script>

<template>
  <div class="page-shell">
    <CommonPageHeading :title="title" :description="description">
      <template v-if="$slots.actions" #actions>
        <slot name="actions" />
      </template>
    </CommonPageHeading>
    <div class="page-content">
      <slot v-if="$slots.header" name="header" />
      <slot />
      <slot v-if="$slots.footer" name="footer" />
    </div>
  </div>
</template>
