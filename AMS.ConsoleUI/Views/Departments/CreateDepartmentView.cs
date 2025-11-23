using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.ConsoleUI.Helpers;
using AMS.ConsoleUI.Views.Base;
using Terminal.Gui;

namespace AMS.ConsoleUI.Views.Departments;

public class CreateDepartmentView : BaseView
{
    private readonly Action _onSuccess;

    public CreateDepartmentView(IServiceProvider serviceProvider, Action onSuccess) : base(serviceProvider)
    {
        _onSuccess = onSuccess;
        SetupUi();
    }

    private void SetupUi()
    {
        var lblName = new Label("Name:") { X = 1, Y = 1 };
        var txtName = new TextField(string.Empty) { X = 15, Y = 1, Width = 40 };

        var btnSave = new Button("Save") { X = 15, Y = 3 };
        var btnCancel = new Button("Cancel") { X = 25, Y = 3 };

        btnSave.Clicked += () =>
        {
            var name = txtName.Text?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name))
            {
                DialogHelper.ShowError("Name is required.");
                return;
            }

            var dto = new CreateDepartmentDto(name.Trim());

            ExecuteServiceAction<IDepartmentService>(service =>
            {
                var result = service.CreateDepartmentAsync(dto, CancellationToken.None).GetAwaiter().GetResult();
                if (result.IsSuccess)
                {
                    DialogHelper.ShowSuccess("Department created successfully.");
                    _onSuccess?.Invoke();
                    Terminal.Gui.Application.RequestStop();
                }
                else
                {
                    DialogHelper.ShowError(result.Error);
                }
            });
        };

        btnCancel.Clicked += () => Terminal.Gui.Application.RequestStop();

        Add(lblName, txtName, btnSave, btnCancel);
    }
}