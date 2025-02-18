// <copyright file="NavisRunner.cs" company="IT4BIM">
// Copyright (c) IT4BIM. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using Autodesk.Navisworks.Api.Automation;

namespace Bim.Library.ProgramsRunner;

/// <summary> Help run instance off NavisWorks and help to execute script to export nwf file.</summary>
public class NavisRunner
{
    private readonly NavisworksApplication navisApp;

    /// <summary>
    /// Initializes a new instance of the <see cref="NavisRunner"/> class.
    /// Start NavisWorks application and save it on variable.
    /// </summary>
    public NavisRunner()
    {
        this.navisApp = new NavisworksApplication
        {
            Visible = false,
        };
    }

    /// <summary> Using started app off Navisworks and run plugin for export report. </summary>
    /// <param name="filePath">absolute path to nwf file.</param>
    /// <param name="commandName">Relative path to command on navis ribbon panel.</param>
    public void GenerateCollisionReport(string filePath, string commandName = "NavisReportCreater.ClashReport.Report")
    {
        try
        {
            // open selected file.
            this.navisApp.OpenFile(filePath);

            // export to xml.
            this.navisApp.ExecuteAddInPlugin(commandName);
        }
        catch
        {
            // do nothing.
        }
    }

    /// <summary> Kill NavisWorks app instance from task manager. </summary>
    public void Kill()
    {
        try
        {
            this.navisApp.Dispose();
        }
        catch
        {
            // do nothing.
        }
    }
}
