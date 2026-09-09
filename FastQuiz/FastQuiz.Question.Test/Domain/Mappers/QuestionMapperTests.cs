using FastQuiz.Question.Domain.Entities;
using FastQuiz.Question.Domain.Mappers;
using FastQuiz.Question.UseCase.Dto;
using Mapster;
using Shouldly;
using Q = FastQuiz.Question.Domain.Entities.Question;

namespace FastQuiz.Question.Test.Domain.Mappers;

[TestFixture]
internal class QuestionMapperTests
{
    [SetUp]
    public void Setup()
    {
        // 注册所有 Mapper 配置
        TypeAdapterConfig.GlobalSettings.Scan(typeof(QuestionMapper).Assembly);
    }

    [Test]
    public void Question_To_QuestionDto_AllProperties_ShouldMapCorrectly()
    {
        // Arrange
        var answers = new List<QuestionAnswer>
        {
            new("a1", "q1", "A. 北京", true, 1, MatchStrategy.Exact),
            new("a2", "q1", "B. 上海", false, 2, MatchStrategy.Exact),
            new("a3", "q1", "C. 广州", false, 3, MatchStrategy.Exact),
        };

        var question = new Q(
            id: "q1",
            type: QuestionType.SingleChoice,
            level: 3,
            markupContent: "中国的首都是哪里？",
            remark: "这是一道常识题",
            groupId: "g1",
            answers: answers
        );

        // Act
        var dto = question.Adapt<QuestionDto>();

        // Assert
        dto.Id.ShouldBe(question.Id);
        dto.Type.ShouldBe(question.Type);
        dto.Level.ShouldBe(question.Level);
        dto.MarkupContent.ShouldBe(question.MarkupContent);
        dto.Remark.ShouldBe(question.Remark);
        dto.GroupId.ShouldBe(question.GroupId);
        dto.Answers.ShouldNotBeNull();
        dto.Answers.Count.ShouldBe(3);
    }

    [Test]
    public void Question_To_QuestionDto_Answers_ShouldMapCorrectly()
    {
        // Arrange
        var answers = new List<QuestionAnswer>
        {
            new("a1", "q1", "A. 北京", true, 1, MatchStrategy.Exact),
            new("a2", "q1", "B. 上海", false, 2, MatchStrategy.Manual),
        };

        var question = new Q(
            id: "q1",
            type: QuestionType.SingleChoice,
            level: 1,
            markupContent: "测试题目",
            remark: null,
            groupId: "g1",
            answers: answers
        );

        // Act
        var dto = question.Adapt<QuestionDto>();

        // Assert
        dto.Answers[0].Id.ShouldBe("a1");
        dto.Answers[0].QuestionId.ShouldBe("q1");
        dto.Answers[0].MarkupContent.ShouldBe("A. 北京");
        dto.Answers[0].IsCorrect.ShouldBeTrue();
        dto.Answers[0].SortOrder.ShouldBe(1);
        dto.Answers[0].MatchStrategy.ShouldBe(MatchStrategy.Exact);

        dto.Answers[1].Id.ShouldBe("a2");
        dto.Answers[1].MatchStrategy.ShouldBe(MatchStrategy.Manual);
    }

    [Test]
    public void Question_To_QuestionDto_WithEmptyAnswers_ShouldMapCorrectly()
    {
        // Arrange
        var question = new Q(
            id: "q2",
            type: QuestionType.FillIn,
            level: 5,
            markupContent: "请简述面向对象的三大特性",
            remark: "开放题",
            groupId: "g2",
            answers: []
        );

        // Act
        var dto = question.Adapt<QuestionDto>();

        // Assert
        dto.Id.ShouldBe("q2");
        dto.Type.ShouldBe(QuestionType.FillIn);
        dto.Level.ShouldBe(5);
        dto.Answers.ShouldNotBeNull();
        dto.Answers.ShouldBeEmpty();
    }

    [Test]
    public void QuestionAnswer_To_QuestionAnswerDto_AllProperties_ShouldMapCorrectly()
    {
        // Arrange
        var answer = new QuestionAnswer(
            id: "a99",
            questionId: "q99",
            markupContent: "D. 重庆",
            isCorrect: true,
            sortOrder: 4,
            strategy: MatchStrategy.Exact
        );

        // Act
        var dto = answer.Adapt<QuestionAnswerDto>();

        // Assert
        dto.Id.ShouldBe(answer.Id);
        dto.QuestionId.ShouldBe(answer.QuestionId);
        dto.MarkupContent.ShouldBe(answer.MarkupContent);
        dto.IsCorrect.ShouldBe(answer.IsCorrect);
        dto.SortOrder.ShouldBe(answer.SortOrder);
        dto.MatchStrategy.ShouldBe(answer.MatchStrategy);
    }

    [Test]
    public void Question_To_QuestionDto_WithNullRemark_ShouldMapToNull()
    {
        // Arrange
        var question = new Q(
            id: "q3",
            type: QuestionType.FillIn,
            level: 2,
            markupContent: "地球是圆的",
            remark: null,
            groupId: "g3",
            answers: []
        );

        // Act
        var dto = question.Adapt<QuestionDto>();

        // Assert
        dto.Remark.ShouldBeNull();
    }
}
