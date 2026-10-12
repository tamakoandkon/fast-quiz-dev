using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Question.Domain.Entities;

namespace FastQuiz.API.Test.Utils;

internal static class UrlPackage
{
    #region Auth
    public static string Login() => "/auth/login";

    public static string Register() => "/auth/register";

    public static string MyProfile() => "/auth/profile";

    public static string MyAccount() => "/auth/me";
    #endregion

    #region Tenant
    public static string TenantList() => "/tenant";

    public static string TenantUserMap(string tenantId, string userId) =>
        $"/tenant/{tenantId}/user-map/{userId}";

    public static string TenantUser(string tenantId) => $"/tenant/{tenantId}/user";
    #endregion

    #region Question
    public static string QuestionGroup() => $"/tenant/{Tenant.DefaultTenantId}/question-group";

    public static string QuestionGroup(string groupId) =>
        $"/tenant/{Tenant.DefaultTenantId}/question-group/{groupId}";

    public static string QuestionGroup(
        string? cursor = null,
        int count = 10,
        bool isPrevious = false,
        string? namePrefix = null
    ) =>
        $"/tenant/{Tenant.DefaultTenantId}/question-group?cursor={cursor}&count={count}&isPrevious={isPrevious}&namePrefix={namePrefix}";

    public static string QuiestionGroupBatchDelete() =>
        $"/tenant/{Tenant.DefaultTenantId}/question-group/batch-delete";

    public static string QuestionWithGroupId(string groupId) =>
        $"/tenant/{Tenant.DefaultTenantId}/question-group/{groupId}/question";

    public static string Question(string questionId) =>
        $"/tenant/{Tenant.DefaultTenantId}/question-group/question/{questionId}";

    public static string Question(
        string groupId,
        string? cursor = null,
        int count = 10,
        QuestionType? type = null,
        int? level = null,
        bool isPrevious = false
    )
    {
        var parameters = new List<string>();

        if (cursor != null)
            parameters.Add($"cursor={Uri.EscapeDataString(cursor)}");

        parameters.Add($"count={count}");

        if (type.HasValue)
            parameters.Add($"type={type.Value}");

        if (level.HasValue)
            parameters.Add($"level={level.Value}");

        if (isPrevious)
            parameters.Add($"isPrevious=true");

        var queryString = parameters.Any() ? $"?{string.Join("&", parameters)}" : "";

        return $"/tenant/{Tenant.DefaultTenantId}/question-group/{groupId}/question{queryString}";
    }

    public static string QuestionCount(string groupId, QuestionType type, int level) =>
        $"/tenant/{Tenant.DefaultTenantId}/question-group/{groupId}/count?type={type}&level={level}";

    public static string QuestionGroupStatistics(string groupId) =>
        $"/tenant/{Tenant.DefaultTenantId}/question-group/{groupId}/statistics";
    #endregion

    #region QuizTemplate

    public static string QuizTemplate() => $"/tenant/{Tenant.DefaultTenantId}/quiz-template";

    public static string QuizTemplate(string id) =>
        $"/tenant/{Tenant.DefaultTenantId}/quiz-template/{id}";

    public static string QuizTemplate(
        string? cursor = null,
        int count = 10,
        bool isPrevious = false
    ) =>
        $"/tenant/{Tenant.DefaultTenantId}/quiz-template?cursor={cursor}&count={count}&isPrevious={isPrevious}";

    public static string QuestionSelectionRuleWithTemplateId(string templateId) =>
        $"/tenant/{Tenant.DefaultTenantId}/quiz-template/{templateId}/rule";

    public static string QuestionSelectionRuleWithTemplateId(
        string templateId,
        string? cursor = null,
        int count = 10
    ) =>
        $"/tenant/{Tenant.DefaultTenantId}/quiz-template/{templateId}/rule?cursor={cursor}&count={count}";

    public static string QuestionSelectionRule(string id) =>
        $"/tenant/{Tenant.DefaultTenantId}/quiz-template/rule/{id}";
    #endregion

    #region QuizSnapshot
    public static string QuizSnapshotWithTemplate(string templateId) =>
        $"/tenant/{Tenant.DefaultTenantId}/quiz-template/{templateId}/quiz-snapshot";

    public static string QuizSnapshotWithTemplate(
        string templateId,
        string? cursor,
        int count = 10
    ) =>
        $"/tenant/{Tenant.DefaultTenantId}/quiz-template/{templateId}/quiz-snapshot?cursor={cursor}&count={count}";

    public static string QuizSnapshotWithId(string id) =>
        $"/tenant/{Tenant.DefaultTenantId}/quiz-template/quiz-snapshot/{id}";

    public static string QuestionRecordWithQuizSnapshotId(
        string quizSnapshotId,
        string? cursor,
        int count = 10
    ) =>
        $"/tenant/{Tenant.DefaultTenantId}/quiz-template/quiz-snapshot/{quizSnapshotId}/question-snapshot?cursor={cursor}&count={count}";
    #endregion

    #region QuizAttempt
    public static string QuizAttemptWithTemplateId(string templateId) =>
        $"/tenant/{Tenant.DefaultTenantId}/quiz-template/{templateId}/quiz-attempt";

    public static string MyQuizAttemptWithTemplateId() =>
        $"/tenant/{Tenant.DefaultTenantId}/quiz-template/quiz-attempt/me";

    public static string QuizAttemptBatchWithTemplateId(string templateId) =>
        $"/tenant/{Tenant.DefaultTenantId}/quiz-template/{templateId}/quiz-attempt/batch";

    public static string QuizAttemptWithTemplateId(
        string templateId,
        string? cursor,
        int count = 10,
        bool isPrevious = false
    ) =>
        $"/tenant/{Tenant.DefaultTenantId}/quiz-template/{templateId}/quiz-attempt?cursor={cursor}&count={count}&isPrevious={isPrevious}";

    public static string QuizAttemptWithId(string id) =>
        $"/tenant/{Tenant.DefaultTenantId}/quiz-template/quiz-attempt/{id}";

    public static string QuizAttemptQuestionWithOutAnswerWithId(string id) =>
        $"/tenant/{Tenant.DefaultTenantId}/quiz-template/quiz-attempt/{id}/question";

    public static string QuizAttemptQuestionWithAnswerWithId(string id) =>
        $"/tenant/{Tenant.DefaultTenantId}/quiz-template/quiz-attempt/{id}/question/with-answer";

    public static string QuizAttemptQuestionWithQuestionId(string id) =>
        $"/tenant/{Tenant.DefaultTenantId}/quiz-template/quiz-attempt/question/{id}";

    public static string QuizAttemptQuestionStatusWithQuestionId(string id, bool isCorrect) =>
        $"/tenant/{Tenant.DefaultTenantId}/quiz-template/quiz-attempt/question/{id}/status/{isCorrect}";
    #endregion
}
