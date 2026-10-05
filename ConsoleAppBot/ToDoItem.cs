namespace ConsoleBot;

internal class ToDoItem
{
    public Guid Id { get; init; }

    public ToDoUser User { get; private set; }

    public string Name { get; private set; }

    private DateTime CreatedAt { get; set; }

    public ToDoItemState State { get; set; }

    public DateTime? StateChangedAt { get; set; }

    public ToDoItem(ToDoUser user, string name)
    {
        User = user;
        Name = name;
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        State = ToDoItemState.Active;
    }

    public override string ToString()
    {
        return $"{Name} - {CreatedAt} - {Id}";
    }
}
