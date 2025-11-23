using AMS.ConsoleUI.Views.Offices;
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
            { 1, () => new MasterStudentListView(_serviceProvider) },
            { 2, () => new ProfessorListView(_serviceProvider) },
            { 3, () => new OfficeListView(_serviceProvider) },
            { 4, () => new SequenceListView(_serviceProvider) },
            { 5, () => new ReportsView(_serviceProvider) },
            { 6, () => new DataGeneratorView(_serviceProvider) }
        };

        var menuFrame = new FrameView("Menu")
        {
            X = 0, Y = 0, Width = 25, Height = Dim.Fill()
        };

        _menuList = new ListView(new List<string>
        {
            "Students",
            "Master Students",
            "Professors",
            "Offices",
            "Sequence Counters",
            "Reports & Analysis",
            "Data  Generator",
            "Exit"
        })
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        };

        _menuList.OpenSelectedItem += a => Navigate(a.Item);
        menuFrame.Add(_menuList);

        _contentFrame = new FrameView("Details")
        {
            X = 25, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        };

        Add(menuFrame, _contentFrame);
        Navigate(0);
    }

    private void Navigate(int index)
    {
        if (index == 7)
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