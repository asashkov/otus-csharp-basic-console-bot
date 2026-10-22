namespace ConsoleBot;

public class DuplicateTaskException(string task) : Exception()
{
    string TaskValue { get; } = task;

    public override string Message => $"Задача '{TaskValue}' уже существует.";
}
