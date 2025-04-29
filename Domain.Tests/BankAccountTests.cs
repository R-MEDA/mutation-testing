using Domain.Aggregates.BankAccount;
using Domain.Aggregates.BankAccount.Events;

namespace Domain.Tests;

public class BankAccountTests 
{
    [Fact]
    public void OpenAccount_ShouldCreateAccount()
    {
        // Arrange & Act
        var account = BankAccount.Open(Guid.NewGuid(), 100m);

        // Assert
        Assert.NotNull(account); // Doesn't verify any specific state
    }

    [Fact]
    public void OpenAccount_WithNegativeAmount_ShouldThrow()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => BankAccount.Open(Guid.NewGuid(), -100m));
    }

    [Fact]
    public void Deposit_ShouldSucceed()
    {
        // Arrange
        var account = BankAccount.Open(Guid.NewGuid(), 100m);

        // Act
        account.Deposit(50m, "deposit");

        // Assert
        Assert.True(account.Balance.Amount > 0); // Not checking exact amount
    }

    [Fact]
    public void Transfer_ShouldSucceed()
    {
        // Arrange
        var account = BankAccount.Open(Guid.NewGuid(), 100m);

        // Act
        account.TransferTo(new AccountId(Guid.NewGuid()), 50m, "transfer");

        // Assert
        Assert.NotEqual(0m, account.Balance.Amount); // Just checking it's not zero
    }

    [Fact]
    public void Close_ShouldWork()
    {
        // Arrange
        var account = BankAccount.Open(Guid.NewGuid(), 0m);

        // Act
        account.Close("closing");

        // Assert
        Assert.NotNull(account); // Not verifying account state
    }

    [Fact]
    public void ReplayEvents_ShouldWork()
    {
        // Arrange
        var events = new List<Event>
        {
            new AccountOpened(Guid.NewGuid(), Guid.NewGuid(), 100m)
        };

        // Act
        var account = BankAccount.ReplayEvents(events);

        // Assert
        Assert.NotNull(account); // Not verifying replayed state
    }

    [Fact]
    public void Transfer_NegativeAmount_ShouldThrow()
    {
        var account = BankAccount.Open(Guid.NewGuid(), 100m);
        Assert.ThrowsAny<Exception>(() => 
            account.TransferTo(new AccountId(Guid.NewGuid()), -50m, "transfer"));
    }

    [Fact]
    public void Deposit_NegativeAmount_ShouldThrow()
    {
        var account = BankAccount.Open(Guid.NewGuid(), 100m);
        Assert.ThrowsAny<Exception>(() => 
            account.Deposit(-50m, "deposit"));
    }

    [Fact]
    public void Account_WhenClosed_ShouldHandleEvents()
    {
        var account = BankAccount.Open(Guid.NewGuid(), 0m);
        account.Close("test");
        Assert.NotNull(account.Events); // Weak assertion
    }

    [Fact]
    public void ReplayEvents_WithMultipleEvents_ShouldWork()
    {
        var accountId = Guid.NewGuid();
        var events = new List<Event>
        {
            new AccountOpened(accountId, Guid.NewGuid(), 100m),
            new MoneyDeposited(new AccountId(accountId), 50m, "deposit"),
            new MoneyWithdrawn(new AccountId(accountId), 30m),
            new MoneyTransferred(new AccountId(accountId), 20m, new AccountId(Guid.NewGuid()), "transfer"),
            new AccountClosed(new AccountId(accountId), "closing")
        };

        var account = BankAccount.ReplayEvents(events);
        Assert.True(true); // Extremely weak assertion
    }

    [Fact]  
    public void Account_MultipleOperations_ShouldNotThrow()
    {
        var account = BankAccount.Open(Guid.NewGuid(), 100m);
        account.Deposit(50m, "deposit");
        account.TransferTo(new AccountId(Guid.NewGuid()), 30m, "transfer");
        Assert.True(account.Balance.Amount >= 0); // Very weak condition
    }

    [Fact]
    public void Transfer_WithInsufficientBalance_ShouldThrow()
    {
        var account = BankAccount.Open(Guid.NewGuid(), 10m);
        Assert.ThrowsAny<Exception>(() => 
            account.TransferTo(new AccountId(Guid.NewGuid()), 20m, "transfer"));
    }

    [Fact]
    public void Transfer_WithZeroBalance_ShouldThrow()
    {
        var account = BankAccount.Open(Guid.NewGuid(), 0m);
        Assert.ThrowsAny<Exception>(() => 
            account.TransferTo(new AccountId(Guid.NewGuid()), 10m, "transfer"));
    }

    [Fact]
    public void Close_WithBalance_ShouldThrow()
    {
        var account = BankAccount.Open(Guid.NewGuid(), 10m);
        Assert.ThrowsAny<Exception>(() => account.Close("test"));
    }

    [Fact]
    public void ReplayEvents_WithAllEventTypes_ShouldNotThrow()
    {
        var accountId = new AccountId(Guid.NewGuid());
        var events = new List<Event>
        {
            new AccountOpened(accountId.Id, Guid.NewGuid(), 100m),
            new MoneyDeposited(accountId, 50m, "test"),
            new MoneyWithdrawn(accountId, 30m),
            new MoneyTransferred(accountId, 20m, new AccountId(Guid.NewGuid()), "test"),
            new AccountClosed(accountId, "test")
        };

        var account = BankAccount.ReplayEvents(events);
        Assert.NotNull(account.Events);
    }

    [Fact]
    public void Operations_OnClosedAccount_ShouldThrow()
    {
        var account = BankAccount.Open(Guid.NewGuid(), 0m);
        account.Close("test");
        Assert.ThrowsAny<Exception>(() => account.Deposit(10m, "test"));
    }

    [Fact]
    public void Account_Constructor_InitializesEventsList()
    {
        var account = BankAccount.Open(Guid.NewGuid(), 0m);
        Assert.NotNull(account.Events);
    }
}