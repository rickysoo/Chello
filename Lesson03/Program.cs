// ============================================
// LESSON 3: Loops
// Upgraded Number Guessing Game
// ============================================

Random random = new Random();
int secretNumber = random.Next(1, 11);   // picks a random number 1 to 10
int attempts = 0;
bool hasWon = false;

Console.WriteLine("=== Number Guessing Game ===");
Console.WriteLine("I'm thinking of a number between 1 and 10.");
Console.WriteLine("Keep guessing until you get it!");
Console.WriteLine("");

// WHILE LOOP - keeps repeating as long as the player hasn't won
while (hasWon == false)
{
    Console.Write("Your guess: ");
    string input = Console.ReadLine();
    int guess = int.Parse(input);

    attempts = attempts + 1;   // count each guess

    if (guess == secretNumber)
    {
        hasWon = true;   // this stops the loop
        Console.WriteLine("🎉 Correct! You got it in " + attempts + " attempts!");
    }
    else if (guess < secretNumber)
    {
        Console.WriteLine("Too low! Try again.");
    }
    else
    {
        Console.WriteLine("Too high! Try again.");
    }
}

// Show a rating based on number of attempts
Console.WriteLine("");
if (attempts == 1)
{
    Console.WriteLine("⭐⭐⭐ Amazing! First try!");
}
else if (attempts <= 3)
{
    Console.WriteLine("⭐⭐ Great job! Very few guesses!");
}
else if (attempts <= 6)
{
    Console.WriteLine("⭐ Good effort!");
}
else
{
    Console.WriteLine("Keep practicing — you'll get faster!");
}

// ============================================
// BONUS: FOR LOOP - counts from 1 to 5
// ============================================
Console.WriteLine("");
Console.WriteLine("--- For Loop Example ---");

for (int i = 1; i <= 5; i++)
{
    Console.WriteLine("Count: " + i);
}

// ============================================
// BONUS 2: Loop through a list of names
// ============================================
Console.WriteLine("");
Console.WriteLine("--- Greeting Everyone ---");

string[] names = { "Alice", "Bob", "Charlie", "Ricky" };

foreach (string name in names)
{
    Console.WriteLine("Hello, " + name + "!");
}
