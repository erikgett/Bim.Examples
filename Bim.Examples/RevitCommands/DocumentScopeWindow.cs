// <copyright file="DocumentScopeWindow.cs" company="IT4BIM">
// Copyright (c) IT4BIM. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Bim.Examples.CurrentDocumentScopeWindow;

namespace Bim.Examples.RevitCommands;

[Transaction(TransactionMode.Manual)]
public class DocumentScopeWindow : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        Host.GetService<CurrentDocumentContextWindow>()
            .Show();

        return Result.Succeeded;
    }
}
