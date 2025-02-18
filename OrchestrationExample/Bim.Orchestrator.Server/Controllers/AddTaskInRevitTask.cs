using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace Bim.Orchestrator.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AddTaskInRevitTask : ControllerBase
{
    private static readonly ConcurrentQueue<string> filesQueue = new();

    // GET: api/AddTaskInRevitTask
    [HttpGet]
    public IEnumerable<string> Get() => [.. filesQueue];

    // POST api/AddTaskInRevitTask
    [HttpPost]
    public IActionResult Post([FromBody] string value)
    {
        filesQueue.Enqueue(value);
        return Ok(new { Message = "Task added", Task = value });
    }

    // DELETE api/AddTaskInRevitTask
    [HttpDelete]
    public IActionResult Delete()
    {
        if (filesQueue.TryDequeue(out var task))
        {
            return Ok(new { Message = "Task removed", Task = task });
        }
        return NotFound("Queue is empty");
    }
}

