using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.ConsoleUI.Helpers;
using AMS.ConsoleUI.Views.Base;
using Terminal.Gui;

namespace AMS.ConsoleUI.Views.Offices;

public class CreateOfficeView : BaseView
{
    private readonly Action _onSuccess;

    public CreateOfficeView(IServiceProvider serviceProvider, Action onSuccess) : base(serviceProvider)
    {
        _onSuccess = onSuccess;
        SetupUi();
    }

    private void SetupUi()
    {
        var lblRoom = new Label("Room Number:") { X = 1, Y = 1 };
        var txtRoom = new TextField("") { X = 15, Y = 1, Width = 20 };

        var lblBuilding = new Label("Building:") { X = 1, Y = 3 };
        var txtBuilding = new TextField("Main A") { X = 15, Y = 3, Width = 20 };

        var btnSave = new Button("Save") { X = 10, Y = 6 };
        var btnCancel = new Button("Cancel") { X = 20, Y = 6 };

        btnSave.Clicked += () =>
        {
            var room = txtRoom.Text.ToString();
            var building = txtBuilding.Text.ToString();

            if (string.IsNullOrWhiteSpace(room))
            {
                DialogHelper.ShowError("Room number is required.");
                return;
            }

            var dto = new CreateOfficeDto(building ?? "None", room);

            ExecuteServiceAction<IOfficeService>(service =>
            {
                var result = service.CreateOfficeAsync(dto, CancellationToken.None).GetAwaiter().GetResult();

                if (result.IsSuccess)
                {
                    DialogHelper.ShowSuccess("Office created!");
                    _onSuccess?.Invoke();
                }
                else
                {
                    DialogHelper.ShowError(result.Error);
                }
            });
        };

        btnCancel.Clicked += () => Terminal.Gui.Application.RequestStop();

        Add(lblRoom, txtRoom, lblBuilding, txtBuilding, btnSave, btnCancel);
    }
}