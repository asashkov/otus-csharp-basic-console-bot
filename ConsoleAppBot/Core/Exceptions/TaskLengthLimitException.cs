namespace ConsoleBot.Core.Exceptions;

public class TaskLengthLimitException(int taskLength, int taskLengthLimit) : Exception()
{
    int TaskLengthValue { get; } = taskLength;
    int TaskLengthLimitValue { get; } = taskLengthLimit;

    public override string Message => $"Длина задачи {TaskLengthValue} превышает максимально допустимое значение {TaskLengthLimitValue}.";
}
