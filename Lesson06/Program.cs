// ============================================
// LESSON 6: Classes & Objects
// Main Program - uses the BankAccount class
// ============================================

// Create two bank accounts (two OBJECTS from one CLASS)
BankAccount rickyAccount = new BankAccount("Ricky", "ACC-001", 1000);
BankAccount aliceAccount = new BankAccount("Alice", "ACC-002", 500);

bool running = true;
BankAccount current = rickyAccount;   // start with Ricky's account

Console.WriteLine("=== Bank App ===");

while (running)
{
    Console.WriteLine("");
    Console.WriteLine("Active Account: " + current.OwnerName);
    Console.WriteLine("  1 - View summary");
    Console.WriteLine("  2 - Deposit");
    Console.WriteLine("  3 - Withdraw");
    Console.WriteLine("  4 - View history");
    Console.WriteLine("  5 - Switch account");
    Console.WriteLine("  6 - Quit");
    Console.Write("Your choice: ");

    string choice = Console.ReadLine();

    if (choice == "1")
    {
        current.PrintSummary();
    }
    else if (choice == "2")
    {
        Console.Write("Deposit amount: $");
        double amount = double.Parse(Console.ReadLine());
        current.Deposit(amount);
    }
    else if (choice == "3")
    {
        Console.Write("Withdraw amount: $");
        double amount = double.Parse(Console.ReadLine());
        current.Withdraw(amount);
    }
    else if (choice == "4")
    {
        current.PrintHistory();
    }
    else if (choice == "5")
    {
        if (current == rickyAccount)
        {
            current = aliceAccount;
        }
        else
        {
            current = rickyAccount;
        }
        Console.WriteLine("✅ Switched to " + current.OwnerName + "'s account.");
    }
    else if (choice == "6")
    {
        running = false;
        Console.WriteLine("Goodbye! 👋");
    }
    else
    {
        Console.WriteLine("❌ Invalid choice.");
    }
}
