// Data/TaskRepository.cs
// "Base de datos" en memoria (mismo enfoque que la primera versión).
// Se registra como Singleton para que los datos vivan mientras corre la app.
using Backend.Api.Models;

namespace Backend.Api.Data;

public class TaskRepository
{
    private readonly object _lock = new();
    private readonly List<TaskItem> _tasks = new()
    {
        new TaskItem { Id = 1, Title = "Aprender HTML semántico", Done = true },
        new TaskItem { Id = 2, Title = "Consumir una API protegida con JWT", Done = false },
        new TaskItem { Id = 3, Title = "Probar el backend y el frontend", Done = false },
    };
    private int _nextId = 4;

    public IReadOnlyList<TaskItem> GetAll()
    {
        lock (_lock) return _tasks.ToList();
    }

    public TaskItem Create(string title)
    {
        lock (_lock)
        {
            var task = new TaskItem { Id = _nextId++, Title = title };
            _tasks.Add(task);
            return task;
        }
    }

    public bool Remove(int id)
    {
        lock (_lock)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            return task is not null && _tasks.Remove(task);
        }
    }

    public TaskItem? Toggle(int id)
    {
        lock (_lock)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            if (task is null) return null;
            task.Done = !task.Done;
            return task;
        }
    }
}
