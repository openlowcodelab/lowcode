using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace H.Education.EntityFrameworkCore;

/// <summary>
/// 中小学教育应用数据库上下文
/// </summary>
[ConnectionStringName("EducationDb")]
public class EducationDbContext : AbpDbContext<EducationDbContext>
{
    /// <summary>学生档案</summary>
    public DbSet<StudentEntity> Students { get; set; } = null!;

    /// <summary>试卷</summary>
    public DbSet<ExamPaperEntity> ExamPapers { get; set; } = null!;

    /// <summary>试卷题目</summary>
    public DbSet<ExamQuestionEntity> ExamQuestions { get; set; } = null!;

    /// <summary>答卷</summary>
    public DbSet<AnswerSheetEntity> AnswerSheets { get; set; } = null!;

    /// <summary>答卷明细</summary>
    public DbSet<AnswerDetailEntity> AnswerDetails { get; set; } = null!;

    /// <summary>趣味练习对局</summary>
    public DbSet<PracticeSessionEntity> PracticeSessions { get; set; } = null!;

    /// <summary>趣味练习题目</summary>
    public DbSet<PracticeItemEntity> PracticeItems { get; set; } = null!;

    /// <summary>教材目录（一册一行）</summary>
    public DbSet<TextbookEntity> Textbooks { get; set; } = null!;

    /// <summary>课时资源：视频课程、课件、教学设计、学习任务单、课后练习</summary>
    public DbSet<TeachingResourceEntity> TeachingResources { get; set; } = null!;

    public EducationDbContext(DbContextOptions<EducationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<StudentEntity>(b =>
        {
            b.ToTable("Students");
            b.HasKey(x => x.Id);
            b.Property(x => x.StudentNo).HasMaxLength(64);
            b.Property(x => x.Name).IsRequired().HasMaxLength(64);
            b.Property(x => x.Grade).IsRequired().HasMaxLength(32);
            b.Property(x => x.ClassName).HasMaxLength(64);
            b.Property(x => x.GuardianName).HasMaxLength(64);
            b.Property(x => x.GuardianPhone).HasMaxLength(32);
            b.Property(x => x.Remark).HasMaxLength(500);

            b.HasIndex(x => x.Name);
            b.HasIndex(x => x.Grade);
            // 学号允许留空，SQL Server 唯一索引会把多个 NULL 视为重复，因此加过滤条件
            b.HasIndex(x => x.StudentNo).IsUnique().HasFilter("[StudentNo] IS NOT NULL");
        });

        modelBuilder.Entity<ExamPaperEntity>(b =>
        {
            b.ToTable("ExamPapers");
            b.HasKey(x => x.Id);
            b.Property(x => x.Title).IsRequired().HasMaxLength(200);
            b.Property(x => x.Grade).IsRequired().HasMaxLength(32);
            b.Property(x => x.Term).HasMaxLength(32);
            b.Property(x => x.Unit).HasMaxLength(32);
            b.Property(x => x.Textbook).HasMaxLength(32);
            b.Property(x => x.Description).HasMaxLength(1000);
            b.Property(x => x.AiPrompt).HasColumnType("nvarchar(max)");
            b.Property(x => x.AiModel).HasMaxLength(64);
            b.Property(x => x.GenerateError).HasMaxLength(1000);
            b.Property(x => x.GenerateInput).HasColumnType("nvarchar(max)");
            b.Property(x => x.TotalScore).HasColumnType("decimal(18,2)");

            b.HasIndex(x => x.Subject);
            b.HasIndex(x => x.Grade);
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.GenerateState);
        });

