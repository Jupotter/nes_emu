using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Microsoft.CodeAnalysis;
using NesEmu.UI.ViewModels;

namespace NesEmu.UI.Views;

public partial class CpuView : UserControl
{
    public CpuView()
    {
        InitializeComponent();

        KeyDownEvent.AddClassHandler<TopLevel>(InputElement_OnKeyDown, handledEventsToo: true);
    }

    private void InputElement_OnKeyDown(object? sender, KeyEventArgs e)
    {
        var vm = DataContext as CpuViewModel;
        vm?.HandleKey(e);
    }
}

