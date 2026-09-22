namespace ConsoleBot;

public class Program
{
    private const string StartEndpoint = "/start";
    private const string HelpEndpoint = "/help";
    private const string InfoEndpoint = "/info";
    private const string EchoEndpoint = "/echo";
    private const string AddTaskEndpoint = "/addtask";
    private const string ShowTasksEndpoint = "/showtasks";
    private const string RemoveTaskEndpoint = "/removetask";
    private const string ExitEndpoint = "/exit";

    private static List<string> _tasks = new List<string>();

    static void Main()
    {
        DateOnly creationDate = DateOnly.Parse("05.09.2026");

        Console.WriteLine($"Hello!\n" +
            $"Available commands to interact with the console bot: " +
            $"{StartEndpoint}, {HelpEndpoint}, {InfoEndpoint}, {ExitEndpoint}");

        string? userName = null;
        bool isNameTaken = false;

        while (true)
        {
            PrintMenu(isNameTaken, userName);

            var input = Console.ReadLine();
            string[] inputParams = [];

            if (!string.IsNullOrWhiteSpace(input))
            {
                var inputWithParams = input.Trim().Split(' ');
                input = inputWithParams[0];
                inputParams = inputWithParams.Skip(1).ToArray();
            }

            switch (input)
            {
                case StartEndpoint:
                    if (isNameTaken)
                    {
                        Console.WriteLine($"You are logged in as {userName}. Use one of the other commands.");
                    }
                    else
                    {
                        Console.Write("Enter your name: ");

                        var name = Console.ReadLine();

                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            userName = name.Trim();
                            isNameTaken = true;
                        }
                    }

                    break;
                case HelpEndpoint:
                    PrintHelpEndpointText(isNameTaken);
                    break;
                case InfoEndpoint:
                    Console.WriteLine("ConsoleBot 0.0.2\n" + $"Cteation Date: {creationDate}");
                    break;
                case EchoEndpoint:
                    PrintEchoEndpointText(isNameTaken, inputParams);
                    break;
                case AddTaskEndpoint:
                    AddTask();
                    break;
                case ShowTasksEndpoint:
                    ShowTasks();
                    break;
                case RemoveTaskEndpoint:
                    RemoveTask();
                    break;
                case ExitEndpoint:
                    return;
                default:
                    Console.WriteLine("You typed an incorrect command, try again.");
                    break;
            }
        }
    }

    private static void PrintMenu(bool isNameTaken, string userName)
    {
        string greating = !isNameTaken ? "Please" : $"{userName}, please";

        Console.WriteLine();
        Console.Write($"{greating} enter the command: ");
    }

    private static void PrintEchoEndpointText(bool isNameTaken, string[] inputParams)
    {
        if (!isNameTaken)
        {
            Console.WriteLine($"You are not logged in. Type your name using {StartEndpoint} command.");
            return;
        }

        if (inputParams.Length > 0)
        {
            foreach (var param in inputParams)
            {
                Console.Write(param + " ");
            }
        }

        Console.WriteLine();
    }

    private static void PrintHelpEndpointText(bool isNameTaken)
    {
        string echoEndpointText = string.Empty;
        string addTaskEndpointText = string.Empty;
        string showTasksEndpointText = string.Empty;
        string removeEndpointText = string.Empty;

        if (isNameTaken)
        {
            echoEndpointText = 
                $"{EchoEndpoint}\t\t - You make to print on screen some text if you type it after command using a space character for separation.\n";

            addTaskEndpointText =
                $"{AddTaskEndpoint}\t - Add new task to list.\n";

            showTasksEndpointText =
                $"{ShowTasksEndpoint}\t - Show all tasks in list.\n";

            removeEndpointText =
                $"{RemoveTaskEndpoint}\t - Remove task from the list by number.\n";
        }

        Console.WriteLine("To interact with the console bot you need to use following commands:\n" +
            $"{StartEndpoint}\t\t - Program entry point. Type your name to get more available commands.\n" +
            $"{HelpEndpoint}\t\t - Display this info.\n" +
            $"{InfoEndpoint}\t\t - Display program version and creation date.\n" +
            echoEndpointText +
            addTaskEndpointText +
            showTasksEndpointText +
            removeEndpointText +
            $"{ExitEndpoint}\t\t - Exit from program.");
    }

    private static void AddTask()
    {
        Console.Write("Введите описание задачи: ");

        var task = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(task))
        {
            Console.WriteLine($"Вы ввели некорректное описание задачи.");
            return;
        }

        _tasks.Add(task);

        Console.WriteLine($"Задача \"{task}\" добавлена.");
    }

    private static void ShowTasks()
    {
        if (_tasks.Count == 0)
        {
            Console.WriteLine("Список задач пуст.");
            return;
        }

        for (int i = 0; i < _tasks.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {_tasks[i]}");
        }
    }

    private static void RemoveTask()
    {
        if (_tasks.Count == 0)
        {
            Console.WriteLine("Список задач пуст.");
            return;
        }

        ShowTasks();

        Console.Write("Введите номер задачи для удаления: ");

        if (!int.TryParse(Console.ReadLine(), out int taskNumber)
            || (taskNumber < 1 || taskNumber > _tasks.Count))
        {
            Console.WriteLine($"Неверный номер задачи. Пожалуйста, введите корректное значение (1 - {_tasks.Count})");
            return;
        }
        else
        {
            var task = _tasks[taskNumber - 1];

            _tasks.Remove(task);
            Console.WriteLine($"Задача \"{task}\" удалена.");
        }
    }
}
