namespace ATMChallenge.Domain.Entities
{
    public class Card
    {
        public int Id { get; set; }
        public string CardNumber { get; set; } = null!;
        public string PinHash { get; set; } = null!;
        public bool IsBlocked { get; set; }
        public int FailedPinAttempts { get; set; }
        public int AccountId { get; set; }
        public Account? Account { get; set; }

        // Auditoría básica
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}