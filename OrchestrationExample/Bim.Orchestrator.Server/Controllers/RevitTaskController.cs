using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace Bim.Orchestrator.Server.Controllers;

[Route("[controller]")]
[ApiController]
public class RevitTaskController : ControllerBase
{
    private static readonly ConcurrentQueue<string> filesQueue = new();

    [HttpGet]
    public IActionResult Get()
    {
        if (filesQueue.TryDequeue(out var task))
        {
            return Ok(new { Message = "Task retrieved", Task = task });
        }
        return NotFound(new { Message = "Queue is empty" });
    }


    [HttpPost]
    public IActionResult Post([FromBody] TaskRequest request)
    {
        if (string.IsNullOrEmpty(request.Value))
        {
            return BadRequest("Task value is required.");
        }

        filesQueue.Enqueue(request.Value);
        return Ok(new { Message = "Task added", Task = request.Value });
    }
}
public class TaskRequest
{
    public string Value { get; set; }
}

