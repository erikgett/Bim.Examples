// <copyright file="RevitPanelTools.cs" company="IT4BIM">
// Copyright (c) IT4BIM. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Linq;
using Autodesk.Revit.UI;

namespace Bim.Library.RevitUIExtensions;

/// <summary> Provides utility methods for managing Revit ribbon panels. </summary>
public static class RevitPanelTools
{
    /// <summary>
    /// Retrieves an existing ribbon panel or creates a new one if it does not exist.
    /// </summary>
    /// <param name="application">The UIControlledApplication instance.</param>
    /// <param name="panelName">The name of the ribbon panel.</param>
    /// <param name="tabName">The name of the ribbon tab (default is IT4BIM).</param>
    /// <returns>The existing or newly created RibbonPanel.</returns>
    public static RibbonPanel GetOrCreateRibbonPanel(this UIControlledApplication application, string panelName, string tabName = "IT4BIM") =>
        application.GetRibbonPanels(tabName).FirstOrDefault(x => x.Name == panelName) ?? application.CreateRibbonPanel(tabName, panelName);
}
