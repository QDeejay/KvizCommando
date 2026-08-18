using KvizCommando.Server.Domain.Entities.Questions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KvizCommando.Server.Infrastructure.Persistence.Configurations
{
    public class UserQuestionConfiguration : IEntityTypeConfiguration<UserQuestion>
    {
        public void Configure(EntityTypeBuilder<UserQuestion> builder)
        {
            builder.HasKey(u => u.Id);

            // PlayerId index kereséshez
            builder.HasIndex(u => u.PlayerId);

            // Számított oszlop konfiguráció
            builder.Property(u => u.Ratio)
                .HasComputedColumnSql(
                    // SQLite
                    "CASE WHEN Ask > 0 THEN CAST(OkAnswer AS REAL) / CAST(Ask AS REAL) ELSE 0 END",
                    stored: false
                );
            // SQL Server verzió (kommentben)
            //.HasComputedColumnSql("CAST(CASE WHEN Ask > 0 THEN CAST(OkAnswer AS FLOAT) / CAST(Ask AS FLOAT) ELSE 0 END AS FLOAT)", stored: false);
        }
    }
}
