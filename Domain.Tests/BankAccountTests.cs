using Domain.Aggregates.BankAccount;
using Domain.Aggregates.BankAccount.Events;

namespace Domain.Tests
{
	public class BankAccountTests
	{
		[Fact]
		public void OpenAccount_WithValidDeposit_CreatesAccount()
		{
			var accountHolder = Guid.NewGuid();
			var initialDeposit = 100m;

			var account = BankAccount.Open(accountHolder, initialDeposit);

			Assert.NotNull(account);
			Assert.Equal(initialDeposit, account.Balance.Amount);
			Assert.True(account.IsActive);
			Assert.Equal("EUR", account.Currency);
		}

		[Fact]
		public void OpenAccount_WithNegativeDeposit_ThrowsException()
		{
			var accountHolder = Guid.NewGuid();
			var initialDeposit = -100m;

			var exception = Assert.Throws<ArgumentException>(() => BankAccount.Open(accountHolder, initialDeposit));
			Assert.Equal("Deposit can't be negative", exception.Message);
		}

		[Fact]
		public void Deposit_ValidAmount_IncreasesBalance()
		{
			var account = BankAccount.Open(Guid.NewGuid(), 100m);
			var depositAmount = 50m;

			account.Deposit(depositAmount, "Test deposit");

			Assert.Equal(150m, account.Balance.Amount);
		}

		[Fact]
		public void Deposit_ZeroAmount_ThrowsException()
		{
			var account = BankAccount.Open(Guid.NewGuid(), 100m);

			var exception = Assert.Throws<ArgumentException>(() => account.Deposit(0m, "Invalid deposit"));
			Assert.Equal("Deposit must be positive", exception.Message);
		}

		[Fact]
		public void TransferTo_ValidAmount_DecreasesBalance()
		{
			var account = BankAccount.Open(Guid.NewGuid(), 100m);
			var targetAccountId = new AccountId(Guid.CreateVersion7());

			account.TransferTo(targetAccountId, 50m, "Test transfer");

			Assert.Equal(50m, account.Balance.Amount);
		}

		[Fact]
		public void TransferTo_InsufficientFunds_ThrowsException()
		{
			var account = BankAccount.Open(Guid.NewGuid(), 50m);
			var targetAccountId = new AccountId(Guid.CreateVersion7());

			var exception = Assert.Throws<InvalidOperationException>(() => account.TransferTo(targetAccountId, 100m, "Invalid transfer"));
			Assert.Equal("Not enough balance", exception.Message);
		}

		[Fact]
		public void Close_WithZeroBalance_SetsInactive()
		{
			var account = BankAccount.Open(Guid.NewGuid(), 0m);

			account.Close("Test closure");

			Assert.False(account.IsActive);
		}

		[Fact]
		public void Close_WithNonZeroBalance_ThrowsException()
		{
			var account = BankAccount.Open(Guid.NewGuid(), 100m);

			var exception = Assert.Throws<InvalidOperationException>(() => account.Close("Invalid closure"));
			Assert.Equal("Cannot close with a non zero balance", exception.Message);
		}

		[Fact]
		public void ReplayEvents_WithMultipleEvents_RebuildsAccountState()
		{
			var accountId = Guid.NewGuid();
			var accountHolder = Guid.NewGuid();
			var events = new List<Event>
		{
			new AccountOpened(accountId, accountHolder, 100m),
			new MoneyDeposited(new AccountId(accountId), 50m, "First deposit"),
			new MoneyWithdrawn(new AccountId(accountId), 30m),
			new MoneyTransferred(new AccountId(accountId), 20m, new AccountId(Guid.NewGuid()), "Transfer out")
		};

			var account = BankAccount.ReplayEvents(events);

			Assert.Equal(100m, account.Balance.Amount); // Initial deposit
			Assert.True(account.IsActive);
			Assert.Equal(4, account.Events.Count);
		}

		[Fact]
		public void Deposit_OnInactiveAccount_ThrowsException()
		{
			var account = BankAccount.Open(Guid.NewGuid(), 0m); // Start with zero balance to allow closing
			account.Close("Test closure");

			var exception = Assert.Throws<InvalidOperationException>(() => account.Deposit(50m, "Deposit to closed account"));
			Assert.Equal("Account is not active", exception.Message);
		}

		[Fact]
		public void Transfer_OnInactiveAccount_ThrowsException()
		{
			var account = BankAccount.Open(Guid.NewGuid(), 0m); // Start with zero balance to allow closing
			account.Close("Test closure");
			var targetAccountId = new AccountId(Guid.NewGuid());

			var exception = Assert.Throws<InvalidOperationException>(() => account.TransferTo(targetAccountId, 50m, "Transfer from closed account"));
			Assert.Equal("Account is not active", exception.Message);
		}

		[Fact]
		public void Transfer_WithZeroAmount_ThrowsException()
		{
			var account = BankAccount.Open(Guid.NewGuid(), 100m);
			var targetAccountId = new AccountId(Guid.NewGuid());

			var exception = Assert.Throws<ArgumentException>(() => account.TransferTo(targetAccountId, 0m, "Invalid transfer"));
			Assert.Equal("Transfer must be positive", exception.Message);
		}

		[Fact]
		public void Close_OnInactiveAccount_ThrowsException()
		{
			var account = BankAccount.Open(Guid.NewGuid(), 0m);
			account.Close("First closure");

			var exception = Assert.Throws<InvalidOperationException>(() => account.Close("Second closure"));
			Assert.Equal("Account is not active", exception.Message);
		}

		[Fact]
		public void ReplayEvents_WithEmptyEventList_ReturnsNewAccount()
		{
			var account = BankAccount.ReplayEvents(new List<Event>());

			Assert.NotNull(account);
			Assert.Empty(account.Events);
		}
	}
}