// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows;
using System.Windows.Controls;

namespace Transmittal.Desktop.Controls;
public class Hyperlink : Button
{
    /// <summary>
    /// Property for <see cref="NavigateUri"/>.
    /// </summary>
    public static readonly DependencyProperty NavigateUriProperty = DependencyProperty.Register(nameof(NavigateUri),
        typeof(string), typeof(Hyperlink), new PropertyMetadata(string.Empty));

    /// <summary>
    /// The URL (or application shortcut) to open.
    /// </summary>
    public string NavigateUri
    {
        get => GetValue(NavigateUriProperty) as string ?? string.Empty;
        set => SetValue(NavigateUriProperty, value);
    }

    /// <summary>
    /// Action triggered when the button is clicked.
    /// </summary>
    public Hyperlink() => Click += RequestNavigate;

    private void RequestNavigate(object sender, RoutedEventArgs eventArgs)
    {
        if (string.IsNullOrEmpty(NavigateUri))
            return;

        System.Diagnostics.ProcessStartInfo sInfo = new(new Uri(NavigateUri).AbsoluteUri)
        {
            UseShellExecute = true
        };

        System.Diagnostics.Process.Start(sInfo);
    }
}
