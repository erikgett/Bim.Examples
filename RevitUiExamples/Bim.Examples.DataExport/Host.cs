// <copyright file="Host.cs" company="IT4BIM">
// Copyright (c) IT4BIM. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Bim.Examples.DataExport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.OpenTelemetry;

namespace Bim.Examples;

/// <summary> Provides a host for the application's services and manages their lifetimes. </summary>
public static class Host
{
    private static IHost host;

    /// <summary> Starts the host and configures the application's services. </summary>
    public static void Start()
    {
        var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(
            new HostApplicationBuilderSettings
                {
                    ContentRootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    DisableDefaults = true,
                });

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.OpenTelemetry(x =>
            {
                x.Endpoint = "http://localhost:5341/ingest/otlp/v1/logs";
                x.Protocol = OtlpProtocol.HttpProtobuf;
                x.Headers = new Dictionary<string, string>
                {
                    ["X-Seq-ApiKey"] = "r8prOJJXAcMu53FfY8VA",
                };
            }).CreateLogger();

        builder.Services.AddSerilog();

        builder.Services.AddScoped<IDataExporter, PostgresDataExporter>();

        host = builder.Build();
        host.Start();
    }

    /// <summary> Stops the host and handle <see cref="IHostedService"/> services. </summary>
    public static void Stop()
    {
        host.StopAsync().GetAwaiter().GetResult();
    }

    /// <summary> Get service of type <typeparamref name="T"/>. </summary>
    /// <typeparam name="T">The type of service object to get</typeparam>
    /// <exception cref="System.InvalidOperationException">There is no service of type <typeparamref name="T"/>.</exception>
    /// <returns>instance of service.</returns>
    public static T GetService<T>()
        where T : class
    {
        return host.Services.GetRequiredService<T>();
    }
}
