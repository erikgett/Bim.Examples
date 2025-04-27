// <copyright file="RoomFinishingApplication.cs" company="IT4BIM">
// Copyright (c) IT4BIM. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using Autodesk.Revit.UI;
using Bim.Examples.RevitCommands;
using Bim.Library.RevitUIExtensions;

namespace Bim.Examples;

public class RoomFinishingApplication
    : IExternalApplication
{
    /// <inheritdoc/>
    public Result OnStartup(UIControlledApplication application)
    {
        application.CreateRibbonTab("IT4BIM");
        _ = RoomFinishingPanel(application);

        return Result.Succeeded;
    }

    public RibbonPanel RoomFinishingPanel(UIControlledApplication application)
    {
        RibbonPanel roomFinishingPanel = application.GetOrCreateRibbonPanel("RoomFinishing");

        roomFinishingPanel.CreateButton<FloorFinishing>(
            "Create floors",
            "create floors by rooms perimeter",
            "FloorFinishing.png");

        return roomFinishingPanel;
    }

    public Result OnShutdown(UIControlledApplication application)
        => Result.Succeeded;
}
