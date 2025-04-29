using Domain.Aggregates.BankAccount.Events;
using Domain.Aggregates.Customer;

namespace Domain.Aggregates.BankAccount
{
	public class BankAccount
	{
		public AccountId AccountId { get; private set; }
		public List<Event> Events { get; private set; }
		public CustomerId AccountHolder { get; private set; }
		public Balance Balance { get; private set; }
		public string Currency { get; private set; }
		public bool IsActive { get; private set; }

		private BankAccount()
		{
			Events = [];
		}

		public static BankAccount Open(Guid accountHolder, decimal initialDeposit)
		{
			if (initialDeposit < 0)
			{
				throw new ArgumentException("Deposit can't be negative");
			}

			var bankAccount = new BankAccount();

			var @event = new AccountOpened(Guid.CreateVersion7(), accountHolder, initialDeposit);

			bankAccount.Apply(@event);

			return bankAccount;
		}

		public void Deposit(decimal amount, string description)
		{
			AccountIsActive();
			if (amount <= 0)
			{
				throw new ArgumentException("Deposit must be positive");
			}

			Apply(new MoneyDeposited(AccountId, amount, description));
		}

		public void TransferTo(AccountId toAccountId, decimal amount, string description)
		{
			AccountIsActive();
			if (amount <= 0)
			{
				throw new ArgumentException("Transfer must be positive");
			}

			if (Balance.Amount < amount)
			{
				throw new InvalidOperationException("Not enough balance");
			}

			Apply(new MoneyTransferred(AccountId, amount, toAccountId, description));
		}

		public void Close(string reason)
		{
			AccountIsActive();
			if (Balance.Amount != 0)
			{
				throw new InvalidOperationException("Cannot close with a non zero balance");
			}

			Apply(new AccountClosed(AccountId, reason));
		}

		private void Apply(Event @event)
		{
			switch (@event)
			{
				case AccountOpened e:
					AccountId = new AccountId(e.AccountId);
					AccountHolder = new CustomerId(e.AccountHolder);
					Balance = new Balance(e.InitialDeposit);
					Currency = e.Currency;
					IsActive = true;
					break;

				case MoneyDeposited e:
					Balance = new Balance(Balance.Amount + e.Amount);
					break;

				case MoneyWithdrawn e:
					Balance = new Balance(Balance.Amount - e.Amount);
					break;
				case MoneyTransferred e:
					Balance = new Balance(Balance.Amount - e.Amount);
					break;

				case AccountClosed e:
					IsActive = false;
					break;
			}

			Events.Add(@event);
		}

		public static BankAccount ReplayEvents(IEnumerable<Event> events)
		{
			var bankAccount = new BankAccount();

			foreach (var @event in events)
			{
				bankAccount.Apply(@event);
				Console.WriteLine($"Account: {bankAccount.AccountId.Id} Event: {@event.GetType().Name}, New Balance: ${bankAccount.Balance}");
			}

			return bankAccount;
		}

		private void AccountIsActive()
		{
			if (!IsActive)
			{
				throw new InvalidOperationException("Account is not active");
			}
		}
	}
}