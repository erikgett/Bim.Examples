using Autodesk.Navisworks.Api.Plugins;
using System.Diagnostics;

namespace Bim.CommandForNavisOrchestration;

/// <summary> Simplified Navisworks command for executing tasks in a server environment. </summary>
[Plugin(
    "Bim.CommandForNavisOrchestration.NavisCommand",
    "FirstCommand",
    ToolTip = "NavisCommand - BIM Execution",
    DisplayName = "NavisCommand")]
public class NavisCommand : AddInPlugin
{
    /// <summary> Executes the command process. </summary>
    /// <param name="parameters">Optional parameters for customization.</param>
    /// <returns>0 if execution succeeds, non-zero for errors.</returns>
    public override int Execute(params string[] parameters)
    {
        try
        {
            // if you will use other nuget packages
            // you will need manually load dll or write assembly resolver.
            // Perform the required operation
            ExecuteTask();

            return 0;
        }
        catch
        {
            return 1;
        }
    }

    /// <summary>
    /// Executes a specific task based on provided parameters.
    /// </summary>
    private void ExecuteTask()
    {
        // Task execution logic goes here
        Process.Start("notepad.exe");
    }
}