        modelBuilder.Entity<ExamQuestionEntity>(b =>
        {
            b.ToTable("ExamQuestions");
            b.HasKey(x => x.Id);
            b.Property(x => x.Section).HasMaxLength(100);
            b.Property(x => x.Content).IsRequired().HasColumnType("nvarchar(max)");
            b.Property(x => x.OptionsJson).HasColumnType("nvarchar(max)");
            b.Property(x => x.Answer).IsRequired().HasColumnType("nvarchar(max)");
            b.Property(x => x.Analysis).HasColumnType("nvarchar(max)");
            b.Property(x => x.Score).HasColumnType("decimal(18,2)");

            b.HasIndex(x => x.PaperId);
            b.HasOne(x => x.Paper)
                .WithMany()
                .HasForeignKey(x => x.PaperId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AnswerSheetEntity>(b =>
        {
            b.ToTable("AnswerSheets");
            b.HasKey(x => x.Id);
            b.Property(x => x.PaperTitle).IsRequired().HasMaxLength(200);
            b.Property(x => x.StudentName).IsRequired().HasMaxLength(64);
            b.Property(x => x.GradeNote).HasMaxLength(500);
            b.Property(x => x.FullScore).HasColumnType("decimal(18,2)");
            b.Property(x => x.ObjectiveScore).HasColumnType("decimal(18,2)");
            b.Property(x => x.SubjectiveScore).HasColumnType("decimal(18,2)");
            b.Property(x => x.TotalScore).HasColumnType("decimal(18,2)");
            b.Property(x => x.AccuracyRate).HasColumnType("decimal(18,2)");

            b.HasIndex(x => x.PaperId);
            b.HasIndex(x => x.StudentId);
            b.HasIndex(x => x.StudentName);
            b.HasIndex(x => x.Status);
            b.HasOne(x => x.Paper)
                .WithMany()
                .HasForeignKey(x => x.PaperId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AnswerDetailEntity>(b =>
        {
            b.ToTable("AnswerDetails");
            b.HasKey(x => x.Id);
            b.Property(x => x.QuestionContent).IsRequired().HasColumnType("nvarchar(max)");
            b.Property(x => x.OptionsJson).HasColumnType("nvarchar(max)");
            b.Property(x => x.StudentAnswer).HasColumnType("nvarchar(max)");
            b.Property(x => x.CorrectAnswer).IsRequired().HasColumnType("nvarchar(max)");
            b.Property(x => x.Analysis).HasColumnType("nvarchar(max)");
            b.Property(x => x.Feedback).HasColumnType("nvarchar(max)");
            b.Property(x => x.Score).HasColumnType("decimal(18,2)");
            b.Property(x => x.AwardedScore).HasColumnType("decimal(18,2)");

            b.HasIndex(x => x.SheetId);
            b.HasIndex(x => x.QuestionId);
            b.HasOne(x => x.Sheet)
                .WithMany()
                .HasForeignKey(x => x.SheetId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PracticeSessionEntity>(b =>
        {
            b.ToTable("PracticeSessions");
            b.HasKey(x => x.Id);
            b.Property(x => x.Grade).IsRequired().HasMaxLength(32);
            b.Property(x => x.StudentName).IsRequired().HasMaxLength(64);

            b.HasIndex(x => x.GameType);
            b.HasIndex(x => x.StudentId);
            b.HasIndex(x => x.StudentName);
            b.HasIndex(x => x.Status);
        });

        modelBuilder.Entity<PracticeItemEntity>(b =>
        {
            b.ToTable("PracticeItems");
            b.HasKey(x => x.Id);
            b.Property(x => x.Content).IsRequired().HasColumnType("nvarchar(max)");
            b.Property(x => x.OptionsJson).HasColumnType("nvarchar(max)");
            b.Property(x => x.Answer).IsRequired().HasMaxLength(500);
            b.Property(x => x.Hint).HasMaxLength(200);
            b.Property(x => x.AudioText).HasMaxLength(500);
            b.Property(x => x.StudentAnswer).HasMaxLength(500);

            b.HasIndex(x => x.SessionId);
            b.HasOne(x => x.Session)
                .WithMany()
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TextbookEntity>(b =>
        {
            b.ToTable("Textbooks");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(200);
            b.Property(x => x.Version).IsRequired().HasMaxLength(32);
            b.Property(x => x.Grade).IsRequired().HasMaxLength(32);
            b.Property(x => x.Publisher).HasMaxLength(64);
            b.Property(x => x.UnitLabel).HasMaxLength(32);
            b.Property(x => x.BookUrl).HasMaxLength(500);
            b.Property(x => x.CourseUrl).HasMaxLength(500);
            b.Property(x => x.Remark).HasMaxLength(500);

            b.HasIndex(x => x.Stage);
            b.HasIndex(x => x.Subject);
            b.HasIndex(x => x.Grade);
            // 同一学科+版本+年级+册次只允许一本，导入内置目录靠它兜底去重
            b.HasIndex(x => new { x.Subject, x.Version, x.Grade, x.Volume }).IsUnique();
        });

        modelBuilder.Entity<TeachingResourceEntity>(b =>
        {
            b.ToTable("TeachingResources");
            b.HasKey(x => x.Id);
            b.Property(x => x.Unit).IsRequired().HasMaxLength(64);
            b.Property(x => x.Lesson).HasMaxLength(100);
            b.Property(x => x.Title).IsRequired().HasMaxLength(200);
            b.Property(x => x.Url).HasMaxLength(1000);
            b.Property(x => x.SourceName).HasMaxLength(100);
            b.Property(x => x.Remark).HasMaxLength(500);

            b.HasIndex(x => x.TextbookId);
            b.HasIndex(x => x.Type);
            b.HasIndex(x => new { x.TextbookId, x.UnitSort, x.Type });
            b.HasOne(x => x.Textbook)
                .WithMany()
                .HasForeignKey(x => x.TextbookId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
