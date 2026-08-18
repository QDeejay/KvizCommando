using KvizCommando.Server.Domain.Entities.Players;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KvizCommando.Server.Infrastructure.Persistence.Configurations
{
    public class PlayerCharactersConfiguration : IEntityTypeConfiguration<PlayerCharacter>
    {
        public void Configure(EntityTypeBuilder<PlayerCharacter> b)
        {
            b.ToTable("PlayerCharacters");

            b.HasKey(pc => pc.PlayerId);

            // SQLite verzió – TEXT
            b.Property(pc => pc.CharactersJson)
             .IsRequired()
             .HasColumnType("TEXT");

            b.Property(pc => pc.CandidatesJson)
            .IsRequired()
            .HasColumnType("TEXT");

            b.ToTable(t =>
            {
                t.HasCheckConstraint(
                    "CK_PlayerCharacters_CharactersJson_Valid",
                    "json_valid([CharactersJson])"
                );

                t.HasCheckConstraint(
                    "CK_PlayerCharacters_CandidatesJson_Valid",
                    "json_valid([CandidatesJson])"
                );
            });

            // SQL Server verzió
            // b.Property(pc => pc.CharactersJson)
            //  .IsRequired()
            //  .HasColumnType("nvarchar(max)");
            //b.Property(pc => pc.CandidatesJson)
            //  .IsRequired()
            //  .HasColumnType("nvarchar(max)");
            //
            // b.ToTable(t =>
            // {
            //     t.HasCheckConstraint("CK_PlayerCharacters_CharactersJson_Valid", "ISJSON([CharactersJson]) = 1");
            // });
            // b.ToTable(t =>
            // {
            //     t.HasCheckConstraint("CK_PlayerCharacters_CandidatesJson_Valid", "ISJSON([CandidatesJson]) = 1");
            // });

            b.Property(pc => pc.UpdatedUtc).IsRequired();
        }
    }
}
