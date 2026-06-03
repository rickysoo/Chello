// ============================================
// LESSON 5: Lists & Arrays
// To-Do List App
// ============================================

List<string> todos = new List<string>();
bool running = true;

Console.WriteLine("=== To-Do List App ===");

while (running)
{
    Console.WriteLine("");
    Console.WriteLine("What do you want to do?");
    Console.WriteLine("  1 - View all tasks");
    Console.WriteLine("  2 - Add a task");
    Console.WriteLine("  3 - Remove a task");
    Console.WriteLine("  4 - Clear all tasks");
    Console.WriteLine("  5 - Quit");
    Console.Write("Your choice: ");

    string choice = Console.ReadLine();

    if (choice == "1")
    {
        ViewTasks(todos);
    }
    else if (choice == "2")
    {
        AddTask(todos);
    }
    else if (choice == "3")
    {
        RemoveTask(todos);
    }
    else if (choice == "4")
    {
        todos.Clear();
        Console.WriteLine("✅ All tasks cleared!");
    }
    else if (choice == "5")
    {
        running = false;
        Console.WriteLine("Goodbye! Stay productive! 👋");
    }
    else
    {
        Console.WriteLine("❌ Invalid choice. Please enter 1 to 5.");
    }
}


// --- METHODS ---

void ViewTasks(List<string> list)
{
    Console.WriteLine("");
    if (list.Count == 0)
    {
        Console.WriteLine("📋 No tasks yet. Add something!");
        return;
    }

    Console.WriteLine("📋 Your Tasks (" + list.Count + " total):");
    Console.WriteLine("------------------------------");

    for (int i = 0; i < list.Count; i++)
    {
        Console.WriteLine("  " + (i + 1) + ". " + list[i]);
    }

    Console.WriteLine("------------------------------");
}

void AddTask(List<string> list)
{
    Console.Write("Enter task name: ");
    string task = Console.ReadLine();

    if (task == "")
    {
        Console.WriteLine("❌ Task cannot be empty!");
        return;
    }

    list.Add(task);
    Console.WriteLine("✅ Task added: " + task);
}

void RemoveTask(List<string> list)
{
    if (list.Count == 0)
    {
        Console.WriteLine("❌ No tasks to remove!");
        return;
    }

    ViewTasks(list);
    Console.Write("Enter task number to remove: ");
    string input = Console.ReadLine();
    int number = int.Parse(input);

    if (number < 1 || number > list.Count)
    {
        Console.WriteLine("❌ Invalid number!");
        return;
    }

    string removed = list[number - 1];
    list.RemoveAt(number - 1);
    Console.WriteLine("✅ Removed: " + removed);
}
