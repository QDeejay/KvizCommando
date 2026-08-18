using KvizCommando.Server.Domain.Entities.Players;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KvizCommando.Server.Infrastructure.Persistence.Configurations
{
    public class PlayerLoadoutConfiguration : IEntityTypeConfiguration<PlayerLoadout>
    {
        /// <summary>
        /// Beállítja az entitás EF Core leképezését és adatbázis-korlátait.
        /// </summary>
        public void Configure(EntityTypeBuilder<PlayerLoadout> b)
        {
            b.ToTable("PlayerLoadouts");

            b.HasKey(pl => pl.PlayerId);

            // SQLite-konfiguráció
            b.Property(pl => pl.FactorySlotsJson).IsRequired().HasColumnType("TEXT");
            b.Property(pl => pl.UserSlotsJson).IsRequired().HasColumnType("TEXT");
            b.Property(pl => pl.PendingSlotsJson).IsRequired().HasColumnType("TEXT");

            // Az SQLite JSON-ellenőrzése megakadályozza sérült kérdéslisták mentését.
            b.ToTable(t =>
            {
                t.HasCheckConstraint("CK_PlayerLoadouts_FactorySlots_Json", "json_valid([FactorySlotsJson])");
                t.HasCheckConstraint("CK_PlayerLoadouts_UserSlots_Json", "json_valid([UserSlotsJson])");
                t.HasCheckConstraint("CK_PlayerLoadouts_PendingSlots_Json", "json_valid([PendingSlotsJson])");
            });

            // SQL Server alternatíva
            // b.Property(pl => pl.FactorySlotsJson).IsRequired().HasColumnType("nvarchar(max)");
            // b.Property(pl => pl.UserSlotsJson).IsRequired().HasColumnType("nvarchar(max)");
            // b.Property(pl => pl.PendingSlotsJson).IsRequired().HasColumnType("nvarchar(max)");
            // b.ToTable(t =>
            // {
            //     t.HasCheckConstraint("CK_PlayerLoadouts_FactorySlots_Json", "ISJSON([FactorySlotsJson]) = 1");
            //     t.HasCheckConstraint("CK_PlayerLoadouts_UserSlots_Json", "ISJSON([UserSlotsJson]) = 1");
            //     t.HasCheckConstraint("CK_PlayerLoadouts_PendingSlots_Json", "ISJSON([PendingSlotsJson]) = 1");
            // });

            b.Property(pl => pl.UpdatedUtc).IsRequired();
        }
    }
}
