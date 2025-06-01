// <copyright file="RevitDocumentScopeLifeTimeService.cs" company="IT4BIM">
// Copyright (c) IT4BIM. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Bim.Library.DependencyInjection;

/// <summary> Class help override scope behaviors.</summary>
public static class RevitDocumentScopeLifeTimeService
{
    private static readonly ConcurrentDictionary<Document, IServiceProvider> DocumentServiceProviders = [];
    private static readonly Dictionary<Type, WeakReference<Window>> TransientWindowsCache = [];

    private static bool isFirstInitializing = true;

    private static IServiceCollection services;

    private static Document doc;
    private static UIApplication uiApplication;

    /// <summary> Add in di container current doc uiapp and ControlledApplication.</summary>
    /// <param name="services"> you host service collection.</param>
    /// <param name="application"><see cref="UIControlledApplication"/>.</param>
    public static void AddDocumentScopeLifeTimeSupport(
        this IServiceCollection services,
        UIControlledApplication application)
    {
        if (isFirstInitializing)
        {
            RevitDocumentScopeLifeTimeService.services = services;
            services.AddSingleton(application);
            services.AddScoped(sp => doc);
            services.AddSingleton(sp => uiApplication);
            application.ControlledApplication.DocumentClosing += DocumentClosingEvent;
            application.ViewActivated += ViewActivatedEvent;
            isFirstInitializing = false;
        }
    }

    /// <summary> Get service for current doc if value null.
    /// You can get service for specific doc.</summary>
    /// <typeparam name="T">type of service you need.</typeparam>
    /// <param name="host"><see cref="IHost"/>.</param>
    /// <param name="doc"><see cref="Document"/>.</param>
    /// <returns> service by T type.</returns>
    /// <exception cref="InvalidOperationException">return when service not registered.</exception>
    /// <exception cref="NotSupportedException">Unsupported lifetime.</exception>
    public static T GetService<T>(this IHost host, Document doc = null)
        where T : class
    {
        doc ??= RevitDocumentScopeLifeTimeService.doc;

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(T));
        if (descriptor == null)
        {
            throw new InvalidOperationException($"Service of type {typeof(T)} is not registered.");
        }

        return descriptor.Lifetime switch
        {
            ServiceLifetime.Singleton => host.Services.GetRequiredService<T>(),

            ServiceLifetime.Scoped => GetScopedService<T>(doc),

            ServiceLifetime.Transient => GetOrCreateTransient<T>(host),

            _ => throw new NotSupportedException($"Unsupported lifetime: {descriptor.Lifetime}")
        };
    }

    /// <summary> Get service for current doc if value null.
    /// You can get service for specific doc.</summary>
    /// <param name="host"><see cref="IHost"/>.</param>
    /// <param name="type"><see cref="Type"/>Type of service you need..</param>
    /// <param name="doc"><see cref="Document"/>.</param>
    /// <returns> service by T type.</returns>
    /// <exception cref="InvalidOperationException">return when service not registered.</exception>
    /// <exception cref="NotSupportedException">Unsupported lifetime.</exception>
    public static object GetService(this IHost host, Type type, Document doc = null)
    {
        doc ??= RevitDocumentScopeLifeTimeService.doc;

        var descriptor = services.FirstOrDefault(d => d.ServiceType == type);
        if (descriptor == null)
        {
            throw new InvalidOperationException($"Service of type {type} is not registered.");
        }

        return descriptor.Lifetime switch
        {
            ServiceLifetime.Singleton => host.Services.GetRequiredService(type),

            ServiceLifetime.Scoped => GetScopedService(type, doc),

            ServiceLifetime.Transient => ActivatorUtilities.CreateInstance(host.Services, type),

            _ => throw new NotSupportedException($"Unsupported lifetime: {descriptor.Lifetime}")
        };
    }

    private static T GetOrCreateTransient<T>(IHost host)
        where T : class
    {
        var type = typeof(T);

        if (typeof(Window).IsAssignableFrom(type))
        {
            if (TransientWindowsCache.TryGetValue(type, out var weakRef)
                && weakRef.TryGetTarget(out var existingWindow))
            {
                if (!existingWindow.IsLoaded && !existingWindow.IsVisible)
                {
                    TransientWindowsCache.Remove(type);
                }
                else
                {
                    return (T)(object)existingWindow;
                }
            }

            return host.Services.GetRequiredService<T>();
        }
        else
        {
            return host.Services.GetRequiredService<T>();
        }
    }

    private static T GetScopedService<T>(Document doc)
        where T : class
    {
        if (!DocumentServiceProviders.TryGetValue(doc, out var provider))
        {
            var serviceCollection = new ServiceCollection();
            foreach (var descriptor in services)
            {
                serviceCollection.Add(descriptor);
            }

            provider = serviceCollection.BuildServiceProvider();
            DocumentServiceProviders[doc] = provider;
        }

        return provider.GetRequiredService<T>();
    }

    private static object GetScopedService(Type type, Document doc)
    {
        if (!DocumentServiceProviders.TryGetValue(doc, out var provider))
        {
            var serviceCollection = new ServiceCollection();
            foreach (var descriptor in services)
            {
                serviceCollection.Add(descriptor);
            }

            provider = serviceCollection.BuildServiceProvider();
            DocumentServiceProviders[doc] = provider;
        }

        return provider.GetRequiredService(type);
    }

    private static void DocumentClosingEvent(object sender, DocumentClosingEventArgs e)
    {
        DocumentServiceProviders.TryRemove(e.Document, out var t);
    }

    private static void ViewActivatedEvent(object sender, ViewActivatedEventArgs e)
    {
        doc = e.Document;
        uiApplication = sender as UIApplication;
    }
}
