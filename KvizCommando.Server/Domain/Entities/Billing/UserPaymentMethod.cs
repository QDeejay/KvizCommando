using System;

namespace KvizCommando.Server.Domain.Entities.Billing
{
    /// <summary>
    /// Fizetési mód meta adatok tokenizált feldolgozóval (nincs PAN/CVV tárolás).
    /// </summary>
    public class UserPaymentMethod
    {
        public long Id { get; set; } // PK (IDENTITY SQL Serveren, INTEGER ROWID SQLite-on)
        public string UserId { get; set; } = null!; // FK → AspNetUsers.Id

        public string Processor { get; set; } = string.Empty;          // pl. "Stripe", "Barion"
        public string PaymentMethodToken { get; set; } = string.Empty;  // processzor token

        public string CardBrand { get; set; } = string.Empty;           // pl. "Visa"
        public string CardLast4 { get; set; } = string.Empty;           // "1234"
        public int ExpMonth { get; set; }
        public int ExpYear { get; set; }

        public bool IsDefault { get; set; }

        public DateTime CreatedUtc { get; set; }
        public DateTime UpdatedUtc { get; set; }
    }
}
