namespace ConsoleBot;

public class ToDoService : IToDoService
{
    private IReadOnlyList<ToDoItem> _toDoItems = [];
    private const int MaxTasksCount = 100;
    private const int MaxTaskLength = 100;

    public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
    {
        return _toDoItems;
    }

    //Возвращает ToDoItem для UserId со статусом Active
    public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
    {
        //???
        return (IReadOnlyList<ToDoItem>)_toDoItems.Select(item => item.State.Equals(ToDoItemState.Active));
    }

    public ToDoItem Add(ToDoUser user, string name)
    {
        if (_toDoItems.Count == MaxTasksCount)
            throw new ArgumentOutOfRangeException($"Превышено максимальное количество задач: {MaxTasksCount}");

        if (name!.Length > MaxTaskLength)
            throw new ArgumentException($"Превышено максимальное значение длины названия задачи: {MaxTaskLength}");

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
        foreach (var item in _toDoItems)
        {
            if (item.Id.Equals(id))
            {
                //item;
            }
        }
    }
}
