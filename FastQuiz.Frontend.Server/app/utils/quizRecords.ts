import { QuizAttemptStatus, type QuizAttempDto } from './backEnd';

export function quizRecordLink(tenantId: string, row: QuizAttempDto, canReview: boolean) {
  if (!tenantId || !row.id) return undefined;
  if (row.status === QuizAttemptStatus.Graded)
    return `/tenant/${tenantId}/quiz-attempt/${row.id}/result`;
  if (canReview && row.status === QuizAttemptStatus.Submitted && row.quizTemplateId)
    return `/tenant/${tenantId}/dash/quiz-template/${row.quizTemplateId}/review/${row.id}`;
  return undefined;
}

export function quizRecordName(row: QuizAttempDto) {
  return row.assignUsername || row.assignId || '未知考生';
}
