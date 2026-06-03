// ============================================
// LESSON 6: Classes & Objects
// BankAccount Class - the blueprint
// ============================================

class BankAccount
{
    // Properties - data that belongs to this account
    public string OwnerName { get; private set; }
    public string AccountNumber { get; private set; }
    public double Balance { get; private set; }
    public List<string> Transactions { get; private set; }

    // Constructor - runs when you create a new account
    public BankAccount(string ownerName, string accountNumber, double openingBalance)
    {
        OwnerName = ownerName;
        AccountNumber = accountNumber;
        Balance = openingBalance;
        Transactions = new List<string>();
        Transactions.Add("Account opened with $" + openingBalance);
    }

    // Method - deposit money
    public void Deposit(double amount)
    {
        if (amount <= 0)
        {
            Console.WriteLine("❌ Deposit amount must be greater than zero.");
            return;
        }

        Balance = Balance + amount;
        Transactions.Add("Deposited $" + amount + " | Balance: $" + Balance);
        Console.WriteLine("✅ Deposited $" + amount + " successfully.");
    }

    // Method - withdraw money
    public void Withdraw(double amount)
    {
        if (amount <= 0)
        {
            Console.WriteLine("❌ Withdrawal amount must be greater than zero.");
            return;
        }

        if (amount > Balance)
        {
            Console.WriteLine("❌ Insufficient funds! Your balance is $" + Balance);
            return;
        }

        Balance = Balance - amount;
        Transactions.Add("Withdrew  $" + amount + " | Balance: $" + Balance);
        Console.WriteLine("✅ Withdrew $" + amount + " successfully.");
    }

    // Method - show account summary
    public void PrintSummary()
    {
        Console.WriteLine("------------------------------");
        Console.WriteLine("👤 Owner:   " + OwnerName);
        Console.WriteLine("🔢 Account: " + AccountNumber);
        Console.WriteLine("💰 Balance: $" + Balance);
        Console.WriteLine("------------------------------");
    }

    // Method - show transaction history
    public void PrintHistory()
    {
        Console.WriteLine("📜 Transaction History:");
        Console.WriteLine("------------------------------");
        for (int i = 0; i < Transactions.Count; i++)
        {
            Console.WriteLine("  " + (i + 1) + ". " + Transactions[i]);
        }
        Console.WriteLine("------------------------------");
    }
}
