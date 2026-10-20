namespace ConsoleBot;

public class ToDoService(int maxTasksCount, int maxTaskLength) : IToDoService
{
    private readonly IReadOnlyList<ToDoItem> _toDoItems = [];
    private readonly int _maxTasksCount = maxTasksCount;
    private readonly int _maxTaskLength = maxTaskLength;

    public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
    {
        IReadOnlyList<ToDoItem> activeItems = [];

        foreach (var item in _toDoItems)
            if (item.User.UserId.Equals(userId))
                _ = activeItems.Append(item);

        return activeItems;
    }

    //Возвращает ToDoItem для UserId со статусом Active
    public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
    {
        IReadOnlyList<ToDoItem> activeItems = [];

        foreach (var item in _toDoItems)
            if (item.User.UserId.Equals(userId) && item.State.Equals(ToDoItemState.Active))
                _ = activeItems.Append(item);

        return activeItems;
    }

    public ToDoItem Add(ToDoUser user, string name)
    {
        if (_toDoItems.Count == _maxTasksCount)
            throw new ArgumentOutOfRangeException($"Превышено максимальное количество задач: {_maxTasksCount}");

        if (name!.Length > _maxTaskLength)
            throw new ArgumentException($"Превышено максимальное значение длины названия задачи: {_maxTaskLength}");

        foreach (var task in _toDoItems)
        {
            if (task.Name.Equals(name))
                throw new ArgumentException($"Задача с таким именем уже существует: {task}");
        }

        var newItem = new ToDoItem(user, name);

        _ = _toDoItems.Append(newItem);

        return newItem;
    }

    public void MarkCompleted(Guid id)
    {
        foreach (var item in _toDoItems)
        {
            if (item.Id.Equals(id))
            {
                item.State = ToDoItemState.Completed;
                item.StateChangedAt = DateTime.UtcNow;
            }
        }
    }

    public void Delete(Guid id)
    {
        if (_toDoItems is List<ToDoItem> mutableList)
        {
            foreach (var item in mutableList)
                if (item.Id.Equals(id))
                    mutableList.Remove(item);
        }
    }
}
