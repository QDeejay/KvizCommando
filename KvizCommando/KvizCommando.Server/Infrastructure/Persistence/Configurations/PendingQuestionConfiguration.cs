using KvizCommando.Server.Domain.Entities.Questions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace KvizCommando.Server.Infrastructure.Persistence.Configurations
{
    public class PendingQuestionConfiguration : IEntityTypeConfiguration<PendingQuestion>
    {
        public void Configure(EntityTypeBuilder<PendingQuestion> builder)
        {
            builder.HasKey(u => u.Id);

            // PlayerId index kereséshez
            builder.HasIndex(u => u.PlayerId);

            builder.Property(q => q.Status)
                   .HasConversion<string>(); // Status stringként, olvashatóbb DB-ben
        }
    }
}
