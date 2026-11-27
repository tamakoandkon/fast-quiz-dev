<script setup lang="ts">
const props = defineProps<{ title?: string; description?: string }>();
const route = useRoute();
const location = computed(() => workspaceLocation(route.path));
const heading = computed(() => props.title || location.value.title);
</script>

<template>
  <header class="page-heading">
    <div class="page-heading-main">
      <span class="page-heading-icon" aria-hidden="true"><UIcon :name="location.root.icon" /></span>
      <div class="min-w-0">
        <h1 class="workspace-title">{{ heading }}</h1>
        <p v-if="description" class="page-heading-description">{{ description }}</p>
      </div>
    </div>
    <div v-if="$slots.actions" class="page-heading-actions">
      <slot name="actions" />
    </div>
  </header>
</template>
