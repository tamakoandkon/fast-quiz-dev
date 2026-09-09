using FastQuiz.Question.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Q = FastQuiz.Question.Domain.Entities.Question;

namespace FastQuiz.Question.Infrastructure.Repositories.PostgreSQL;

public class PostgreSqlQuestionDbContext(
    DbContextOptions<PostgreSqlQuestionDbContext> _options,
    IOptions<QuestionDbContextSettings> _dbSettions
) : DbContext(_options)
{
    private QuestionDbContextSettings Settings => _dbSettions.Value;

    public DbSet<QuestionGroup> QuestionGroups { get; set; }
    public DbSet<Q> Questions { get; set; }
    public DbSet<QuestionAnswer> Answers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        OnQuestionGroupCreating(modelBuilder);
        OnQuestionCreating(modelBuilder);
        OnQuestionAnswerCreating(modelBuilder);
    }

    protected void OnQuestionGroupCreating(ModelBuilder builder)
    {
        builder.Entity<QuestionGroup>(entity =>
        {
            entity.ToTable(Settings.QuestionGroupTableName.ToLowerInvariant());

            entity.HasKey(x => x.Id);

            entity
                .Property(x => x.Id)
                .HasMaxLength(128)
                .IsRequired()
                .HasColumnName(nameof(QuestionGroup.Id).ToLowerInvariant());

            entity
                .Property(x => x.Name)
                .HasMaxLength(128)
                .IsRequired()
                .HasColumnName(nameof(QuestionGroup.Name).ToLowerInvariant());

            entity
                .Property(x => x.Remark)
                .HasMaxLength(512)
                .HasColumnName(nameof(QuestionGroup.Remark).ToLowerInvariant());

            entity
                .Property(x => x.OwnerId)
                .HasMaxLength(128)
                .IsRequired()
                .HasColumnName(nameof(QuestionGroup.OwnerId).ToLowerInvariant());

            entity.HasIndex(x => new { x.OwnerId, x.Name });
        });
    }

    protected void OnQuestionCreating(ModelBuilder builder)
    {
        builder.Entity<Q>(entity =>
        {
            entity.ToTable(Settings.QuestionTableName.ToLowerInvariant());

            entity.HasKey(x => x.Id);

            entity
                .Property(x => x.Id)
                .HasMaxLength(128)
                .IsRequired()
                .HasColumnName(nameof(Q.Id).ToLowerInvariant());

            entity
                .Property(x => x.GroupId)
                .HasMaxLength(128)
                .IsRequired()
                .HasColumnName(nameof(Q.GroupId).ToLowerInvariant());

            entity
                .Property(x => x.MarkupContent)
                .IsRequired()
                .HasColumnName(nameof(Q.MarkupContent).ToLowerInvariant());

            entity
                .Property(x => x.Type)
                .HasConversion<int>()
                .IsRequired()
                .HasColumnName(nameof(Q.Type).ToLowerInvariant());

            entity
                .Property(x => x.Level)
                .HasDefaultValue(0)
                .IsRequired()
                .HasColumnName(nameof(Q.Level).ToLowerInvariant());

            entity
                .Property(x => x.Remark)
                .HasMaxLength(512)
                .IsRequired(false)
                .HasColumnName(nameof(Q.Remark).ToLowerInvariant());

            entity
                .HasIndex(x => new
                {
                    x.GroupId,
                    x.Type,
                    x.Level,
                })
                .HasDatabaseName("IX_Questions_GroupId_Type_Level");

            entity.Ignore(x => x.Answers);

            entity
                .HasOne<QuestionGroup>()
                .WithMany()
                .HasForeignKey(x => x.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    protected void OnQuestionAnswerCreating(ModelBuilder builder)
    {
        builder.Entity<QuestionAnswer>(entity =>
        {
            entity.ToTable(Settings.QuestionAnswerTableName.ToLowerInvariant());

            entity.HasKey(x => x.Id);

            entity
                .Property(x => x.Id)
                .HasMaxLength(128)
                .IsRequired()
                .HasColumnName(nameof(QuestionAnswer.Id).ToLowerInvariant());

            entity
                .Property(x => x.QuestionId)
                .HasMaxLength(128)
                .IsRequired()
                .HasColumnName(nameof(QuestionAnswer.QuestionId).ToLowerInvariant());

            entity
                .Property(x => x.MarkupContent)
                .IsRequired()
                .HasColumnName(nameof(QuestionAnswer.MarkupContent).ToLowerInvariant());

            entity
                .Property(x => x.IsCorrect)
                .HasDefaultValue(false)
                .IsRequired()
                .HasColumnName(nameof(QuestionAnswer.IsCorrect).ToLowerInvariant());

            entity
                .Property(x => x.SortOrder)
                .IsRequired()
                .HasColumnName(nameof(QuestionAnswer.SortOrder).ToLowerInvariant());

            entity
                .Property(x => x.MatchStrategy)
                .HasConversion<int>()
                .HasDefaultValue(MatchStrategy.Exact)
                .IsRequired();

            entity
                .HasIndex(x => new { x.QuestionId, x.SortOrder })
                .HasDatabaseName("IX_QuestionAnswers_QuestionId_SortOrder");

            entity
                .HasOne<Q>()
                .WithMany()
                .HasForeignKey(x => x.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
