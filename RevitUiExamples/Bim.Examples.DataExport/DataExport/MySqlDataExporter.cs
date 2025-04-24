// <copyright file="MySqlDataExporter.cs" company="IT4BIM">
// Copyright (c) IT4BIM. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using Autodesk.Revit.DB;
using Microsoft.Extensions.Logging;

namespace Bim.Examples.DataExport;

public class MySqlDataExporter(ILogger<MySqlDataExporter> logger)
    : IDataExporter
{
    public void CreateDatabase()
    {
        logger.LogInformation("MySQL: Database creation started.");
    }

    public void UpdateData(Document doc)
    {
        logger.LogInformation($"MySQL: Data update started. for file - {doc.Title}");
    }

    public void DeleteOldData(Document doc)
    {
        logger.LogInformation($"MySQL: Old data deletion started. from file {doc.Title}");
    }
}
