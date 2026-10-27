using ConsoleBot.Core.DataAccess;
using ConsoleBot.Core.Entities;

namespace ConsoleBot.Infrastructure.DataAccess;

internal class InMemoryToDoRepository : IToDoRepository
{
    private readonly List<ToDoItem> _itemList = [];
    
    public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
    {
        return _itemList.Where(item => item.User.UserId.Equals(userId)).ToList();
    }

    //Возвращает ToDoItem для UserId со статусом Active
    public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
    {
        return _itemList.Where(item => item.User.UserId.Equals(userId)&& item.State.Equals(ToDoItemState.Active)).ToList();
    }

    //Возвращает ToDoItem для UserId со статусом Completed
    public IReadOnlyList<ToDoItem> GetCompletedByUserId(Guid userId)
    {
        return _itemList.Where(item => item.User.UserId.Equals(userId) && item.State.Equals(ToDoItemState.Completed)).ToList();
    }

    public ToDoItem? Get(Guid id)
    {
        return _itemList.SingleOrDefault(item => item.Id.Equals(id));
    }

    public void Add(ToDoItem item)
    {
        _itemList.Add(item);
    }

    public void Update(ToDoItem item)
    {
        var itemIndex = _itemList.FindIndex(listItem => listItem.Id.Equals(item.Id));

        if (itemIndex != -1)
            _itemList[itemIndex] = item;
    }

    public void Delete(Guid id)
    {
        _itemList.RemoveAll(item => item.Id.Equals(id));
    }

    //Проверяет есть ли задача с таким именем у пользователя
    public bool ExistsByName(Guid userId, string name)
    {
        return _itemList.Exists(item => item.User.UserId.Equals(userId) && item.Name.Equals(name));
    }

    //Возвращает количество активных задач у пользователя
    public int CountActive(Guid userId)
    {
        return GetActiveByUserId(userId).Count;
    }

    public IReadOnlyList<ToDoItem> Find(Guid userId, Func<ToDoItem, bool> predicate)
    {
        return GetAllByUserId(userId).Where(predicate).ToList();
    }
}
