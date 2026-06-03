// ============================================
// LESSON 7: Error Handling
// A calculator that NEVER crashes
// ============================================

bool running = true;

Console.WriteLine("=== Safe Calculator ===");
Console.WriteLine("This calculator handles all mistakes gracefully.");

while (running)
{
    Console.WriteLine("");
    Console.WriteLine("  1 - Calculate");
    Console.WriteLine("  2 - Quit");
    Console.Write("Your choice: ");

    string choice = Console.ReadLine();

    if (choice == "1")
    {
        Calculate();
    }
    else if (choice == "2")
    {
        running = false;
        Console.WriteLine("Goodbye! 👋");
    }
    else
    {
        Console.WriteLine("❌ Please enter 1 or 2.");
    }
}


// --- METHODS ---

void Calculate()
{
    double num1 = AskForNumber("Enter first number:  ");
    double num2 = AskForNumber("Enter second number: ");

    Console.WriteLine("");
    Console.WriteLine("Choose operation:");
    Console.WriteLine("  + - * /");
    Console.Write("Your choice: ");
    string op = Console.ReadLine();

    try
    {
        double result = DoMath(num1, num2, op);
        Console.WriteLine("✅ Result: " + num1 + " " + op + " " + num2 + " = " + result);
    }
    catch (DivideByZeroException)
    {
        Console.WriteLine("❌ Error: Cannot divide by zero!");
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine("❌ Error: " + ex.Message);
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Something went wrong: " + ex.Message);
    }
    finally
    {
        Console.WriteLine("--- Calculation attempt complete ---");
    }
}

double AskForNumber(string question)
{
    while (true)   // keep asking until valid number entered
    {
        Console.Write(question + " ");

        try
        {
            string input = Console.ReadLine();
            double number = double.Parse(input);
            return number;
        }
        catch (FormatException)
        {
            Console.WriteLine("  ⚠️  That's not a valid number. Try again.");
        }
    }
}

double DoMath(double a, double b, string op)
{
    if (op == "+") return a + b;
    if (op == "-") return a - b;
    if (op == "*") return a * b;
    if (op == "/")
    {
        if (b == 0)
        {
            throw new DivideByZeroException();
        }
        return a / b;
    }

    throw new ArgumentException("Unknown operator: " + op + ". Use +, -, * or /");
}
