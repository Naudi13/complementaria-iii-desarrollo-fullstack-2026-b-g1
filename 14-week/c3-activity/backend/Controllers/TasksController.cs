// Controllers/TasksController.cs
// Endpoints de tareas. [Authorize] a nivel de clase: todos exigen un JWT válido.
using Backend.Api.Data;
using Backend.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tasks")]
public class TasksController : ControllerBase
{
    private readonly TaskRepository _repository;

    public TasksController(TaskRepository repository) => _repository = repository;

    [HttpGet]
    public IActionResult GetAll() => Ok(new { ok = true, data = _repository.GetAll() });

    [HttpPost]
    public IActionResult Create([FromBody] CreateTaskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(new { ok = false, error = "El título es obligatorio" });
        }

        var task = _repository.Create(request.Title.Trim());
        return StatusCode(StatusCodes.Status201Created, new { ok = true, data = task });
    }

    [HttpPatch("{id:int}/toggle")]
    public IActionResult Toggle(int id)
    {
        var task = _repository.Toggle(id);
        return task is null
            ? NotFound(new { ok = false, error = "Tarea no encontrada" })
            : Ok(new { ok = true, data = task });
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id) =>
        _repository.Remove(id)
            ? Ok(new { ok = true, message = "Tarea eliminada" })
            : NotFound(new { ok = false, error = "Tarea no encontrada" });
}
