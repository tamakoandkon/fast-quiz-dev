const toEpoch = (iso?: string | null): number | undefined => {
  if (!iso) return undefined;
  const ms = Date.parse(iso);
  return Number.isNaN(ms) ? undefined : ms;
};

const nowEpoch = (): number => Date.now();

/** 试卷是否“限时”（设置了明确的考试时间窗）。 */
export const isTimedQuiz = (template: QuizTemplateDto): boolean => {
  return template.isUnlimitedTime !== true && !!template.endTime;
};

/** 该场（限时）考试是否已经结束；不限时永远视为“已可阅”。 */
export const isExamOver = (template: QuizTemplateDto): boolean => {
  if (!isTimedQuiz(template)) return true;
  const end = toEpoch(template.endTime);
  return end !== undefined && nowEpoch() >= end;
};

/**
 * 一份作答是否进入“待阅卷队列”。
 *
 * 语义（与产品对齐）：
 * - 不限时的试卷：只要 status=Submitted 即需人工阅卷；
 * - 限时的试卷：考试窗口已结束 且 status=Submitted 才需人工阅卷（窗口未结束不开放阅卷）。
 */
export const isAttemptNeedsReview = (
  attempt: QuizAttempDto,
  template: QuizTemplateDto,
): boolean => {
  if (attempt.status !== QuizAttemptStatus.Submitted) return false;
  return isExamOver(template);
};

/** 整场是否允许开放“批改”入口（#2：考试结束之后才可以阅卷）。 */
export const canStartReviewing = (template: QuizTemplateDto): boolean => {
  return isExamOver(template);
};
