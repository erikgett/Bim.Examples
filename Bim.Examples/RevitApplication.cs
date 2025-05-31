// <copyright file="RevitApplication.cs" company="IT4BIM">
// Copyright (c) IT4BIM. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Bim.Examples.RevitCommands;
using Bim.Library.RevitUIExtensions;
using Bim.Library.Tools;

namespace Bim.Examples;

public class RevitApplication
    : IExternalApplication
{
    /// <inheritdoc/>
    public Result OnStartup(UIControlledApplication application)
    {
        ResolveHelper.BeginAssemblyResolve<RevitApplication>();
        Host.Start(application);

        application.CreateRibbonTab("IT4BIM");

        RibbonPanel diExampplePanel = application.GetOrCreateRibbonPanel("DI_Example");

        diExampplePanel.CreateButton<ExportToDatabase>(
            "exportData",
            "di example for export data",
            "ExportToDatabase.png");

        ResolveHelper.EndAssemblyResolve();

        return Result.Succeeded;
    }

    public Result OnShutdown(UIControlledApplication application)
        => Result.Succeeded;
}

public static class Extensions
{
    public static long GetValue(this ElementId elementId)
    {
        #if R17 || R18 || R19 || R20 || R21 || R22 || R23 || R24
        return elementId.IntegerValue;
        #else
        return elementId.Value;
        #endif
    }
}
