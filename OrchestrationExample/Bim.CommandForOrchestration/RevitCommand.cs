// <copyright file="RevitCommand.cs" company="IT4BIM">
// Copyright (c) IT4BIM. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Net.Http;
using System.Threading;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Newtonsoft.Json.Linq;

namespace Bim.CommandForOrchestration;

[Transaction(TransactionMode.Manual)]
public class RevitCommand : IExternalCommand
{
    private static readonly HttpClient client = new();

    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        ProcessQueue();
        return Result.Succeeded;
    }

    private void ProcessQueue()
    {
        while (true)
        {
            try
            {
                // request for take a file.
                HttpResponseMessage response = client.GetAsync("http://localhost:5140/RevitTask").Result;

                if (response.IsSuccessStatusCode)
                {
                    string json = response.Content.ReadAsStringAsync().Result;
                    JObject fileData = JObject.Parse(json);
                    string file = fileData["task"].ToString();

                    if (!string.IsNullOrEmpty(file))
                    {
                        this.ExecuteTask(file);
                    }
                }
                else
                {
                    // if query is empty wait some time.
                    Thread.Sleep(10000);
                }
            }
            catch (Exception ex)
            {
                // log your exception.
                Thread.Sleep(10000);
            }
        }
    }

    private void ExecuteTask(string task)
    {
        // your file - export ifc export, nwc, make needed check and unload to db and something else.
    }
}
