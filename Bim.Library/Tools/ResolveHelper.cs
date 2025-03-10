// <copyright file="ResolveHelper.cs" company="IT4BIM">
// Copyright (c) IT4BIM. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.IO;
using System.Reflection;

namespace Bim.Library.Tools;

public static class ResolveHelper
{
    private static string moduleDirectory;
    private static object domainResolvers;

    /// <summary>
    ///     Subscribes the current domain to resolve dependencies for the type.
    /// </summary>
    /// <typeparam name="T">Type, to search for dependencies in the directory where this type is defined.</typeparam>
    /// <remarks>
    ///     Dependencies are searched in a directory of the specified type.
    ///     At the time of dependency resolution, all other dependency resolution methods for the domain are disabled,
    ///     this requires calling <see cref="EndAssemblyResolve" /> immediately after executing user code where dependency failures occur.
    /// </remarks>
    public static void BeginAssemblyResolve<T>()
    {
        BeginAssemblyResolve(typeof(T));
    }

    /// <summary>
    ///     Subscribes the current domain to resolve dependencies for the type.
    /// </summary>
    /// <param name="type">Type, to search for dependencies in the directory where this type is defined.</param>
    /// <remarks>
    ///     Dependencies are searched in a directory of the specified type.
    ///     At the time of dependency resolution, all other dependency resolution methods for the domain are disabled,
    ///     this requires calling <see cref="EndAssemblyResolve" /> immediately after executing user code where dependency failures occur.
    /// </remarks>
    public static void BeginAssemblyResolve(Type type)
    {
        if (domainResolvers is not null)
        {
            return;
        }

        if (type.Module.FullyQualifiedName == "<Unknown>")
        {
            return;
        }

        var domainType = AppDomain.CurrentDomain.GetType();
        var resolversField = domainType.GetField("_AssemblyResolve", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)!;
        var resolvers = resolversField.GetValue(AppDomain.CurrentDomain);
        resolversField.SetValue(AppDomain.CurrentDomain, null);

        domainResolvers = resolvers;
        moduleDirectory = Path.GetDirectoryName(type.Module.FullyQualifiedName);

        AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
    }

    /// <summary>
    ///     Unsubscribes the current domain to resolve dependencies for the type.
    /// </summary>
    public static void EndAssemblyResolve()
    {
        if (domainResolvers is null)
        {
            return;
        }

        var domainType = AppDomain.CurrentDomain.GetType();
        var resolversField = domainType.GetField("_AssemblyResolve", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)!;
        resolversField.SetValue(AppDomain.CurrentDomain, domainResolvers);

        domainResolvers = null;
        moduleDirectory = null;
    }

    private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
    {
        var assemblyName = new AssemblyName(args.Name).Name;
        var assemblyPath = Path.Combine(moduleDirectory!, $"{assemblyName}.dll");
        if (!File.Exists(assemblyPath))
        {
            return null;
        }

        return Assembly.LoadFrom(assemblyPath);
    }
}
