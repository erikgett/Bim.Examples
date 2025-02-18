// <copyright file="RevitRunner.cs" company="IT4BIM">
// Copyright (c) IT4BIM. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Diagnostics;

namespace Bim.Library.ProgramsRunner;

/// <summary> Run Revit exe with plugin to export nwc. </summary>
public class RevitRunner
{
    private readonly string pathToRevitExe;

    private readonly string argument;

    private Process revitProcess;

    /// <summary> Initializes a new instance of the <see cref="RevitRunner"/> class.</summary>
    /// <param name="argument">path to Revit journal.</param>
    /// <param name="revitVersion">path to Revit exe.</param>
    public RevitRunner(
        string argument,
        string revitVersion = "2021")
    {
        this.argument = argument;
        this.pathToRevitExe = $"C:\\Program Files\\Autodesk\\Revit {revitVersion}\\Revit.exe";
    }

    /// <summary> Finalizes an instance of the <see cref="RevitRunner"/> class. </summary>
    ~RevitRunner()
    {
        this.Kill();
    }

    /// <summary> Run Revit and plugin for export from rvt to nwc. </summary>
    public void Run()
    {
        this.revitProcess = new Process();
        this.revitProcess.StartInfo.FileName = this.pathToRevitExe;
        this.revitProcess.StartInfo.Arguments = this.argument;

        this.revitProcess.Start();
    }

    /// <summary> Kill instance off Revit. </summary>
    public void Kill()
    {
        try
        {
            this.revitProcess?.Kill();
        }
        catch
        {
            // do nothing.
        }
    }
}
