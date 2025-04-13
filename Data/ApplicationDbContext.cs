using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BeFit.Models;

namespace BeFit.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ExerciseType> ExerciseTypes { get; set; }
        public DbSet<TrainingSession> TrainingSessions { get; set; }
        public DbSet<CompletedExercise> CompletedExercises { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TrainingSession>()
                .HasMany(t => t.CompletedExercises)
                .WithOne(e => e.TrainingSession)
                .HasForeignKey(e => e.TrainingSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ExerciseType>()
                .HasMany(t => t.CompletedExercises)
                .WithOne(e => e.ExerciseType)
                .HasForeignKey(e => e.ExerciseTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}