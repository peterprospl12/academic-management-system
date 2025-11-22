// File: AMS.ConsoleUI/Views/MainView.cs

using AMS.ConsoleUI.Views.Professors;
using AMS.ConsoleUI.Views.Reports;
using AMS.ConsoleUI.Views.Students;
using AMS.ConsoleUI.Views.System;
using Terminal.Gui;

namespace AMS.ConsoleUI.Views;

public class MainView : Window
{
    private readonly FrameView _contentFrame;
    private readonly ListView _menuList;
    private readonly Dictionary<int, Func<View>> _navigationMap;
    private readonly IServiceProvider _serviceProvider;

    public MainView(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Title = "Academic Management System (AMS v1.0)";

        _navigationMap = new Dictionary<int, Func<View>>
        {
            { 0, () => new StudentListView(_serviceProvider) },
            { 1, () => new ProfessorListView(_serviceProvider) },
            { 2, () => new SequenceListView(_serviceProvider) },
            { 3, () => new ReportsView(_serviceProvider) }
        };

        var menuFrame = new FrameView("Menu")
        {
            X = 0, Y = 0, Width = 25, Height = Dim.Fill()
        };

        _menuList = new ListView(new List<string>
        {
            "Students",
            "Professors",
            "Sequence Counters",
            "Reports & Analysis", // 3
            "Exit" // 4
        })
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        };

        _menuList.OpenSelectedItem += a => Navigate(a.Item);
        menuFrame.Add(_menuList);

        // 2. Right Content Area
        _contentFrame = new FrameView("Details")
        {
            X = 25, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        };

        Add(menuFrame, _contentFrame);
        Navigate(0);
    }

    private void Navigate(int index)
    {
        if (index == 4) // Exit index adjusted
        {
            Terminal.Gui.Application.RequestStop();
            return;
        }

        _contentFrame.RemoveAll();

        if (_navigationMap.TryGetValue(index, out var createView))
        {
            var view = createView();
            _contentFrame.Add(view);
            view.SetFocus();
        }
    }
}