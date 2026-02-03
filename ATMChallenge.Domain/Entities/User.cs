namespace ATMChallenge.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public int AccountId { get; set; }
        public Account? Account { get; set; }
    }
}