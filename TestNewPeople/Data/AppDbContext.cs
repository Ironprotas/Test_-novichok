using Microsoft.EntityFrameworkCore;
using TestNewPeople.Models;

namespace TestNewPeople.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Newcomer> Newcomers => Set<Newcomer>();
    public DbSet<OnboardingTask> Tasks => Set<OnboardingTask>();
    public DbSet<WeekSurvey> Surveys => Set<WeekSurvey>();
    public DbSet<HelpCase> Cases => Set<HelpCase>();
    public DbSet<MentorMark> MentorMarks => Set<MentorMark>();
    public DbSet<ChangeLog> ChangeLogs => Set<ChangeLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Newcomer>(e =>
        {
            e.HasKey(x => x.Id);
            e.OwnsOne(x => x.Mentor);
            e.HasMany(x => x.Tasks)
                .WithOne(x => x.Newcomer)
                .HasForeignKey(x => x.NewcomerId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Survey)
                .WithOne(x => x.Newcomer)
                .HasForeignKey<WeekSurvey>(x => x.NewcomerId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Cases)
                .WithOne(x => x.Newcomer)
                .HasForeignKey(x => x.NewcomerId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.MentorMarks)
                .WithOne(x => x.Newcomer)
                .HasForeignKey(x => x.NewcomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OnboardingTask>(e =>
        {
            e.HasKey(x => new { x.NewcomerId, x.Id });
        });

        modelBuilder.Entity<WeekSurvey>(e =>
        {
            e.HasKey(x => x.NewcomerId);
            e.Ignore(x => x.IsCompleted);
        });

        modelBuilder.Entity<HelpCase>(e => e.HasKey(x => x.Id));
        modelBuilder.Entity<MentorMark>(e => e.HasKey(x => x.Id));
        modelBuilder.Entity<ChangeLog>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.AtUtc);
        });
    }
}