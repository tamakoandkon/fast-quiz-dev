using System.Globalization;
using FastQuiz.Quiz.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FastQuiz.Quiz.Infrastructure.Repositories.PostgreSQL;

public class PostgreSQLQuizDbContext(
    DbContextOptions<PostgreSQLQuizDbContext> _options,
    IOptions<QuizDbContextSettings> _settings
) : DbContext(_options)
{
    private QuizDbContextSettings Settings => _settings.Value;

    public DbSet<QuizTemplate> QuizTemplates { get; set; }
    public DbSet<QuestionSelectionRule> QuestionSelecionRules { get; set; }
    public DbSet<QuizSnapshot> QuizSnapshots { get; set; }
    public DbSet<QuestionSnapshot> QuestionSnapshots { get; set; }
    public DbSet<QuestionAnswerSnapshot> QuestionAnswerSnapshots { get; set; }

    public DbSet<QuizSnapshotQuestionSnapshotMap> QuizSnapshotQuestionSnapshotMaps { get; set; }

    public DbSet<QuizAttempt> QuizAttempts { get; set; }

    public DbSet<QuestionRecord> QuestionRecords { get; set; }
    public DbSet<AnswerRecord> AnswerRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        OnQuizTemplateCreating(modelBuilder);
        OnQuestionSelectionRuleCreating(modelBuilder);
        OnQuestionSnapshotCreating(modelBuilder);
        OnQuestionAnswerSnapshotCreating(modelBuilder);
        OnQuizSnapshotQuestionSnapshotMapCreating(modelBuilder);
        OnQuizSnapshotCreating(modelBuilder);
        OnQuizAttemptCreating(modelBuilder);
        OnQuestionRecordCreating(modelBuilder);
        OnAnswerRecordCreating(modelBuilder);
    }

    protected void OnQuizTemplateCreating(ModelBuilder builder)
    {
        var format = "yyyy-MM-dd HH:mm:ss.fffffffzzz";

        builder.Entity<QuizTemplate>(entity =>
        {
            entity.ToTable(Settings.QuizTemplateTableName.ToLowerInvariant());

            entity.HasKey(e => e.Id);

            entity
                .Property(e => e.Id)
                .HasMaxLength(128)
                .HasColumnName(nameof(QuizTemplate.Id).ToLowerInvariant());
            entity
                .Property(e => e.Name)
                .HasMaxLength(200)
                .IsRequired()
                .HasColumnName(nameof(QuizTemplate.Name).ToLowerInvariant());
            entity
                .Property(e => e.Description)
                .HasMaxLength(10000)
                .IsRequired()
                .HasColumnName(nameof(QuizTemplate.Description).ToLowerInvariant());
            entity
                .Property(e => e.Remark)
                .HasMaxLength(500)
                .IsRequired()
                .HasColumnName(nameof(QuizTemplate.Remark).ToLowerInvariant());
            entity
                .Property(e => e.OwnerId)
                .HasMaxLength(50)
                .IsRequired()
                .HasColumnName(nameof(QuizTemplate.OwnerId).ToLowerInvariant());
            entity
                .Property(e => e.AttemptLimit)
                .IsRequired()
                .HasColumnName(nameof(QuizTemplate.AttemptLimit).ToLowerInvariant());
            entity
                .Property(e => e.IsUnlimitedTime)
                .IsRequired()
                .HasColumnName(nameof(QuizTemplate.IsUnlimitedTime).ToLowerInvariant());
            entity
                .Property(e => e.BeginTime)
                .HasConversion(
                    v => v.ToString(format, CultureInfo.InvariantCulture),
                    v => DateTimeOffset.Parse(v, CultureInfo.InvariantCulture)
                )
                .HasColumnName(nameof(QuizTemplate.BeginTime).ToLowerInvariant());
            entity
                .Property(e => e.EndTime)
                .HasConversion(
                    v => v.ToString(format, CultureInfo.InvariantCulture),
                    v => DateTimeOffset.Parse(v, CultureInfo.InvariantCulture)
                )
                .HasColumnName(nameof(QuizTemplate.EndTime).ToLowerInvariant())
            ;
            entity
                .Property(e => e.DurationMinutes)
                .IsRequired()
                .HasColumnName(nameof(QuizTemplate.DurationMinutes).ToLowerInvariant());
            entity
                .Property(e => e.IsPublished)
                .IsRequired()
                .HasColumnName(nameof(QuizTemplate.IsPublished).ToLowerInvariant());
            entity
                .Property(e => e.CreatedAt)
                .IsRequired()
                .HasConversion(
                    v => v.ToString(format, CultureInfo.InvariantCulture),
                    v => DateTimeOffset.Parse(v, CultureInfo.InvariantCulture)
                )
                .HasColumnName(nameof(QuizTemplate.CreatedAt).ToLowerInvariant());
            entity
                .Property(e => e.UpdatedAt)
                .IsRequired()
                .HasConversion(
                    v => v.ToString(format, CultureInfo.InvariantCulture),
                    v => DateTimeOffset.Parse(v, CultureInfo.InvariantCulture)
                )
                .HasColumnName(nameof(QuizTemplate.UpdatedAt).ToLowerInvariant());
            entity
                .Property(e => e.UserGroupId)
                .HasMaxLength(128)
                .HasColumnName(nameof(QuizTemplate.UserGroupId).ToLowerInvariant());

            entity.HasIndex(e => new { e.OwnerId, e.IsPublished });
        });
    }

    protected void OnQuestionSelectionRuleCreating(ModelBuilder builder)
    {
        builder.Entity<QuestionSelectionRule>(entity =>
        {
            entity.ToTable(Settings.QuestionSelectionRuleTableName.ToLowerInvariant());

            entity.HasKey(e => e.Id);

            entity
                .Property(e => e.Id)
                .HasMaxLength(128)
                .HasColumnName(nameof(QuestionSelectionRule.Id).ToLowerInvariant());
            entity
                .Property(e => e.QuizeTemplateId)
                .HasMaxLength(128)
                .HasColumnName(nameof(QuestionSelectionRule.QuizeTemplateId).ToLowerInvariant());
            entity
                .Property(e => e.GroupId)
                .HasMaxLength(128)
                .HasColumnName(nameof(QuestionSelectionRule.GroupId).ToLowerInvariant());
            entity
                .Property(e => e.Type)
                .HasColumnName(nameof(QuestionSelectionRule.Type).ToLowerInvariant());
            entity
                .Property(e => e.Level)
                .HasColumnName(nameof(QuestionSelectionRule.Level).ToLowerInvariant());
            entity
                .Property(e => e.Count)
                .HasColumnName(nameof(QuestionSelectionRule.Count).ToLowerInvariant());
            entity
                .Property(e => e.Score)
                .HasColumnName(nameof(QuestionSelectionRule.Score).ToLowerInvariant());

            entity
                .HasOne<QuizTemplate>()
                .WithMany()
                .HasForeignKey(e => e.QuizeTemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.QuizeTemplateId);
        });
    }

    protected void OnQuestionSnapshotCreating(ModelBuilder builder)
    {
        builder.Entity<QuestionSnapshot>(entity =>
        {
            entity.ToTable(Settings.QuestionSnapshotTableName.ToLowerInvariant());

            entity.HasKey(e => e.Id);

            entity
                .Property(e => e.Id)
                .HasMaxLength(128)
                .HasColumnName(nameof(QuestionSnapshot.Id).ToLowerInvariant());
            entity
                .Property(e => e.QuizTemplateId)
                .IsRequired()
                .HasMaxLength(128)
                .HasColumnName(nameof(QuestionSnapshot.QuizTemplateId).ToLowerInvariant());
            entity
                .Property(e => e.QuestionId)
                .IsRequired()
                .HasMaxLength(128)
                .HasColumnName(nameof(QuestionSnapshot.QuestionId).ToLowerInvariant());
            entity
                .Property(e => e.MarkUpContent)
                .IsRequired()
                .HasColumnName(nameof(QuestionSnapshot.MarkUpContent).ToLowerInvariant());
            entity
                .Property(e => e.Type)
                .IsRequired()
                .HasColumnName(nameof(QuestionSnapshot.Type).ToLowerInvariant());
            entity
                .Property(e => e.Level)
                .IsRequired()
                .HasColumnName(nameof(QuestionSnapshot.Level).ToLowerInvariant());
            entity
                .Property(e => e.GroupId)
                .IsRequired()
                .HasMaxLength(128)
                .HasColumnName(nameof(QuestionSnapshot.GroupId).ToLowerInvariant());
            entity
                .Property(e => e.ParentId)
                .HasMaxLength(128)
                .HasColumnName(nameof(QuestionSnapshot.ParentId).ToLowerInvariant());

            entity
                .HasOne<QuizTemplate>()
                .WithMany()
                .HasForeignKey(e => e.QuizTemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Ignore(e => e.AnswerSnapshots);

            entity.HasIndex(e => e.QuizTemplateId);
            entity.HasIndex(e => e.QuestionId);
            entity.HasIndex(e => new
            {
                e.GroupId,
                e.Type,
                e.Level,
            });
        });
    }

    protected void OnQuestionAnswerSnapshotCreating(ModelBuilder builder)
    {
        builder.Entity<QuestionAnswerSnapshot>(entity =>
        {
            entity.ToTable(Settings.QuestionAnswerSnapshotTableName.ToLowerInvariant());

            entity.HasKey(e => e.Id);

            entity
                .Property(e => e.Id)
                .HasMaxLength(128)
                .HasColumnName(nameof(QuestionAnswerSnapshot.Id).ToLowerInvariant());
            entity
                .Property(e => e.MarkUpContent)
                .IsRequired()
                .HasColumnName(nameof(QuestionAnswerSnapshot.MarkUpContent).ToLowerInvariant());
            entity
                .Property(e => e.IsCorrect)
                .IsRequired()
                .HasColumnName(nameof(QuestionAnswerSnapshot.IsCorrect).ToLowerInvariant());
            entity
                .Property(e => e.QuestionSnapshotId)
                .HasMaxLength(128)
                .IsRequired()
                .HasColumnName(nameof(QuestionAnswerSnapshot.QuestionSnapshotId).ToLowerInvariant());
            entity
                .Property(e => e.SortOrder)
                .IsRequired()
                .HasColumnName(nameof(QuestionAnswerSnapshot.SortOrder).ToLowerInvariant());
            entity
                .Property(e => e.Strategy)
                .IsRequired()
                .HasColumnName(nameof(QuestionAnswerSnapshot.Strategy).ToLowerInvariant());

            entity
                .HasOne<QuestionSnapshot>()
                .WithMany()
                .HasForeignKey(e => e.QuestionSnapshotId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.QuestionSnapshotId, e.SortOrder });
        });
    }

    protected void OnQuizSnapshotCreating(ModelBuilder builder)
    {
        builder.Entity<QuizSnapshot>(entity =>
        {
            entity.ToTable(Settings.QuizSnapshotTableName.ToLowerInvariant());

            entity.HasKey(e => e.Id);

            entity
                .Property(e => e.Id)
                .HasMaxLength(128)
                .HasColumnName(nameof(QuizSnapshot.Id).ToLowerInvariant());
            entity
                .Property(e => e.QuizTemplateId)
                .HasMaxLength(128)
                .IsRequired()
                .HasColumnName(nameof(QuizSnapshot.QuizTemplateId).ToLowerInvariant());
            entity
                .Property(e => e.TotalScore)
                .IsRequired()
                .HasColumnName(nameof(QuizSnapshot.TotalScore).ToLowerInvariant());

            entity.Ignore(e => e.Questions);

            entity
                .HasOne<QuizTemplate>()
                .WithMany()
                .HasForeignKey(e => e.QuizTemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.QuizTemplateId);
        });
    }

    protected void OnQuizSnapshotQuestionSnapshotMapCreating(ModelBuilder builder)
    {
        builder.Entity<QuizSnapshotQuestionSnapshotMap>(entity =>
        {
            entity.ToTable(Settings.QuizSnapshotQuestionSnapshotMapTableName.ToLowerInvariant());

            entity.HasKey(e => new { e.QuizSnapshotId, e.QuestionSnapshotId });

            entity
                .Property(e => e.QuizSnapshotId)
                .HasMaxLength(128)
                .IsRequired()
                .HasColumnName(nameof(QuizSnapshotQuestionSnapshotMap.QuizSnapshotId).ToLowerInvariant());
            entity
                .Property(e => e.QuestionSnapshotId)
                .HasMaxLength(128)
                .IsRequired()
                .HasColumnName(
                    nameof(QuizSnapshotQuestionSnapshotMap.QuestionSnapshotId).ToLowerInvariant()
                );

            entity
                .HasOne<QuizSnapshot>()
                .WithMany()
                .HasForeignKey(e => e.QuizSnapshotId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne<QuestionSnapshot>()
                .WithMany()
                .HasForeignKey(e => e.QuestionSnapshotId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    protected void OnQuizAttemptCreating(ModelBuilder builder)
    {
        builder.Entity<QuizAttempt>(entity =>
        {
            entity.ToTable(Settings.QuizAttemptTableName.ToLowerInvariant());

            entity.HasKey(e => e.Id);

            entity
                .Property(e => e.Id)
                .HasMaxLength(128)
                .IsRequired()
                .HasColumnName(nameof(QuizAttempt.Id).ToLowerInvariant());
            entity
                .Property(e => e.AssignId)
                .HasMaxLength(128)
                .HasColumnName(nameof(QuizAttempt.AssignId).ToLowerInvariant());
            entity
                .Property(e => e.QuizTemplateId)
                .HasMaxLength(128)
                .IsRequired()
                .HasColumnName(nameof(QuizAttempt.QuizTemplateId).ToLowerInvariant());
            entity
                .Property(e => e.QuizSnapshotId)
                .HasMaxLength(128)
                .IsRequired()
                .HasColumnName(nameof(QuizAttempt.QuizSnapshotId).ToLowerInvariant());
            entity
                .Property(e => e.StartTime)
                .HasColumnName(nameof(QuizAttempt.StartTime).ToLowerInvariant());
            entity
                .Property(e => e.SubmitTime)
                .HasColumnName(nameof(QuizAttempt.SubmitTime).ToLowerInvariant());
            entity
                .Property(e => e.Score)
                .IsRequired()
                .HasColumnName(nameof(QuizAttempt.Score).ToLowerInvariant());
            entity
                .Property(e => e.Status)
                .IsRequired()
                .HasColumnName(nameof(QuizAttempt.Status).ToLowerInvariant());

            entity.Ignore(e => e.QuestionRecords);

            entity
                .HasOne<QuizTemplate>()
                .WithMany()
                .HasForeignKey(e => e.QuizTemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne<QuizSnapshot>()
                .WithMany()
                .HasForeignKey(e => e.QuizSnapshotId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.QuizSnapshotId);
            entity
                .HasIndex(e => e.AssignId)
                .HasFilter($"{nameof(QuizAttempt.AssignId)} IS NOT NULL")
            ;
            entity.HasIndex(e => new { e.QuizTemplateId, e.Status });
        });
    }

    protected void OnQuestionRecordCreating(ModelBuilder builder)
    {
        builder.Entity<QuestionRecord>(entity =>
        {
            entity.ToTable(Settings.QuestionRecordTableName.ToLowerInvariant());

            entity.HasKey(e => e.Id);

            entity
                .Property(e => e.Id)
                .HasMaxLength(128)
                .IsRequired()
                .HasColumnName(nameof(QuestionRecord.Id).ToLowerInvariant());
            entity
                .Property(e => e.MarkUpContent)
                .IsRequired()
                .HasColumnName(nameof(QuestionRecord.MarkUpContent).ToLowerInvariant());
            entity
                .Property(e => e.IsCorrect)
                .HasColumnName(nameof(QuestionRecord.IsCorrect).ToLowerInvariant());
            entity
                .Property(e => e.QuizAttemptId)
                .HasMaxLength(128)
                .IsRequired()
                .HasColumnName(nameof(QuestionRecord.QuizAttemptId).ToLowerInvariant());
            entity
                .Property(e => e.SortOrder)
                .IsRequired()
                .HasColumnName(nameof(QuestionRecord.SortOrder).ToLowerInvariant());
            entity
                .Property(e => e.QuestionSnapshotId)
                .HasMaxLength(128)
                .IsRequired()
                .HasColumnName(nameof(QuestionRecord.QuestionSnapshotId).ToLowerInvariant());

            entity.Ignore(e => e.AnswerRecords);

            entity
                .HasOne<QuizAttempt>()
                .WithMany()
                .HasForeignKey(e => e.QuizAttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne<QuestionSnapshot>()
                .WithMany()
                .HasForeignKey(e => e.QuestionSnapshotId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.QuizAttemptId, e.SortOrder });
        });
    }

    protected void OnAnswerRecordCreating(ModelBuilder builder)
    {
        builder.Entity<AnswerRecord>(entity =>
        {
            entity.ToTable(Settings.AnswerRecordTableName.ToLowerInvariant());

            entity.HasKey(e => e.Id);

            entity
                .Property(e => e.Id)
                .HasMaxLength(128)
                .IsRequired()
                .HasColumnName(nameof(AnswerRecord.Id).ToLowerInvariant());
            entity
                .Property(e => e.QuizAttemptId)
                .HasMaxLength(128)
                .IsRequired()
                .HasColumnName(nameof(AnswerRecord.QuizAttemptId).ToLowerInvariant());
            entity
                .Property(e => e.QuestionRecordId)
                .HasMaxLength(128)
                .IsRequired()
                .HasColumnName(nameof(AnswerRecord.QuestionRecordId).ToLowerInvariant());
            entity
                .Property(e => e.Value)
                .IsRequired()
                .HasColumnName(nameof(AnswerRecord.Value).ToLowerInvariant());

            entity
                .HasOne<QuizAttempt>()
                .WithMany()
                .HasForeignKey(e => e.QuizAttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne<QuestionRecord>()
                .WithMany()
                .HasForeignKey(e => e.QuestionRecordId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.QuestionRecordId);
        });
    }
}
