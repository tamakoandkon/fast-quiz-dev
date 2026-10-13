<script setup lang="ts">
const userStore = useUserStore();
const { tenantId, can } = useWorkspaceAccess();
const cards = computed(() => [
  { title: '考试中心', description: '浏览已发布的考试，准备好后开始作答。', icon: 'i-lucide-graduation-cap', path: '/quiz-attempt', action: '查看考试' },
  { title: '我的考试', description: '回顾考试记录与成绩，让每一次练习都有进步。', icon: 'i-lucide-chart-no-axes-column-increasing', path: '/my-quiz-attempt', action: '查看记录' },
  { title: '试卷管理', description: '创建与发布试卷，有序安排每一次考核。', icon: 'i-lucide-file-text', path: '/dash/quiz-template', action: '管理试卷', permission: 'quiz-template' },
  { title: '题库管理', description: '整理题目与题组，建立你的知识资源库。', icon: 'i-lucide-library', path: '/dash/question-group', action: '管理题库', permission: 'question' },
  { title: '用户管理', description: '管理成员信息，协作开展学习与考试。', icon: 'i-lucide-users', path: '/dash/user', action: '管理成员', permission: 'user' },
  { title: '组织管理', description: '按团队与组织归类，让协作更清晰。', icon: 'i-lucide-building-2', path: '/dash/organization', action: '管理组织', permission: 'organization' },
].filter(card => can(card.permission)));
</script>
<template>
  <section>
    <header class="workspace-hero">
      <p class="workspace-eyebrow">FASTQUIZ / OVERVIEW</p>
      <h1 class="workspace-title">你好，{{ userStore.user?.username || '欢迎回来' }}。</h1>
      <p class="workspace-description">学习、考试与管理，在这里井然有序。</p>
    </header>
    <div class="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 gap-5">
      <NuxtLink v-for="card in cards" :key="card.path" :to="'/tenant/' + tenantId + card.path" class="workspace-card">
        <span class="workspace-card-icon"><UIcon :name="card.icon" /></span>
        <h2>{{ card.title }}</h2><p>{{ card.description }}</p>
        <span class="workspace-card-action">{{ card.action }}<UIcon name="i-lucide-arrow-up-right" /></span>
      </NuxtLink>
    </div>
    <div class="workspace-note">
      <UIcon name="i-lucide-sparkles" class="text-xl text-muted shrink-0 mt-1" />
      <p><strong>专注下一步。</strong>从一场考试开始，或回顾已有记录。小小的积累，让进步清晰可见。</p>
    </div>
  </section>
</template>
