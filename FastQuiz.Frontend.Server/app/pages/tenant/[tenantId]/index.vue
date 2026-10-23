<script setup lang="ts">
const userStore = useUserStore();
const { tenantId, can } = useWorkspaceAccess();
const cards = computed(() => [
  { title: '考试中心', icon: 'i-lucide-graduation-cap', path: '/quiz-attempt' },
  { title: '我的考试', icon: 'i-lucide-chart-no-axes-column-increasing', path: '/my-quiz-attempt' },
  { title: '试卷管理', icon: 'i-lucide-file-text', path: '/dash/quiz-template', permission: 'quiz-template' },
  { title: '题库管理', icon: 'i-lucide-library', path: '/dash/question-group', permission: 'question' },
  { title: '用户管理', icon: 'i-lucide-users', path: '/dash/user', permission: 'user' },
  { title: '组织管理', icon: 'i-lucide-building-2', path: '/dash/organization', permission: 'organization' },
].filter(card => can(card.permission)));
</script>
<template>
  <section>
    <header class="workspace-hero">
      <h1 class="workspace-title">你好，{{ userStore.user?.username || '欢迎回来' }}</h1>
    </header>
    <div class="workspace-grid">
      <NuxtLink v-for="card in cards" :key="card.path" :to="'/tenant/' + tenantId + card.path" class="workspace-card">
        <UIcon :name="card.icon" class="workspace-card-icon" aria-hidden="true" />
        <h2>{{ card.title }}</h2>
        <UIcon name="i-lucide-arrow-up-right" class="workspace-card-arrow" aria-hidden="true" />
      </NuxtLink>
    </div>
  </section>
</template>
