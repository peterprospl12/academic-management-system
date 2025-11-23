using AMS.Application.Common.Models;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.ConsoleUI.Helpers;
using AMS.ConsoleUI.Views.Base;
using NStack;
using Terminal.Gui;

namespace AMS.ConsoleUI.Views.Offices;

public class AssignOfficeView : BaseView
{
    private readonly Action _onSuccess;
    private readonly ProfessorDto _professor;
    private List<OfficeDto> _availableOffices = new();

    public AssignOfficeView(IServiceProvider serviceProvider, ProfessorDto professor, Action onSuccess)
        : base(serviceProvider)
    {
        _professor = professor;
        _onSuccess = onSuccess;

        LoadOffices();
        SetupUi();
    }

    private void LoadOffices()
    {
        var result = ExecuteServiceFunc<IOfficeService, Result<List<OfficeDto>>>(s =>
            s.GetAllOfficesAsync(CancellationToken.None).GetAwaiter().GetResult());

        if (result.IsSuccess)
            _availableOffices = result.Value
                .Where(o => o.OccupantId == null || o.OccupantId == _professor.Id)
                .OrderBy(o => o.RoomNumber)
                .ToList();
        else
            DialogHelper.ShowError("Failed to load offices.");
    }

    private void SetupUi()
    {
        var lblInfo = new Label($"Assign Office for: {_professor.Title} {_professor.LastName}")
        {
            X = 1, Y = 1, Width = Dim.Fill(), TextAlignment = TextAlignment.Left
        };

        var lblOffice = new Label("Select Office:") { X = 1, Y = 3 };

        var officeList = _availableOffices.Select(o =>
        {
            var isCurrent = o.OccupantId == _professor.Id;
            var status = isCurrent ? " [CURRENT]" : " [Empty]";
            return (ustring)$"{o.RoomNumber} ({o.Building}){status}";
        }).ToList();

        var comboOffice = new ComboBox
        {
            X = 16, Y = 3, Width = 30, Height = 6
        };
        comboOffice.SetSource(officeList);

        var currentIndex = _availableOffices.FindIndex(o => o.OccupantId == _professor.Id);
        if (currentIndex >= 0)
        {
            comboOffice.SelectedItem = currentIndex;
            comboOffice.Text = officeList[currentIndex];
        }

        var btnSave = new Button("Assign") { X = 10, Y = 8 };
        var btnCancel = new Button("Cancel") { X = 22, Y = 8 };

        btnSave.Clicked += () =>
        {
            if (comboOffice.SelectedItem < 0 || comboOffice.SelectedItem >= _availableOffices.Count)
            {
                DialogHelper.ShowError("Please select an office.");
                return;
            }

            var selectedOffice = _availableOffices[comboOffice.SelectedItem];

            ExecuteServiceAction<IOfficeService>(service =>
            {
                var result = service
                    .AssignProfessorToOfficeAsync(selectedOffice.Id, _professor.Id, CancellationToken.None)
                    .GetAwaiter().GetResult();

                if (result.IsSuccess)
                {
                    DialogHelper.ShowSuccess($"Professor assigned to room {selectedOffice.RoomNumber}!");
                    _onSuccess?.Invoke();
                }
                else
                {
                    DialogHelper.ShowError(result.Error);
                }
            });
        };

        btnCancel.Clicked += () => Terminal.Gui.Application.RequestStop();

        Add(lblInfo, lblOffice, comboOffice, btnSave, btnCancel);
    }
}