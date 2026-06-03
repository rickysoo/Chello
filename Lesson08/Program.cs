// ============================================
// LESSON 8: File Read & Write
// To-Do List that saves to a file
// ============================================

string filePath = "todos.txt";   // file saved in same folder as program
List<string> todos = new List<string>();
bool running = true;

// Load existing tasks from file when app starts
LoadFromFile();

Console.WriteLine("=== To-Do List (with File Save) ===");
Console.WriteLine("Your tasks are saved automatically!");

while (running)
{
    Console.WriteLine("");
    Console.WriteLine("Tasks loaded: " + todos.Count);
    Console.WriteLine("  1 - View all tasks");
    Console.WriteLine("  2 - Add a task");
    Console.WriteLine("  3 - Remove a task");
    Console.WriteLine("  4 - Clear all tasks");
    Console.WriteLine("  5 - Quit");
    Console.Write("Your choice: ");

    string choice = Console.ReadLine();

    if (choice == "1")
    {
        ViewTasks();
    }
    else if (choice == "2")
    {
        AddTask();
    }
    else if (choice == "3")
    {
        RemoveTask();
    }
    else if (choice == "4")
    {
        ClearTasks();
    }
    else if (choice == "5")
    {
        running = false;
        Console.WriteLine("Goodbye! Your tasks are saved. 👋");
    }
    else
    {
        Console.WriteLine("❌ Invalid choice. Enter 1 to 5.");
    }
}


// --- FILE METHODS ---

void LoadFromFile()
{
    try
    {
        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);
            todos.AddRange(lines);
            Console.WriteLine("✅ Loaded " + todos.Count + " tasks from file.");
        }
        else
        {
            Console.WriteLine("📋 No saved tasks found. Starting fresh!");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Could not load file: " + ex.Message);
    }
}

void SaveToFile()
{
    try
    {
        File.WriteAllLines(filePath, todos);
        Console.WriteLine("💾 Tasks saved to file.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Could not save file: " + ex.Message);
    }
}


// --- TASK METHODS ---

void ViewTasks()
{
    Console.WriteLine("");
    if (todos.Count == 0)
    {
        Console.WriteLine("📋 No tasks yet. Add something!");
        return;
    }

    Console.WriteLine("📋 Your Tasks (" + todos.Count + " total):");
    Console.WriteLine("------------------------------");
    for (int i = 0; i < todos.Count; i++)
    {
        Console.WriteLine("  " + (i + 1) + ". " + todos[i]);
    }
    Console.WriteLine("------------------------------");
}

void AddTask()
{
    Console.Write("Enter task name: ");
    string task = Console.ReadLine();

    if (task == "")
    {
        Console.WriteLine("❌ Task cannot be empty!");
        return;
    }

    todos.Add(task);
    SaveToFile();
    Console.WriteLine("✅ Task added: " + task);
}

void RemoveTask()
{
    if (todos.Count == 0)
    {
        Console.WriteLine("❌ No tasks to remove!");
        return;
    }

    ViewTasks();
    Console.Write("Enter task number to remove: ");

    try
    {
        int number = int.Parse(Console.ReadLine());

        if (number < 1 || number > todos.Count)
        {
            Console.WriteLine("❌ Invalid number!");
            return;
        }

        string removed = todos[number - 1];
        todos.RemoveAt(number - 1);
        SaveToFile();
        Console.WriteLine("✅ Removed: " + removed);
    }
    catch (FormatException)
    {
        Console.WriteLine("❌ Please enter a valid number.");
    }
}

void ClearTasks()
{
    Console.Write("Are you sure? This will delete all tasks. (yes/no): ");
    string confirm = Console.ReadLine();

    if (confirm == "yes")
    {
        todos.Clear();
        SaveToFile();
        Console.WriteLine("✅ All tasks cleared and file updated.");
    }
    else
    {
        Console.WriteLine("Cancelled.");
    }
}
