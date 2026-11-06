// Models/TaskItem.cs
// Modelo de una tarea. Solo describe la forma de los datos.
namespace Backend.Api.Models;

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool Done { get; set; }
}
