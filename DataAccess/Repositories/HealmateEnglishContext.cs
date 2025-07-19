using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Models;

public partial class HealmateEnglishContext : DbContext
{
    public HealmateEnglishContext()
    {
    }

    public HealmateEnglishContext(DbContextOptions<HealmateEnglishContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Apilog> Apilogs { get; set; }

    public virtual DbSet<PresetReading> PresetReadings { get; set; }

    public virtual DbSet<PresetWritingTopic> PresetWritingTopics { get; set; }

    public virtual DbSet<ReadingOption> ReadingOptions { get; set; }

    public virtual DbSet<ReadingQuestion> ReadingQuestions { get; set; }

    public virtual DbSet<ReadingSession> ReadingSessions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<WritingSession> WritingSessions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(
                  "Server= LAPTOP-1MCPK8AU\\SQLEXPRESS; Database=HealmateEnglish; Uid=sa; Pwd=123; TrustServerCertificate=True"
               );
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Apilog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__APILogs__9E2397E0EE575825");

            entity.ToTable("APILogs");

            entity.Property(e => e.LogId).HasColumnName("log_id");
            entity.Property(e => e.ApiResponseStatus)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("api_response_status");
            entity.Property(e => e.RequestType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("request_type");
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("timestamp");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Apilogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__APILogs__user_id__5F7E2DAC");
        });

        modelBuilder.Entity<PresetReading>(entity =>
        {
            entity.HasKey(e => e.PresetId).HasName("PK__PresetRe__01ED5E7EE90888E1");

            entity.Property(e => e.PresetId).HasColumnName("preset_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Part)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("part");
            entity.Property(e => e.Passage).HasColumnName("passage");
            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .HasColumnName("title");
            entity.Property(e => e.IsAiCreated)
                .HasColumnName("IsAiCreated")
                .HasDefaultValue(false);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.PresetReadings)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PresetRea__creat__43D61337");
        });

        modelBuilder.Entity<PresetWritingTopic>(entity =>
        {
            entity.HasKey(e => e.TopicId).HasName("PK__PresetWr__D5DAA3E9AF3C7AC2");

            entity.Property(e => e.TopicId).HasColumnName("topic_id");
            entity.Property(e => e.Band)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("band");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .HasColumnName("title");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.PresetWritingTopics)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PresetWri__creat__47A6A41B");
        });

        modelBuilder.Entity<ReadingOption>(entity =>
        {
            entity.HasKey(e => e.OptionId).HasName("PK__ReadingO__F4EACE1BD813A980");

            entity.Property(e => e.OptionId).HasColumnName("option_id");
            entity.Property(e => e.IsCorrect)
                .HasDefaultValue(false)
                .HasColumnName("is_correct");
            entity.Property(e => e.OptionLabel)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("option_label");
            entity.Property(e => e.OptionText)
                .HasMaxLength(255)
                .HasColumnName("option_text");
            entity.Property(e => e.QuestionId).HasColumnName("question_id");
            entity.Property(e => e.UserSelected)
                .HasDefaultValue(false)
                .HasColumnName("user_selected");

            entity.HasOne(d => d.Question).WithMany(p => p.ReadingOptions)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("FK__ReadingOp__quest__55F4C372");
        }); modelBuilder.Entity<ReadingQuestion>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PK__ReadingQ__2EC2154968AEC159");

            entity.Property(e => e.QuestionId).HasColumnName("question_id");
            entity.Property(e => e.Explanation).HasColumnName("explanation");
            entity.Property(e => e.QuestionText).HasColumnName("question_text");
            entity.Property(e => e.PresetId).HasColumnName("preset_id");
            entity.Property(e => e.IsMultipleChoice)
                .HasColumnName("is_multiple_choice")
                .HasDefaultValue(false);

            entity.HasOne(d => d.Preset).WithMany(p => p.ReadingQuestions)
                .HasForeignKey(d => d.PresetId)
                .HasConstraintName("FK__ReadingQu__prese__503BEA1C");
        });

        modelBuilder.Entity<ReadingSession>(entity =>
        {
            entity.HasKey(e => e.SessionId).HasName("PK__ReadingS__69B13FDCD98747D6");

            entity.Property(e => e.SessionId).HasColumnName("session_id");
            entity.Property(e => e.Band)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("band");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Passage).HasColumnName("passage");
            entity.Property(e => e.PresetId).HasColumnName("preset_id");
            entity.Property(e => e.Score).HasColumnName("score");
            entity.Property(e => e.SourceType)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("source_type");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Preset).WithMany(p => p.ReadingSessions)
                .HasForeignKey(d => d.PresetId)
                .HasConstraintName("FK__ReadingSe__prese__4D5F7D71");

            entity.HasOne(d => d.User).WithMany(p => p.ReadingSessions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__ReadingSe__user___4C6B5938");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__B9BE370F0E4F50FD");

            entity.HasIndex(e => e.Email, "UQ__Users__AB6E61644CFE3435").IsUnique();

            entity.HasIndex(e => e.Username, "UQ__Users__F3DBC572C7132036").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.IsAdmin)
                .HasDefaultValue(false)
                .HasColumnName("is_admin");
            entity.Property(e => e.Level)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Beginner")
                .HasColumnName("level");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("username");
        });

        modelBuilder.Entity<WritingSession>(entity =>
        {
            entity.HasKey(e => e.SessionId).HasName("PK__WritingS__69B13FDC6267CD5B");

            entity.Property(e => e.SessionId).HasColumnName("session_id");
            entity.Property(e => e.AiFeedback).HasColumnName("ai_feedback");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomTopic)
                .HasMaxLength(255)
                .HasColumnName("custom_topic");
            entity.Property(e => e.Score).HasColumnName("score");
            entity.Property(e => e.SourceType)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("source_type");
            entity.Property(e => e.TopicId).HasColumnName("topic_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.UserText).HasColumnName("user_text");

            entity.HasOne(d => d.Topic).WithMany(p => p.WritingSessions)
                .HasForeignKey(d => d.TopicId)
                .HasConstraintName("FK__WritingSe__topic__5BAD9CC8");

            entity.HasOne(d => d.User).WithMany(p => p.WritingSessions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__WritingSe__user___5AB9788F");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
