// <copyright file="DataExportApplication.cs" company="IT4BIM">
// Copyright (c) IT4BIM. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using Autodesk.Revit.UI;
using Bim.Examples.RevitCommands;
using Bim.Library.RevitUIExtensions;

namespace Bim.Examples;

public class DataExportApplication
    : IExternalApplication
{
    /// <inheritdoc/>
    public Result OnStartup(UIControlledApplication application)
    {
        application.CreateRibbonTab("IT4BIM");
        DataExportPanel(application);

        return Result.Succeeded;
    }

    public RibbonPanel DataExportPanel(UIControlledApplication application)
    {
        RibbonPanel diExampplePanel = application.GetOrCreateRibbonPanel("DataExport");

        diExampplePanel.CreateButton<ExportToDatabase>(
            "exportData",
            "di example for export data",
            "ExportToDatabase.png");

        return diExampplePanel;
    }

    public Result OnShutdown(UIControlledApplication application)
        => Result.Succeeded;
}