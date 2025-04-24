// <copyright file="IDataExporter.cs" company="IT4BIM">
// Copyright (c) IT4BIM. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using Autodesk.Revit.DB;

namespace Bim.Examples.DataExport;

public interface IDataExporter
{
    void CreateDatabase();

    void UpdateData(Document doc);

    void DeleteOldData(Document doc);
}
