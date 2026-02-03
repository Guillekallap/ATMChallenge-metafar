namespace ATMChallenge.Application.DTOs.Responses
{
    public record OperationDto(string Type, decimal Amount, DateTime Timestamp);

    public record PagedOperationsResponse(int Page, int PageSize, int TotalItems, List<OperationDto> Items);
}