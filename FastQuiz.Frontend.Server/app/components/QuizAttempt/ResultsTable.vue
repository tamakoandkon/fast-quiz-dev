<script setup lang="ts">
import { quizRecordLink, quizRecordName } from '~/utils/quizRecords';

const props = defineProps<{
  records: QuizAttempDto[];
  tenantId: string;
  canReview?: boolean;
  showAssignee?: boolean;
  showExam?: boolean;
}>();
const columns = computed(() => [
  ...(props.showAssignee ? [{ key: 'assignee', label: '考生' }] : []),
  ...(props.showExam ? [{ key: 'exam', label: '考试' }] : []),
  { key: 'status', label: '状态', width: '82px' },
  { key: 'startTime', label: '开始时间', width: '112px' },
  { key: 'submitTime', label: '交卷时间', width: '112px' },
  { key: 'score', label: '得分', width: '64px' },
  { key: 'actions', label: '操作', width: '92px' },
]);
const statuses = {
  [QuizAttemptStatus.Pending]: '未开始',
  [QuizAttemptStatus.InProgress]: '进行中',
  [QuizAttemptStatus.Submitted]: '待批阅',
  [QuizAttemptStatus.Graded]: '已批阅',
};
const datePart = (value: string | null | undefined, time = false) => {
  if (!value) return '--';
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return '--';
  return time ? date.toLocaleTimeString('zh-CN', { hour12: false }) : date.toLocaleDateString('zh-CN');
};
</script>

<template>
  <CommonDataTable :columns="columns" :data="records" :selectable="false" compact>
    <template #assignee="{ row }">
      <span class="record-name" :title="row.assignId ?? undefined">{{ quizRecordName(row) }}</span>
    </template>
    <template #exam="{ row }">
      <span class="record-name">{{ row.quizTemplateNema || '--' }}</span>
    </template>
    <template #status="{ row }">
      <UBadge :color="row.status === QuizAttemptStatus.Graded ? 'success' : row.status === QuizAttemptStatus.Submitted ? 'warning' : 'neutral'" size="sm" variant="subtle">
        {{ row.status ? statuses[row.status] : '未知' }}
      </UBadge>
    </template>
    <template #startTime="{ row }">
      <time :datetime="row.startTime ?? undefined" class="record-date">
        <span>{{ datePart(row.startTime) }}</span><span>{{ row.startTime ? datePart(row.startTime, true) : '' }}</span>
      </time>
    </template>
    <template #submitTime="{ row }">
      <time :datetime="row.submitTime ?? undefined" class="record-date">
        <span>{{ datePart(row.submitTime) }}</span><span>{{ row.submitTime ? datePart(row.submitTime, true) : '' }}</span>
      </time>
    </template>
    <template #score="{ row }">
      <span :class="row.status === QuizAttemptStatus.Graded ? 'text-green-600 font-semibold tabular-nums' : ''">
        {{ row.status === QuizAttemptStatus.Graded ? row.score ?? '--' : '--' }}
      </span>
    </template>
    <template #actions="{ row }">
      <UButton v-if="quizRecordLink(tenantId, row, !!canReview)" :to="quizRecordLink(tenantId, row, !!canReview)" color="neutral" variant="outline" size="xs" icon="i-lucide-eye">
        {{ row.status === QuizAttemptStatus.Graded ? '查看答案' : '阅卷' }}
      </UButton>
      <span v-else class="text-xs text-muted">{{ row.status === QuizAttemptStatus.Submitted ? '等待阅卷' : '尚未交卷' }}</span>
    </template>
  </CommonDataTable>
</template>

<style scoped>
.record-name { display: block; width: 100%; min-width: 0; overflow-wrap: anywhere; text-align: left; }
.record-date { display: flex; flex-direction: column; font-size: 12px; font-variant-numeric: tabular-nums; }
.record-date > :last-child { color: #86868b; }
</style>
