using KvizCommando.Server.Domain.Entities.Questions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KvizCommando.Server.Infrastructure.Persistence.Configurations
{
    public class UserQuestionConfiguration : IEntityTypeConfiguration<UserQuestion>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<UserQuestion> builder)
        {
            builder.HasKey(u => u.Id);

            // A játékoshoz tartozó kérdések lekérdezését külön index támogatja.
            builder.HasIndex(u => u.PlayerId);

            // SQLite alatt a REAL típus biztosítja, hogy az osztás ne egész számként történjen.
            builder.Property(u => u.Ratio)
                .HasComputedColumnSql(
                    "CASE WHEN Ask > 0 THEN CAST(OkAnswer AS REAL) / CAST(Ask AS REAL) ELSE 0 END",
                    stored: false
                );

            // SQL Server használatakor a fenti kifejezést erre kell cserélni:
            // .HasComputedColumnSql("CAST(CASE WHEN Ask > 0 THEN CAST(OkAnswer AS FLOAT) / CAST(Ask AS FLOAT) ELSE 0 END AS FLOAT)", stored: false);
        }
    }
}
