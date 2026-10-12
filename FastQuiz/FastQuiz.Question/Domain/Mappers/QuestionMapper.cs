using FastQuiz.Question.Domain.Entities;
using FastQuiz.Question.UseCase.Dto;
using Mapster;

namespace FastQuiz.Question.Domain.Mappers;

using Q = FastQuiz.Question.Domain.Entities.Question;

public class QuestionMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config
            .NewConfig<Q, QuestionDto>()
            .Map(d => d.Type, s => s.Type)
            .Map(d => d.Level, s => s.Level)
            .Map(d => d.Remark, s => s.Remark)
            .Map(d => d.GroupId, s => s.GroupId)
            .Map(d => d.MarkupContent, s => s.MarkupContent)
            .Map(d => d.Id, s => s.Id)
            .Map(d => d.Answers, s => s.Answers)
            .Map(d => d.Id, s => s.Id);

        config
            .NewConfig<QuestionAnswer, QuestionAnswerDto>()
            .Map(d => d.Id, s => s.Id)
            .Map(d => d.MarkupContent, s => s.MarkupContent)
            .Map(d => d.IsCorrect, s => s.IsCorrect)
            .Map(d => d.MatchStrategy, s => s.MatchStrategy)
            .Map(d => d.SortOrder, s => s.SortOrder)
            .Map(d => d.QuestionId, s => s.QuestionId);

        config
            .NewConfig<IGrouping<QuestionType, Q>, QuestionTypeCountDto>()
            .Map(d => d.Type, s => s.Key)
            .Map(d => d.Count, s => s.Count());

        config
            .NewConfig<IGrouping<int, Q>, QuestionLevelCountDto>()
            .Map(d => d.Level, s => s.Key)
            .Map(d => d.Count, s => s.Count());
    }
}
