// ============================================
// LESSON 4: Methods
// A simple calculator using methods
// ============================================

// --- MAIN PROGRAM STARTS HERE ---

Console.WriteLine("=== Calculator ===");
Console.WriteLine("");

// Call methods to do the work
PrintWelcome();

double num1 = AskForNumber("Enter first number: ");
double num2 = AskForNumber("Enter second number: ");

double sum        = Add(num1, num2);
double difference = Subtract(num1, num2);
double product    = Multiply(num1, num2);

Console.WriteLine("");
Console.WriteLine("--- Results ---");
Console.WriteLine(num1 + " + " + num2 + " = " + sum);
Console.WriteLine(num1 + " - " + num2 + " = " + difference);
Console.WriteLine(num1 + " x " + num2 + " = " + product);

// Division is special - can't divide by zero!
if (num2 != 0)
{
    double quotient = Divide(num1, num2);
    Console.WriteLine(num1 + " / " + num2 + " = " + quotient);
}
else
{
    Console.WriteLine("Cannot divide by zero!");
}

Console.WriteLine("");
PrintGoodbye();


// --- METHODS DEFINED BELOW ---

// Method with no input, no output — just prints something
void PrintWelcome()
{
    Console.WriteLine("Welcome! Let's do some math.");
    Console.WriteLine("-------------------------------");
}

// Method with no input, no output
void PrintGoodbye()
{
    Console.WriteLine("Thanks for using the calculator!");
}

// Method that takes input and returns a number
double AskForNumber(string question)
{
    Console.Write(question);
    string input = Console.ReadLine();
    double number = double.Parse(input);
    return number;
}

// Methods that take two numbers and return the result
double Add(double a, double b)
{
    return a + b;
}

double Subtract(double a, double b)
{
    return a - b;
}

double Multiply(double a, double b)
{
    return a * b;
}

double Divide(double a, double b)
{
    return a / b;
}
