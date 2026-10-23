<script setup lang="ts">
definePageMeta({ layout: false });

onMounted(() => {
  navigateTo("/tenant/manage", { replace: true });
});
</script>

<template>
  <main class="legacy-route" aria-live="polite">正在前往租户管理…</main>
</template>

<style scoped>
.legacy-route {
  min-height: 100dvh;
  display: grid;
  place-items: center;
  color: #62666d;
}
</style>
