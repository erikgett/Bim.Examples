// <copyright file="MainUIApplication.cs" company="IT4BIM">
// Copyright (c) IT4BIM. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using Autodesk.Revit.UI;

namespace Bim.Examples;

public class MainUIApplication
    : IExternalApplication
{
    /// <inheritdoc/>
    public Result OnStartup(UIControlledApplication application)
    {
        application.CreateRibbonTab("IT4BIM");

        new RoomFinishingApplication().RoomFinishingPanel(application);
        new HoleTaskApplication().HoleTaskPanel(application);
        new DataExportApplication().DataExportPanel(application);
        return Result.Succeeded;
    }

    public Result OnShutdown(UIControlledApplication application)
        => Result.Succeeded;
}