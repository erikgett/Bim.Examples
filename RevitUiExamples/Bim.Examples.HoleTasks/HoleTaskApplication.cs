// <copyright file="HoleTaskApplication.cs" company="IT4BIM">
// Copyright (c) IT4BIM. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using Autodesk.Revit.UI;
using Bim.Examples.RevitCommands;
using Bim.Library.RevitUIExtensions;

namespace Bim.Examples;

public class HoleTaskApplication
    : IExternalApplication
{
    /// <inheritdoc/>
    public Result OnStartup(UIControlledApplication application)
    {
        application.CreateRibbonTab("IT4BIM");
        HoleTaskPanel(application);

        return Result.Succeeded;
    }

    public RibbonPanel HoleTaskPanel(UIControlledApplication application)
    {
        RibbonPanel diExampplePanel = application.GetOrCreateRibbonPanel("HoleTasks");

        diExampplePanel.CreateButton<RevitCommands.HoleTaskApplication>(
            "exportData",
            "di example for export data",
            "HoleTaskApplication.png");

        return diExampplePanel;
    }

    public Result OnShutdown(UIControlledApplication application)
        => Result.Succeeded;
}