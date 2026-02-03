namespace ATMChallenge.Application.DTOs.Responses
{
    public record BalanceResponse(string UserFullName, string AccountNumber, decimal Balance, DateTime? LastWithdrawal);
}