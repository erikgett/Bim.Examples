// <copyright file="RevitApplication.cs" company="IT4BIM">
// Copyright (c) IT4BIM. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Bim.Examples;

public class RevitApplication
    : IExternalApplication
{
    /// <inheritdoc/>
    public Result OnStartup(UIControlledApplication application)
    {
        int a = 4;
        int b = 5;
        int c = a + b;
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
