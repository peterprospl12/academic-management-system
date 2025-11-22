using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui;

namespace AMS.ConsoleUI.Views;

public class MainView : Window
{
    private readonly FrameView _contentFrame;
    private readonly IServiceProvider _serviceProvider;

    public MainView(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Title = "Academic Management System (AMS v1.0)";

        var menuFrame = new FrameView("Menu")
        {
            X = 0, Y = 0,
            Width = 25, Height = Dim.Fill()
        };

        var menuList = new ListView(new List<string>
        {
            "Lista Studentów",
            "Dodaj Studenta",
            "Lista Profesorów",
            "Raporty i Analizy",
            "Wyjście"
        })
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        };

        menuList.OpenSelectedItem += a => Navigate(a.Item);

        menuFrame.Add(menuList);

        _contentFrame = new FrameView("Szczegóły")
        {
            X = 25, Y = 0,
            Width = Dim.Fill(), Height = Dim.Fill()
        };

        Add(menuFrame, _contentFrame);
    }

    private void Navigate(int index)
    {
        _contentFrame.RemoveAll();

        switch (index)
        {
            case 0: ShowStudentList(); break;
            case 1: ShowAddStudentDialog(); break;
            case 2: ShowProfessorList(); break;
            case 3: ShowReports(); break;
            case 4: Terminal.Gui.Application.RequestStop(); break;
        }
    }

    private void ShowStudentList()
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IStudentService>();

        var result = service.GetAllStudentsAsync(CancellationToken.None).GetAwaiter().GetResult();

        if (result.IsFailure)
        {
            MessageBox.ErrorQuery("Błąd", result.Error, "Ok");
            return;
        }

        var studentStrings = result.Value
            .Select(s => $"{s.UniversityIndex} - {s.FirstName} {s.LastName}")
            .ToList();

        var list = new ListView(studentStrings)
        {
            Width = Dim.Fill(), Height = Dim.Fill()
        };

        list.KeyPress += e =>
        {
            if (e.KeyEvent.Key == Key.DeleteChar)
            {
                var selectedIndex = list.SelectedItem;
                if (selectedIndex < 0 || selectedIndex >= result.Value.Count) return;

                var studentToDelete = result.Value[selectedIndex];

                var confirm = MessageBox.Query("Usuwanie", $"Czy usunąć {studentToDelete.FirstName}?", "Tak", "Nie");
                if (confirm == 0)
                {
                    service.DeleteStudentAsync(studentToDelete.Id, CancellationToken.None).GetAwaiter().GetResult();
                    ShowStudentList();
                }

                e.Handled = true;
            }
        };

        _contentFrame.Add(list);
        list.SetFocus();
        Terminal.Gui.Application.Refresh();
    }

    private void ShowAddStudentDialog()
    {
        var nameLabel = new Label("Imię:") { X = 1, Y = 1 };
        var nameText = new TextField("") { X = 15, Y = 1, Width = 20 };

        var surnameLabel = new Label("Nazwisko:") { X = 1, Y = 3 };
        var surnameText = new TextField("") { X = 15, Y = 3, Width = 20 };

        var btnSave = new Button("Zapisz") { X = 15, Y = 6 };

        btnSave.Clicked += () =>
        {
            var firstName = nameText.Text.ToString() ?? string.Empty;
            var lastName = surnameText.Text.ToString() ?? string.Empty;

            var dto = new CreateStudentDto(
                firstName,
                lastName,
                "Ulica 1", "Miasto", "00-000", 1);

            using var scope = _serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IStudentService>();

            var res = service.CreateStudentAsync(dto, CancellationToken.None).GetAwaiter().GetResult();

            if (res.IsSuccess)
            {
                MessageBox.Query("Sukces", "Dodano studenta!", "Ok");
                Navigate(0);
            }
            else
            {
                MessageBox.ErrorQuery("Błąd", res.Error, "Ok");
            }
        };

        _contentFrame.Add(nameLabel, nameText, surnameLabel, surnameText, btnSave);
    }

    private void ShowProfessorList()
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IProfessorService>();

        var result = service.GetAllProfessorsAsync(CancellationToken.None).GetAwaiter().GetResult();

        if (result.IsFailure) return;

        var profStrings = result.Value
            .Select(p => $"{p.Title.ToString()} {p.FirstName} {p.LastName} ({p.UniversityIndex})")
            .ToList();

        var list = new ListView(profStrings)
        {
            Width = Dim.Fill(), Height = Dim.Fill()
        };
        _contentFrame.Add(list);
    }

    private void ShowReports()
    {
        var btnHardest = new Button("Najtrudniejszy Plan") { X = 1, Y = 1 };
        var resultLabel = new Label("Wynik pojawi się tutaj...") { X = 1, Y = 4, Width = Dim.Fill(), Height = 10 };

        btnHardest.Clicked += () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var reportService = scope.ServiceProvider.GetRequiredService<IReportsService>();

            var res = reportService.GetStudentWithHardestPlanAsync(CancellationToken.None).GetAwaiter().GetResult();

            if (res.IsSuccess)
                resultLabel.Text = $"Student: {res.Value.FullName}\n" +
                                   $"Indeks: {res.Value.IndexNumber}\n" +
                                   $"ECTS (Kursy): {res.Value.CurrentEcts}\n" +
                                   $"ECTS (Wymogi): {res.Value.PrerequisitesEcts}\n" +
                                   $"SUMA (Trudność): {res.Value.TotalDifficultyScore}";
            else
                resultLabel.Text = "Błąd: " + res.Error;
        };

        _contentFrame.Add(btnHardest, resultLabel);
    }
}