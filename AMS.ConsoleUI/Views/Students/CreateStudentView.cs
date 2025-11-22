using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.ConsoleUI.Helpers;
using AMS.ConsoleUI.Views.Base;
using Terminal.Gui;

namespace AMS.ConsoleUI.Views.Students;

public class CreateStudentView : BaseView
{
    private readonly Action _onSuccess;

    public CreateStudentView(IServiceProvider serviceProvider, Action onSuccess) : base(serviceProvider)
    {
        _onSuccess = onSuccess;
        SetupUi();
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

        var lblYear = new Label("Year (1-5):") { X = 1, Y = 11 };
        var txtYear = new TextField("1") { X = 15, Y = 11, Width = 5 };

        var btnSave = new Button("Save") { X = 15, Y = 14 };
        var btnCancel = new Button("Cancel") { X = 25, Y = 14 };

        btnSave.Clicked += () =>
        {
            if (!int.TryParse(txtYear.Text.ToString(), out var year))
            {
                DialogHelper.ShowError("Year must be a valid number.");
                txtYear.SetFocus();
                return;
            }

            var firstName = txtFirst.Text?.ToString() ?? string.Empty;
            var lastName = txtLast.Text?.ToString() ?? string.Empty;
            var street = txtStreet.Text?.ToString() ?? string.Empty;
            var city = txtCity.Text?.ToString() ?? string.Empty;
            var postalCode = txtZip.Text?.ToString() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                DialogHelper.ShowError("First Name and Last Name are required.");
                return;
            }

            var dto = new CreateStudentDto(
                firstName,
                lastName,
                street,
                city,
                postalCode,
                year
            );

            ExecuteServiceAction<IStudentService>(service =>
            {
                var result = service.CreateStudentAsync(dto, CancellationToken.None).GetAwaiter().GetResult();

                if (result.IsSuccess)
                {
                    DialogHelper.ShowSuccess("Student added!");
                    _onSuccess?.Invoke();
                }
                else
                {
                    DialogHelper.ShowError($"Failed to create student:\n{result.Error}");
                }
            });
        };

        btnCancel.Clicked += () => { Terminal.Gui.Application.RequestStop(); };

        Add(lblFirst, txtFirst,
            lblLast, txtLast,
            lblStreet, txtStreet,
            lblCity, txtCity,
            lblZip, txtZip,
            lblYear, txtYear,
            btnSave, btnCancel);
    }
}