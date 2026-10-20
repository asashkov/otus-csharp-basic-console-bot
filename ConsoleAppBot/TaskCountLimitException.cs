namespace ConsoleBot;

public class TaskCountLimitException(int taskCountLimit) : Exception()
{
    int TaskCountLimitValue { get; } = taskCountLimit;

    public override string Message => $"Превышено максимальное количество задач равное {TaskCountLimitValue}.";
}
