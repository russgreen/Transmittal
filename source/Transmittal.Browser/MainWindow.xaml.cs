using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Transmittal.Browser.Models;
using Transmittal.Browser.Services;
using Transmittal.Browser.ViewModels;

namespace Transmittal.Browser;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    private readonly IBrowserLaunchOptionsProvider _launchOptionsProvider;
    private readonly ILogger<MainWindow> _logger;
    private Point _dragStartPoint;

    public MainWindow(MainWindowViewModel viewModel,
        IBrowserLaunchOptionsProvider launchOptionsProvider,
        ILogger<MainWindow> logger)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _launchOptionsProvider = launchOptionsProvider;
        _logger = logger;
        DataContext = _viewModel;

        Loaded += MainWindowLoaded;
    }

    private async void MainWindowLoaded(object sender, RoutedEventArgs e)
    {
        var options = _launchOptionsProvider.GetLaunchOptions();
        Directory.CreateDirectory(options.UserDataDirectory);

        var webView2Environment = await CoreWebView2Environment.CreateAsync(
            browserExecutableFolder: null,
            userDataFolder: options.UserDataDirectory,
            options: new CoreWebView2EnvironmentOptions($"--remote-debugging-port={options.RemoteDebuggingPort}"));

        await WebView.EnsureCoreWebView2Async(webView2Environment);
        WebView.NavigationCompleted += WebViewOnNavigationCompleted;

        if (!Uri.TryCreate(options.StartUrl, UriKind.Absolute, out var startUri))
        {
            _logger.LogWarning("Invalid start url {StartUrl}, defaulting to about:blank", options.StartUrl);
            startUri = new Uri("about:blank");
        }

        WebView.Source = startUri;
        _viewModel.CurrentAddress = startUri.ToString();
    }

    private void TransferFilesListBox_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not ListBox listBox)
        {
            return;
        }

        _dragStartPoint = e.GetPosition(null);

        var listBoxItem = FindVisualParent<ListBoxItem>(e.OriginalSource as DependencyObject);
        if (listBoxItem?.DataContext is TransferFileItem fileItem &&
            listBox.SelectedItems.Count > 1 &&
            listBox.SelectedItems.Contains(fileItem))
        {
            // Preserve existing multi-selection when drag starts on one of selected items.
            e.Handled = true;
        }
    }

    private void TransferFilesListBox_OnPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        var currentPoint = e.GetPosition(null);
        if (Math.Abs(currentPoint.X - _dragStartPoint.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(currentPoint.Y - _dragStartPoint.Y) < SystemParameters.MinimumVerticalDragDistance)
        {
            return;
        }

        if (sender is not ListBox listBox)
        {
            return;
        }

        var existingFiles = new List<string>();
        foreach (var item in listBox.SelectedItems.OfType<TransferFileItem>())
        {
            if (item.Exists)
            {
                existingFiles.Add(item.FilePath);
            }
        }

        if (existingFiles.Count == 0)
        {
            return;
        }

        var dataObject = new DataObject(DataFormats.FileDrop, existingFiles.ToArray());
        DragDrop.DoDragDrop(listBox, dataObject, DragDropEffects.Copy);
    }

    private void RefreshButton_OnClick(object sender, RoutedEventArgs e)
    {
        WebView.CoreWebView2?.Reload();
    }

    private void GoButton_OnClick(object sender, RoutedEventArgs e)
    {
        NavigateToAddress(_viewModel.CurrentAddress);
    }

    private void AddressBarTextBox_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }

        NavigateToAddress(_viewModel.CurrentAddress);
        e.Handled = true;
    }

    private void NavigateToAddress(string addressText)
    {
        if (WebView.CoreWebView2 is null)
        {
            return;
        }

        var text = addressText?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        if (!text.Contains("://", StringComparison.Ordinal))
        {
            text = $"https://{text}";
        }

        if (!Uri.TryCreate(text, UriKind.Absolute, out var uri))
        {
            _logger.LogWarning("Invalid address entered: {Address}", addressText);
            return;
        }

        WebView.CoreWebView2.Navigate(uri.ToString());
    }

    private void WebViewOnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        _viewModel.CurrentAddress = WebView.Source?.ToString() ?? _viewModel.CurrentAddress;
    }

    private static T? FindVisualParent<T>(DependencyObject? child) where T : DependencyObject
    {
        while (child is not null)
        {
            if (child is T target)
            {
                return target;
            }

            child = VisualTreeHelper.GetParent(child);
        }

        return null;
    }
}
