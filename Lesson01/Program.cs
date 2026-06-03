// ============================================
// LESSON 1: Hello World & Variables
// ============================================

// 1. Print text to the screen
Console.WriteLine("Hello, World!");
Console.WriteLine("Welcome to C# programming!");

// 2. Variables - containers that store information
string name = "Ricky";         // stores text
int age = 30;                  // stores a whole number
double height = 1.75;          // stores a decimal number
bool isLearning = true;        // stores true or false

// 3. Display the variables
Console.WriteLine("-----------------------------");
Console.WriteLine("Name: " + name);
Console.WriteLine("Age: " + age);
Console.WriteLine("Height: " + height + "m");
Console.WriteLine("Learning C#: " + isLearning);

// 4. Simple math with numbers
int a = 10;
int b = 3;
Console.WriteLine("-----------------------------");
Console.WriteLine("10 + 3 = " + (a + b));
Console.WriteLine("10 - 3 = " + (a - b));
Console.WriteLine("10 x 3 = " + (a * b));
Console.WriteLine("10 / 3 = " + (a / b));        // whole number division
Console.WriteLine("10 mod 3 = " + (a % b));       // remainder

// 5. Ask the user for input
Console.WriteLine("-----------------------------");
Console.Write("What is your name? ");             // no new line at end
string userName = Console.ReadLine();             // waits for user to type
Console.WriteLine("Nice to meet you, " + userName + "!");
