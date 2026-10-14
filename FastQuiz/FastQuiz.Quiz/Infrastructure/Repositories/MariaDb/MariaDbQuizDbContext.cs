using System.Globalization;
using FastQuiz.Quiz.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FastQuiz.Quiz.Infrastructure.Repositories.MariaDb;

public class MariaDbQuizDbContext(
    DbContextOptions<MariaDbQuizDbContext> _options,
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

            entity.Property(e => e.Id).HasMaxLength(128);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(10000).IsRequired();
            entity.Property(e => e.Remark).HasMaxLength(500).IsRequired();
            entity.Property(e => e.OwnerId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.AttemptLimit).IsRequired();
            entity.Property(e => e.IsUnlimitedTime).IsRequired();
            entity
                .Property(e => e.BeginTime)
                .HasConversion(
                    v => v.ToString(format, CultureInfo.InvariantCulture),
                    v => DateTimeOffset.Parse(v, CultureInfo.InvariantCulture)
                );
            ;
            entity
                .Property(e => e.EndTime)
                .HasConversion(
                    v => v.ToString(format, CultureInfo.InvariantCulture),
                    v => DateTimeOffset.Parse(v, CultureInfo.InvariantCulture)
                );
            ;
            entity.Property(e => e.DurationMinutes).IsRequired();
            entity.Property(e => e.IsPublished).IsRequired();
            entity
                .Property(e => e.CreatedAt)
                .IsRequired()
                .HasConversion(
                    v => v.ToString(format, CultureInfo.InvariantCulture),
                    v => DateTimeOffset.Parse(v, CultureInfo.InvariantCulture)
                );
            ;
            entity
                .Property(e => e.UpdatedAt)
                .IsRequired()
                .HasConversion(
                    v => v.ToString(format, CultureInfo.InvariantCulture),
                    v => DateTimeOffset.Parse(v, CultureInfo.InvariantCulture)
                );
            ;
            entity.Property(e => e.UserGroupId).HasMaxLength(128);

            entity.HasIndex(e => new { e.OwnerId, e.IsPublished });
        });
    }

    protected void OnQuestionSelectionRuleCreating(ModelBuilder builder)
    {
        builder.Entity<QuestionSelectionRule>(entity =>
        {
            entity.ToTable(Settings.QuestionSelectionRuleTableName.ToLowerInvariant());

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasMaxLength(128);
            entity.Property(e => e.QuizeTemplateId).HasMaxLength(128);
            entity.Property(e => e.GroupId).HasMaxLength(128);
            entity.Property(e => e.Type);
            entity.Property(e => e.Level);
            entity.Property(e => e.Count);
            entity.Property(e => e.Score);

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

            entity.Property(e => e.Id).HasMaxLength(128);
            entity.Property(e => e.QuizTemplateId).IsRequired().HasMaxLength(128);
            entity.Property(e => e.QuestionId).IsRequired().HasMaxLength(128);
            entity.Property(e => e.MarkUpContent).IsRequired();
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.Level).IsRequired();
            entity.Property(e => e.GroupId).IsRequired().HasMaxLength(128);
            entity.Property(e => e.ParentId).HasMaxLength(128);

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

            entity.Property(e => e.Id).HasMaxLength(128);
            entity.Property(e => e.MarkUpContent).IsRequired();
            entity.Property(e => e.IsCorrect).IsRequired();
            entity.Property(e => e.QuestionSnapshotId).HasMaxLength(128).IsRequired();
            entity.Property(e => e.SortOrder).IsRequired();
            entity.Property(e => e.Strategy).IsRequired();

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

            entity.Property(e => e.Id).HasMaxLength(128);
            entity.Property(e => e.QuizTemplateId).HasMaxLength(128).IsRequired();
            entity.Property(e => e.TotalScore).IsRequired();

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

            entity.Property(e => e.QuizSnapshotId).HasMaxLength(128).IsRequired();
            entity.Property(e => e.QuestionSnapshotId).HasMaxLength(128).IsRequired();

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

            entity.Property(e => e.Id).HasMaxLength(128).IsRequired();
            entity.Property(e => e.AssignId).HasMaxLength(128);
            entity.Property(e => e.QuizTemplateId).HasMaxLength(128).IsRequired();
            entity.Property(e => e.QuizSnapshotId).HasMaxLength(128).IsRequired();
            entity.Property(e => e.StartTime);
            entity.Property(e => e.SubmitTime);
            entity.Property(e => e.Score).IsRequired();
            entity.Property(e => e.Status).IsRequired();

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
                .HasFilter($"{nameof(QuizAttempt.AssignId)} IS NOT NULL");
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

            entity.Property(e => e.Id).HasMaxLength(128).IsRequired();
            entity.Property(e => e.MarkUpContent).IsRequired();
            entity.Property(e => e.IsCorrect);
            entity.Property(e => e.QuizAttemptId).HasMaxLength(128).IsRequired();
            entity.Property(e => e.SortOrder).IsRequired();
            entity.Property(e => e.QuestionSnapshotId).HasMaxLength(128).IsRequired();

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

            entity.Property(e => e.Id).HasMaxLength(128).IsRequired();
            entity.Property(e => e.QuizAttemptId).HasMaxLength(128).IsRequired();
            entity.Property(e => e.QuestionRecordId).HasMaxLength(128).IsRequired();
            entity.Property(e => e.Value).IsRequired();

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
