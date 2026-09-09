namespace FastQuiz.Question.UseCase.Dto;

public record QuestionGroupDto(string Id, string OwnerId, string Name, string? Remark, int Count);
