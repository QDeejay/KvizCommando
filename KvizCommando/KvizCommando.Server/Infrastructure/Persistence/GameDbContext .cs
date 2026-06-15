using KvizCommando.Server.Domain.Entities.Questions;
using KvizCommando.Server.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace KvizCommando.Server.Infrastructure.Persistence
{
    public class GameDbContext : DbContext
    {
        public GameDbContext(DbContextOptions<GameDbContext> options)
            : base(options)
        {
        }

        public DbSet<GuessQuestion> GuessQuestions { get; set; }
        public DbSet<FactoryQuestion> FactoryQuestions { get; set; }
        public DbSet<UserQuestion> UserQuestions { get; set; }
        public DbSet<PendingQuestion> PendingQuestions { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new UserQuestionConfiguration());
            modelBuilder.ApplyConfiguration(new PendingQuestionConfiguration());


        }

    }
}


/// Add-Migration InitialGame -Context GameDbContext -OutputDir "Data\Migrations\Game"
/// Update-Database -Context GameDbContext


/*
 
 
 
 */