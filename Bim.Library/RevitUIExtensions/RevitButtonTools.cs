// <copyright file="RevitButtonTools.cs" company="IT4BIM">
// Copyright (c) IT4BIM. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Autodesk.Revit.UI;

namespace Bim.Library.RevitUIExtensions;

/// <summary> Provides helper methods for creating Revit ribbon buttons. </summary>
public static class RevitButtonTools
{
    /// <summary> Creates a new push button on the specified ribbon ribbonPanel. </summary>
    /// <typeparam name="TCommand">The command class type.</typeparam>
    /// <param name="ribbonPanel">The ribbon ribbonPanel to add the button to.</param>
    /// <param name="label">The label of the button.</param>
    /// <param name="tooltip">The tooltip of the button.</param>
    /// <param name="iconImageName">The name of the icon image resource.</param>
    /// <param name="tooltipImageName">The name of the tooltip image resource. (Optional).</param>
    /// <param name="helpURL">url to replay user by url link.</param>
    /// <param name="longDescription">longDescription for your button.</param>
    /// <param name="availabilityClass">The availability class type. (Optional).</param>
    /// <param name="smallIconImageName">The name of the small image resource. (Optional).</param>
    /// <returns>The created <see cref="PushButton"/>.</returns>
    public static PushButton CreateButton<TCommand>(
        this RibbonPanel ribbonPanel,
        string label,
        string tooltip,
        string iconImageName,
        string tooltipImageName = null,
        string helpURL = null,
        string longDescription = null,
        Type availabilityClass = null,
        string smallIconImageName = null)
        where TCommand : IExternalCommand
    {
        var btnData = CreatePushButtonData<TCommand>(
            label,
            tooltip,
            iconImageName,
            tooltipImageName,
            helpURL,
            longDescription,
            availabilityClass,
            smallIconImageName);

        return ribbonPanel.AddItem(btnData) as PushButton;
    }

    /// <summary> Creates a new push button data. </summary>
    /// <typeparam name="TCommand">The command class type.</typeparam>
    /// <param name="label">The label of the button.</param>
    /// <param name="tooltip">The tooltip of the button.</param>
    /// <param name="iconImageName">The name of the icon image resource.</param>
    /// <param name="tooltipImageName">The name of the tooltip image resource. (Optional).</param>
    /// <param name="helpURL">url to replay user by url link.</param>
    /// <param name="longDescription">longDescription for your button.</param>
    /// <param name="availabilityClass">The availability class type. (Optional).</param>
    /// <param name="smallIconImageName">The name of the small image resource. (Optional).</param>
    /// <returns>The created <see cref="PushButton"/>.</returns>
    public static PushButtonData CreatePushButtonData<TCommand>(
        string label,
        string tooltip,
        string iconImageName,
        string tooltipImageName = null,
        string helpURL = null,
        string longDescription = null,
        Type availabilityClass = null,
        string smallIconImageName = null)
        where TCommand : IExternalCommand
    {
        string assemblyName = typeof(TCommand).Assembly.Location;
        string commandNamespacePath = typeof(TCommand).FullName;

        longDescription ??= string.Empty;

        PushButtonData btnData = new (
            Guid.NewGuid().ToString(),
            label,
            assemblyName,
            commandNamespacePath)
        {
            ToolTip = tooltip,
            LongDescription = longDescription,
            LargeImage = GetIcon(iconImageName, typeof(TCommand).Assembly),
            ToolTipImage = GetIcon(tooltipImageName, typeof(TCommand).Assembly),
            AvailabilityClassName = availabilityClass?.FullName ?? string.Empty,
            Image = ConvertToSmallImage<TCommand>(iconImageName, smallIconImageName, typeof(TCommand).Assembly),
        };

        if (!string.IsNullOrEmpty(helpURL))
        {
            ContextualHelp contextualHelp = new(ContextualHelpType.Url, helpURL);
            btnData.SetContextualHelp(contextualHelp);
        }

        return btnData;
    }

