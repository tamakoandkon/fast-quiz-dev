import { isAxiosError } from "axios";

export const examUnavailableReason = (template: QuizTemplateDto) => {
  if (template.isUnlimitedTime) return null;

  const now = Date.now();
  if (template.beginTime && now < Date.parse(template.beginTime)) return "考试未开始";
  if (template.endTime && now > Date.parse(template.endTime)) return "考试已结束";
  return null;
};

export const useExamEntry = (tenantId: Ref<string>) => {
  const { $api } = useNuxtApp();
  const router = useRouter();
  const toast = useToast();
  const enteringTemplateId = ref<string | null>(null);

  const enterExam = async (templateId?: string | null, existingAttempt?: QuizAttempDto) => {
    if (!templateId || enteringTemplateId.value) return;
    enteringTemplateId.value = templateId;
    const tenant = tenantId.value;

    try {
      const attempt = existingAttempt
        ?? (await $api.tenant.quizTemplateQuizAttemptJoinCreate(tenant, templateId)).data;
      if (!attempt.id) throw new Error("缺少考试记录");

      if (attempt.status === QuizAttemptStatus.Pending) {
        await $api.tenant.quizTemplateQuizAttemptUpdate(tenant, attempt.id, {
          status: QuizAttemptStatus.InProgress,
        });
      }

      await router.push(`/tenant/${tenant}/quiz-attempt/${attempt.id}/answer`);
    } catch (error) {
      const detail = isAxiosError<ProblemDetails>(error) ? error.response?.data?.title : null;
      toast.add({ title: detail || "暂时无法进入考试，请重试", color: "error" });
    } finally {
      enteringTemplateId.value = null;
    }
  };

  return { enterExam, enteringTemplateId };
};
