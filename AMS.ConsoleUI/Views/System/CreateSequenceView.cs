using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.ConsoleUI.Helpers;
using AMS.ConsoleUI.Views.Base;
using Terminal.Gui;

namespace AMS.ConsoleUI.Views.System;

public class CreateSequenceView : BaseView
{
    private readonly Action _onSuccess;

    public CreateSequenceView(IServiceProvider serviceProvider, Action onSuccess) : base(serviceProvider)
    {
        _onSuccess = onSuccess;
        SetupUi();
    }

    private void SetupUi()
    {
        var lblPrefix = new Label("Prefix (e.g., 'S'):") { X = 1, Y = 1 };
        var txtPrefix = new TextField("") { X = 20, Y = 1, Width = 10 };

        var lblValue = new Label("Initial Value:") { X = 1, Y = 3 };
        var txtValue = new TextField("1") { X = 20, Y = 3, Width = 10 };

        var btnSave = new Button("Save") { X = 10, Y = 6 };
        var btnCancel = new Button("Cancel") { X = 22, Y = 6 };

        btnSave.Clicked += () =>
        {
            var prefix = txtPrefix.Text?.ToString()?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(prefix))
            {
                DialogHelper.ShowError("Prefix cannot be empty.");
                txtPrefix.SetFocus();
                return;
            }

            if (prefix.Length > 3)
            {
                DialogHelper.ShowError("Prefix is too long (max 3 chars).");
                return;
            }

            if (!int.TryParse(txtValue.Text.ToString(), out var initValue))
            {
                DialogHelper.ShowError("Initial Value must be a valid number.");
                txtValue.SetFocus();
                return;
            }

            if (initValue < 0)
            {
                DialogHelper.ShowError("Initial Value cannot be negative.");
                return;
            }

            var dto = new CreateSequenceDto(prefix, initValue);

            ExecuteServiceAction<ISequenceService>(service =>
            {
                var result = service.CreateSequenceAsync(dto, CancellationToken.None).GetAwaiter().GetResult();

                if (result.IsSuccess)
                {
                    DialogHelper.ShowSuccess($"Sequence '{prefix}' added successfully!");
                    _onSuccess?.Invoke();
                }
                else
                {
                    DialogHelper.ShowError($"Failed to create sequence:\n{result.Error}");
                }
            });
        };

        btnCancel.Clicked += () => { Terminal.Gui.Application.RequestStop(); };

        Add(lblPrefix, txtPrefix, lblValue, txtValue, btnSave, btnCancel);
    }
}