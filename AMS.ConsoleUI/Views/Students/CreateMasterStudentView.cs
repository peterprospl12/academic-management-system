using AMS.Application.Common.Models;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.ConsoleUI.Helpers;
using AMS.ConsoleUI.Views.Base;
using Terminal.Gui;

namespace AMS.ConsoleUI.Views.Students;

public class CreateMasterStudentView : BaseView
{
    private readonly Action _onSuccess;
    private List<ProfessorDto> _availableProfessors;

    public CreateMasterStudentView(IServiceProvider serviceProvider, Action onSuccess) : base(serviceProvider)
    {
        _onSuccess = onSuccess;
        LoadProfessors();
        SetupUi();
    }

    private void LoadProfessors()
    {
        var result = ExecuteServiceFunc<IProfessorService, Result<List<ProfessorDto>>>(s =>
            s.GetAllProfessorsAsync(CancellationToken.None).GetAwaiter().GetResult());

        if (result.IsSuccess)
        {
            _availableProfessors = result.Value;
        }
        else
        {
            _availableProfessors = new List<ProfessorDto>();
            DialogHelper.ShowError("Failed to load professors for supervisor selection.");
        }
    }

    private void SetupUi()
    {
        var lblFirst = new Label("First Name:") { X = 1, Y = 1 };
        var txtFirst = new TextField("") { X = 15, Y = 1, Width = 30 };

        var lblLast = new Label("Last Name:") { X = 1, Y = 3 };
        var txtLast = new TextField("") { X = 15, Y = 3, Width = 30 };

        var lblStreet = new Label("Street:") { X = 1, Y = 5 };
        var txtStreet = new TextField("") { X = 15, Y = 5, Width = 30 };

        var lblCity = new Label("City:") { X = 1, Y = 7 };
        var txtCity = new TextField("") { X = 15, Y = 7, Width = 30 };

        var lblZip = new Label("Postal Code:") { X = 1, Y = 9 };
        var txtZip = new TextField("") { X = 15, Y = 9, Width = 10 };

        var lblYear = new Label("Year (3-5):") { X = 1, Y = 11 };
        var txtYear = new TextField("1") { X = 15, Y = 11, Width = 5 };

        var lblThesis = new Label("Thesis Topic:") { X = 1, Y = 13 };
        var txtThesis = new TextField("") { X = 15, Y = 13, Width = 30 };

        var lblSupervisor = new Label("Supervisor:") { X = 1, Y = 15 };

        var profNames = _availableProfessors
            .Select(p => $"{p.Title} {p.LastName} ({p.UniversityIndex})")
            .ToList();

        var comboSupervisor = new ComboBox
        {
            X = 15, Y = 15, Width = 30, Height = 5
        };
        comboSupervisor.SetSource(profNames);

        var btnSave = new Button("Save") { X = 15, Y = 18 };
        var btnCancel = new Button("Cancel") { X = 25, Y = 18 };

        btnSave.Clicked += () =>
        {
            if (!int.TryParse(txtYear.Text.ToString(), out var year))
            {
                DialogHelper.ShowError("Year must be a valid number.");
                txtYear.SetFocus();
                return;
            }

            if (comboSupervisor.SelectedItem < 0 || comboSupervisor.SelectedItem >= _availableProfessors.Count)
            {
                DialogHelper.ShowError("Please select a valid supervisor.");
                comboSupervisor.SetFocus();
                return;
            }

            var selectedProf = _availableProfessors[comboSupervisor.SelectedItem];

            var dto = new CreateMasterStudentDto(
                txtFirst.Text?.ToString() ?? string.Empty,
                txtLast.Text?.ToString() ?? string.Empty,
                year,
                txtStreet.Text?.ToString() ?? string.Empty,
                txtCity.Text?.ToString() ?? string.Empty,
                txtZip.Text?.ToString() ?? string.Empty,
                txtThesis.Text?.ToString() ?? string.Empty,
                selectedProf.Id
            );

            ExecuteServiceAction<IMasterStudentService>(service =>
            {
                var res = service.CreateMasterStudentAsync(dto, CancellationToken.None).GetAwaiter().GetResult();
                if (res.IsSuccess)
                {
                    DialogHelper.ShowSuccess("Master Student added successfully!");
                    _onSuccess?.Invoke();
                }
                else
                {
                    DialogHelper.ShowError(res.Error);
                }
            });
        };

        btnCancel.Clicked += () => Terminal.Gui.Application.RequestStop();

        Add(lblFirst, txtFirst,
            lblLast, txtLast,
            lblStreet, txtStreet,
            lblCity, txtCity,
            lblZip, txtZip,
            lblYear, txtYear,
            lblThesis, txtThesis,
            lblSupervisor, comboSupervisor,
            btnSave, btnCancel);
    }
}