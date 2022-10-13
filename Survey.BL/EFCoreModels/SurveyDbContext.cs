using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

namespace Survey.BL.EFCoreModels
{
    public partial class SurveyDbContext : DbContext
    {
        public SurveyDbContext()
        {
        }

        public SurveyDbContext(DbContextOptions<SurveyDbContext> options)
            : base(options)
        {
            
        }

        public virtual DbSet<QuestionType> QuestionTypes { get; set; } = null!;
        public virtual DbSet<Section> Sections { get; set; } = null!;
        public virtual DbSet<SectionQuestion> SectionQuestions { get; set; } = null!;
        public virtual DbSet<Survey> Surveys { get; set; } = null!;
        public virtual DbSet<SurveyResponse> SurveyResponses { get; set; } = null!;
        public virtual DbSet<SurveyResponseJson> SurveyResponseJsons { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                //                optionsBuilder.UseSqlServer("Data Source=localhost;Initial Catalog=SurveyDb;Integrated Security=True");
                //optionsBuilder.UseSqlServer(Configuration.GetConnectionString("BloggingDatabase"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<QuestionType>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.TypeName).HasMaxLength(50);
            });

            modelBuilder.Entity<Section>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(1000);

                entity.HasOne(d => d.Survey)
                    .WithMany(p => p.Sections)
                    .HasForeignKey(d => d.SurveyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Sections_Surveys");
            });

            modelBuilder.Entity<SectionQuestion>(entity =>
            {
                entity.HasOne(d => d.Section)
                    .WithMany(p => p.SectionQuestions)
                    .HasForeignKey(d => d.SectionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SectionQuestions_Sections");
            });

            modelBuilder.Entity<Survey>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(1000);
            });

            modelBuilder.Entity<SurveyResponseJson>(entity =>
 {
     entity.HasKey(e => e.SurveyId);

     entity.Property(e => e.SurveyId).ValueGeneratedNever();

     entity.Property(e => e.ResponseInJson).HasColumnName("ResponseInJson");

     entity.HasOne(d => d.Survey)
         .WithOne(p => p.SurveyResponseJson)
         .HasForeignKey<SurveyResponseJson>(d => d.SurveyId)
         .OnDelete(DeleteBehavior.ClientSetNull)
         .HasConstraintName("FK_SurveyResponseJsons_Surveys");
 });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
