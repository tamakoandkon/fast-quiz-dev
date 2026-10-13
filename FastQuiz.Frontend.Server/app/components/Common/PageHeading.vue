<script setup lang="ts">
const route = useRoute();
const heading = computed(() => {
  const path = route.path;
  const entries = [
    ['/tenant/manage', '租户管理', '管理工作空间，为每个团队提供独立的学习环境。'],
    ['/my-quiz-attempt', '我的考试', '回顾作答记录与成绩，看到每一步进步。'],
    ['/dash/question-group', '题库管理', '整理题目与题组，让知识积累更有条理。'],
    ['/dash/quiz-template', '试卷管理', '从组卷到发布，有序安排每一次考试。'],
    ['/dash/organization', '组织管理', '清晰组织团队成员，让协作更加顺畅。'],
    ['/dash/user-records', '考试成绩', '查看成员的考试记录与学习表现。'],
    ['/dash/user', '用户管理', '管理成员信息，保持团队井然有序。'],
    ['/dash/role', '角色管理', '为成员配置合适的权限，明确团队职责。'],
  ];
  const match = entries.find(([fragment]) => path.includes(fragment!));
  if (match) return { title: match[1], description: match[2] };
  if (/\/tenant\/[^/]+\/quiz-attempt\/?$/.test(path)) {
    return { title: '考试中心', description: '选择一场考试，从容准备，专注作答。' };
  }
  return null;
});
</script>
<template>
  <header v-if="heading" class="page-heading">
    <h1 class="workspace-title">{{ heading.title }}</h1>
    <p class="workspace-description">{{ heading.description }}</p>
  </header>
</template>
