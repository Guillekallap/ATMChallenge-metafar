namespace ATMChallenge.Application.DTOs.Requests
{
    public record WithdrawRequest(string CardNumber, decimal Amount);
}