    /// <summary> Adds a SplitButton to the Ribbon. </summary>
    /// <param name="ribbonPanel">The ribbon panel to add the button to.</param>
    /// <param name="internalName">Split button internal name.</param>
    /// <param name="buttonText">Split button display text.</param>
    /// <param name="buttonsData">Push buttons to add to the SplitButton (2 to 5 buttons).</param>
    /// <returns>The created SplitButton.</returns>
    public static SplitButton CreateSplitButtons(
        this RibbonPanel ribbonPanel,
        string internalName,
        string buttonText,
        params PushButtonData[] buttonsData)
    {
        if (buttonsData == null || buttonsData.Length < 2 || buttonsData.Length > 5)
        {
            throw new ArgumentException("SplitButton must contain between 2 and 5 buttons.", nameof(buttonsData));
        }

        var pushButtonData = new SplitButtonData(internalName, buttonText);
        SplitButton splitButton = (SplitButton)ribbonPanel.AddItem(pushButtonData);

        PushButton firstButton = null;

        foreach (PushButtonData data in buttonsData)
        {
            PushButton button = splitButton.AddPushButton(data);
            firstButton ??= button;
        }

        splitButton.CurrentButton = firstButton;
        return splitButton;
    }

    /// <summary> Adds two stacked buttons to the Ribbon. </summary>
    /// <param name="ribbonPanel">The ribbon panel to add the buttons to.</param>
    /// <param name="button1">First button data.</param>
    /// <param name="button2">Second button data.</param>
    /// <returns>The added Ribbon items. </returns>
    public static IList<RibbonItem> CreateStackedButtons(
        this RibbonPanel ribbonPanel,
        PushButtonData button1,
        PushButtonData button2)
    {
        return ribbonPanel.AddStackedItems(button1, button2);
    }

    /// <summary> Adds three stacked buttons to the Ribbon. </summary>
    /// <param name="ribbonPanel">The ribbon panel to add the buttons to.</param>
    /// <param name="button1">First button data.</param>
    /// <param name="button2">Second button data.</param>
    /// <param name="button3">Third button data.</param>
    /// <returns>The added Ribbon items. </returns>
    public static IList<RibbonItem> CreateStackedButtons(
        this RibbonPanel ribbonPanel,
        PushButtonData button1,
        PushButtonData button2,
        PushButtonData button3)
    {
        return ribbonPanel.AddStackedItems(button1, button2, button3);
    }

    /// <summary> Gets the icon image from the specified resource name. </summary>
    /// <param name="iconName">The name of the icon image resource.</param>
    /// <returns>A <see cref="BitmapImage"/> representing the icon.</returns>
    private static BitmapImage GetIcon(string iconName, Assembly assembly)
    {
        if (string.IsNullOrEmpty(iconName))
        {
            return null;
        }

        var uri = new Uri($"pack://application:,,,/{assembly.GetName().Name};component/Resources/{iconName}", UriKind.RelativeOrAbsolute);
        return new BitmapImage(uri);
    }

    private static BitmapImage ConvertToSmallImage<TCommand>(string iconImageName, string smallIconImageName, Assembly assembly)
        where TCommand : IExternalCommand
    {
        if (!string.IsNullOrEmpty(smallIconImageName))
        {
            return GetIcon(smallIconImageName, typeof(TCommand).Assembly);
        }

        BitmapImage smallImage = null;

        try
        {
            var bitmapImage = GetIcon(iconImageName, typeof(TCommand).Assembly);

            // create transformed image.
            var transformedBitmap = new TransformedBitmap(
                bitmapImage,
                new ScaleTransform(16.0 / bitmapImage.PixelWidth, 16.0 / bitmapImage.PixelHeight));

            // convert to bitmapimage.
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(transformedBitmap));

            var stream = new MemoryStream();
            encoder.Save(stream);
            stream.Position = 0;

            smallImage = new BitmapImage();
            smallImage.BeginInit();
            smallImage.CacheOption = BitmapCacheOption.OnLoad;
            smallImage.StreamSource = stream;
            smallImage.EndInit();
            smallImage.Freeze();
        }
        catch (Exception)
        { }

        return smallImage;
    }
}
