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


/*
 using KvizCommando.Server.Domain.Entities.Players;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KvizCommando.Server.Infrastructure.Persistence.Configurations
{
    public class PlayerCharacterConfiguration : IEntityTypeConfiguration<PlayerCharacter>
    {
        public void Configure(EntityTypeBuilder<PlayerCharacter> b)
        {
            b.ToTable("PlayerCharacters");

            b.HasKey(pc => new { pc.PlayerId, pc.SlotNo });

            b.Property(pc => pc.SlotNo)
             .HasColumnType("INTEGER"); // SQLite nem különböztet meg tinyint/int → mind INTEGER

            /// SQLite nem támogatja a tinyint típust
            //b.Property(pc => pc.SlotNo)
            // .HasColumnType("tinyint");

            // CHECK constraint: 1..8
            b.ToTable(t => t.HasCheckConstraint("CK_PlayerCharacters_SlotNo_Range", "[SlotNo] BETWEEN 1 AND 8"));


            /// sqlite verziótól függően:
            b.Property(pc => pc.AttitudeJson)
 .IsRequired()
 .HasColumnType("TEXT"); // nvarchar(max) → TEXT

            b.Property(pc => pc.CharStatisticJson)
             .IsRequired()
             .HasColumnType("TEXT"); // nvarchar(max) → TEXT


            /// sql server esetén:
            // JSON oszlopok és CHECK ISJSON
            // b.Property(pc => pc.AttitudeJson)
            //  .IsRequired()
            //  .HasColumnType("nvarchar(max)");

            //            b.Property(pc => pc.CharStatisticJson)
            //           .IsRequired()
            //         .HasColumnType("nvarchar(max)");


            /// sqlite verziótól függően:
            b.ToTable(t =>
            {
                t.HasCheckConstraint("CK_PlayerCharacters_Attitude_Json", "json_valid([AttitudeJson])");
                t.HasCheckConstraint("CK_PlayerCharacters_CharStatistic_Json", "json_valid([CharStatisticJson])");
            });


            /// SQL Server verziótól függően:
            //b.ToTable(t => t.HasCheckConstraint("CK_PlayerCharacters_Attitude_Json", "ISJSON([AttitudeJson]) = 1"));
            //b.ToTable(t => t.HasCheckConstraint("CK_PlayerCharacters_CharStatistic_Json", "ISJSON([CharStatisticJson]) = 1"));

            b.Property(pc => pc.Name).HasMaxLength(64);
            /// sqlite nem támogatja a max length-et stringnél
            b.Property(pc => pc.CreatedUtc).HasColumnType("TEXT").IsRequired();
            b.Property(pc => pc.UpdatedUtc).HasColumnType("TEXT").IsRequired();
            /// sql server esetén:
            //b.Property(pc => pc.CreatedUtc).IsRequired();
            //b.Property(pc => pc.UpdatedUtc).IsRequired();

            b.Property(pc => pc.RowVersion)
            // .IsRowVersion()   // SQLite nem támogatja szervernél visszakapcsolni
             .IsConcurrencyToken();

            // Gyors aggregációkhoz bevonó index
            b.HasIndex(pc => pc.PlayerId)
             .HasDatabaseName("IX_PlayerCharacters_PlayerId");
        }
    }
}
 
 */