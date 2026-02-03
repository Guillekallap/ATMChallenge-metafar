namespace ATMChallenge.Domain.Entities
{
    public enum OperationType
    {
        Withdrawal,
        Deposit
    }

    public class Operation
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public Account? Account { get; set; }
        public OperationType Type { get; set; }
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }

        // Auditoría
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}