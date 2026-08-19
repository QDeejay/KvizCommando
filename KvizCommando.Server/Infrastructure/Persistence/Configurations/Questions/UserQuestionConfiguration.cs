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

            // A számított oszlop SQL-kifejezését a provider konfigurációja adja meg.
        }
    }
}
