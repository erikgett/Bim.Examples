// <copyright file="DocumentInfo.cs" company="IT4BIM">
// Copyright (c) IT4BIM. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using Autodesk.Revit.DB;

namespace Bim.Examples.CurrentDocumentScopeWindow;

public class DocumentInfo(Document doc)
{
    public string Title { get; set; } = doc.Title;

    public string PathName { get; set; } = doc.PathName;
}
