<script setup lang="ts">
const props = defineProps({
  // 显式标题优先，未传时按路由推导。
  title: { type: String, default: undefined },
});
const route = useRoute();
const routeHeading = computed(() => {
  const path = route.path;
  const entries = [
    ['/tenant/manage', '租户管理'],
    ['/my-quiz-attempt', '我的考试'],
    ['/dash/question-group', '题库管理'],
    ['/dash/quiz-template', '试卷管理'],
    ['/dash/organization', '组织管理'],
    ['/dash/user-records', '考试成绩'],
    ['/dash/user', '用户管理'],
    ['/dash/role', '角色管理'],
  ];
  const match = entries.find(([fragment]) => path.includes(fragment!));
  if (match) return match[1];
  if (/\/tenant\/[^/]+\/quiz-attempt\/?$/.test(path)) {
    return '考试中心';
  }
  return null;
});
const heading = computed(() => props.title ?? routeHeading.value);
</script>
<template>
  <header v-if="heading || $slots.actions" class="page-heading">
    <h1 v-if="heading" class="workspace-title">{{ heading }}</h1>
    <div v-if="$slots.actions" class="page-heading-actions">
      <slot name="actions"></slot>
    </div>
  </header>
</template>
