using System.ComponentModel.DataAnnotations.Schema;

namespace ATMChallenge.Domain.Entities
{
    public class Account
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; } = null!;
        public decimal Balance { get; set; }
        public DateTime? LastWithdrawal { get; set; }

        // Auditoría básica
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Token de concurrencia para evitar race conditions
        public byte[]? RowVersion { get; set; }

        // Navigation
        public List<Card>? Cards { get; set; }
        public List<Operation>? Operations { get; set; }
    }
}