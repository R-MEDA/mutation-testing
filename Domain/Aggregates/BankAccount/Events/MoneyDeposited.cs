namespace Domain.Aggregates.BankAccount.Events
{
    public record MoneyDeposited(AccountId AccountId, decimal Amount, string Description) : Event(AccountId.Id);
}
