export const getQuestionTypeName = (type: QuestionType): string => {
  const typeMap: Record<QuestionType, string> = {
    [QuestionType.SingleChoice]: "单选",
    [QuestionType.MultipleChoice]: "多选",
    [QuestionType.FillIn]: "填空",
  };
  return typeMap[type] || "未知";
};

export const getMatchStrategy = (type: MatchStrategy): string => {
  const typeMap: Record<MatchStrategy, string> = {
    [MatchStrategy.Contains]: "包含匹配",
    [MatchStrategy.Exact]: "精确匹配",
    [MatchStrategy.Regex]: "正则匹配",
    [MatchStrategy.Manual]: "手动匹配",
  };
  return typeMap[type] || "未知";
};

/**
 * 一道题是否需要人工阅卷。
 *
 * 现状约定（无主观题题型）：仅「填空」题且其任一答案采用 Manual 匹配策略时，
 * 需要老师人工判断对错；其余（单选/多选、以及填空的 Exact/Contains/Regex）
 * 属于客观题，交卷时由判定逻辑自动维护 isCorrect，不进入人工阅卷队列。
 *
 * @param question 带答案的题目记录（question-snapshot 的 type 与 answers 均在）。
 */
export const needsManualMarking = (
  question: QuestionRecordWithAnswerDto | QuestionRecordWithoutAnswerDto,
): boolean => {
  if (question.type !== QuestionType.FillIn) return false;

  if (!question.answers || question.answers.length === 0) return false;

  return question.answers.some((a) => a.matchStrategy === MatchStrategy.Manual);
};
