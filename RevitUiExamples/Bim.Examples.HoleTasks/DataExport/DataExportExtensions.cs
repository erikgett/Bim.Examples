// <copyright file="DataExportExtensions.cs" company="IT4BIM">
// Copyright (c) IT4BIM. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using Autodesk.Revit.DB;

namespace Bim.Examples.DataExport;

public static class DataExportExtensions
{
    public static void ExportDataToDb(
        this IDataExporter exporter,
        Document doc)
    {
        exporter.CreateDatabase();
        exporter.UpdateData(doc);
        exporter.DeleteOldData(doc);
    }
}
