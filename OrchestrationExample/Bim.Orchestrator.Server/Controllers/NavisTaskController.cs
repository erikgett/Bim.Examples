using Bim.Library.ProgramsRunner;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace Bim.Orchestrator.Server.Controllers;

[Route("[controller]")]
[ApiController]
public class NavisTaskController : ControllerBase
{
    private static readonly ConcurrentQueue<string> filesQueue = new();
    private static readonly SemaphoreSlim semaphore = new(5);
    private static readonly Lock queueLock = new();
    private static bool isProcessing = false;

    [HttpPost]
    public IActionResult Post([FromBody] TaskRequest request)
    {
        if (string.IsNullOrEmpty(request.Value))
        {
            return BadRequest("Task value is required.");
        }

        filesQueue.Enqueue(request.Value);
        StartProcessing();

        return Ok(new { Message = "Task added", Task = request.Value });
    }

    private void StartProcessing()
    {
        lock (queueLock)
        {
            if (!isProcessing)
            {
                isProcessing = true;
                Task.Run(ProcessQueue);
            }
        }
    }

    private async Task ProcessQueue()
    {
        while (!filesQueue.IsEmpty)
        {
            if (filesQueue.TryDequeue(out string filePath))
            {
                await semaphore.WaitAsync();
                _ = Task.Run(() =>
                {
                    try
                    {
                        NavisRunner navisRunner = new ();
                        navisRunner.ExecuteCommand(filePath);
                        navisRunner.Kill();
                    }
                    catch
                    {
                        // log error)
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });
            }
            await Task.Delay(100);
        }

        lock (queueLock)
        {
            isProcessing = false;
        }
    }
}
