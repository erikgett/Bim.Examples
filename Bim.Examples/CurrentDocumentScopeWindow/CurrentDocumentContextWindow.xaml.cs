// <copyright file="CurrentDocumentContextWindow.xaml.cs" company="IT4BIM">
// Copyright (c) IT4BIM. All rights reserved.
// Licensed under the NC license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Windows;

namespace Bim.Examples.CurrentDocumentScopeWindow;

public partial class CurrentDocumentContextWindow : Window
{
    /// <summary> Initializes a new instance of the <see cref="CurrentDocumentContextWindow"/> class. </summary>
    public CurrentDocumentContextWindow(DocumentInfo documentInfo)
    {
        this.DataContext = documentInfo;
        this.InitializeComponent();
    }
}
