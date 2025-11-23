using AMS.Application.Common.Models;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.ConsoleUI.Helpers;
using AMS.ConsoleUI.Views.Base;
using Terminal.Gui;

namespace AMS.ConsoleUI.Views.Courses;

public class CreateCourseView : BaseView
{
    private readonly Action _onSuccess;
    private List<DepartmentDto> _departments = new();
    private List<ProfessorDto> _professors = new();

    public CreateCourseView(IServiceProvider serviceProvider, Action onSuccess) : base(serviceProvider)
    {
        _onSuccess = onSuccess;
        LoadLookups();
        SetupUi();
    }

    private void LoadLookups()
    {
        var deptResult = ExecuteServiceFunc<IDepartmentService, Result<List<DepartmentDto>>>(service =>
            service.GetAllDepartmentsAsync(CancellationToken.None).GetAwaiter().GetResult());
        if (deptResult.IsSuccess)
            _departments = deptResult.Value.OrderBy(d => d.Name).ToList();
        else
            DialogHelper.ShowError("Failed to load departments.");

        var profResult = ExecuteServiceFunc<IProfessorService, Result<List<ProfessorDto>>>(service =>
            service.GetAllProfessorsAsync(CancellationToken.None).GetAwaiter().GetResult());
        if (profResult.IsSuccess)
            _professors = profResult.Value
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .ToList();
        else
            DialogHelper.ShowError("Failed to load professors.");
    }

    private void SetupUi()
    {
        var lblName = new Label("Name:") { X = 1, Y = 1 };
        var txtName = new TextField(string.Empty) { X = 15, Y = 1, Width = 40 };

        var lblCode = new Label("Code:") { X = 1, Y = 3 };
        var txtCode = new TextField(string.Empty) { X = 15, Y = 3, Width = 20 };

        var lblEcts = new Label("ECTS:") { X = 1, Y = 5 };
        var txtEcts = new TextField("6") { X = 15, Y = 5, Width = 5 };

        var lblDept = new Label("Department:") { X = 1, Y = 7 };
        var comboDept = new ComboBox { X = 15, Y = 7, Width = 40, Height = 6 };
        comboDept.SetSource(_departments.Select(d => d.Name).ToList());
        if (_departments.Count > 0)
            comboDept.SelectedItem = 0;

        var lblLecturer = new Label("Lecturer:") { X = 1, Y = 9 };
        var comboLecturer = new ComboBox { X = 15, Y = 9, Width = 40, Height = 6 };
        comboLecturer.SetSource(_professors
            .Select(p => $"{p.Title} {p.FirstName} {p.LastName} ({p.UniversityIndex})")
            .ToList());
        if (_professors.Count > 0)
            comboLecturer.SelectedItem = 0;

        var btnSave = new Button("Save") { X = 15, Y = 13 };
        var btnCancel = new Button("Cancel") { X = 25, Y = 13 };

        btnSave.Clicked += () =>
        {
            if (_departments.Count == 0 || _professors.Count == 0)
            {
                DialogHelper.ShowError("Missing departments or professors.");
                return;
            }

            if (!int.TryParse(txtEcts.Text?.ToString(), out var ects) || ects <= 0)
            {
                DialogHelper.ShowError("ECTS must be a positive number.");
                txtEcts.SetFocus();
                return;
            }

            var name = txtName.Text?.ToString() ?? string.Empty;
            var code = txtCode.Text?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(code))
            {
                DialogHelper.ShowError("Name and Code are required.");
                return;
            }

            var deptIndex = comboDept.SelectedItem;
            var lecturerIndex = comboLecturer.SelectedItem;
            if (deptIndex < 0 || deptIndex >= _departments.Count)
            {
                DialogHelper.ShowError("Select a department.");
                comboDept.SetFocus();
                return;
            }

            if (lecturerIndex < 0 || lecturerIndex >= _professors.Count)
            {
                DialogHelper.ShowError("Select a lecturer.");
                comboLecturer.SetFocus();
                return;
            }

            var dto = new CreateCourseDto(
                name.Trim(),
                code.Trim().ToUpperInvariant(),
                ects,
                _departments[deptIndex].Id,
                _professors[lecturerIndex].Id
            );

            ExecuteServiceAction<ICourseService>(service =>
            {
                var result = service.CreateCourseAsync(dto, CancellationToken.None).GetAwaiter().GetResult();
                if (result.IsSuccess)
                {
                    DialogHelper.ShowSuccess("Course created successfully.");
                    _onSuccess?.Invoke();
                }
                else
                {
                    DialogHelper.ShowError(result.Error);
                }
            });
        };

        btnCancel.Clicked += () => Terminal.Gui.Application.RequestStop();

        btnSave.Enabled = _departments.Count > 0 && _professors.Count > 0;

        Add(lblName, txtName,
            lblCode, txtCode,
            lblEcts, txtEcts,
            lblDept, comboDept,
            lblLecturer, comboLecturer,
            btnSave, btnCancel);
    }
}