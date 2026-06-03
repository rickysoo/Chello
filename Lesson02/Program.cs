// ============================================
// LESSON 2: If/Else Decisions
// Number Guessing Game
// ============================================

// The secret number the player must guess
int secretNumber = 7;

Console.WriteLine("=== Number Guessing Game ===");
Console.WriteLine("I'm thinking of a number between 1 and 10.");
Console.Write("Your guess: ");

// Read the player's guess and convert it from text to a number
string input = Console.ReadLine();
int guess = int.Parse(input);

// Make a decision based on the guess
if (guess == secretNumber)
{
    Console.WriteLine("🎉 Correct! You guessed it!");
}
else if (guess < secretNumber)
{
    Console.WriteLine("Too low! The answer was " + secretNumber);
}
else if (guess > secretNumber)
{
    Console.WriteLine("Too high! The answer was " + secretNumber);
}

// ============================================
// BONUS: Check if a number is even or odd
// ============================================
Console.WriteLine("----------------------------");
Console.Write("Enter any number to check even/odd: ");
string input2 = Console.ReadLine();
int number = int.Parse(input2);

if (number % 2 == 0)
{
    Console.WriteLine(number + " is EVEN");
}
else
{
    Console.WriteLine(number + " is ODD");
}

// ============================================
// BONUS 2: Grade checker
// ============================================
Console.WriteLine("----------------------------");
Console.Write("Enter your test score (0-100): ");
string input3 = Console.ReadLine();
int score = int.Parse(input3);

if (score >= 90)
{
    Console.WriteLine("Grade: A - Excellent!");
}
else if (score >= 80)
{
    Console.WriteLine("Grade: B - Good job!");
}
else if (score >= 70)
{
    Console.WriteLine("Grade: C - Not bad!");
}
else if (score >= 60)
{
    Console.WriteLine("Grade: D - Need improvement");
}
else
{
    Console.WriteLine("Grade: F - Please study harder!");
}
