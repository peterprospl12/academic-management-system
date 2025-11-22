using System.ComponentModel;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.ConsoleUI.Helpers;
using AMS.ConsoleUI.Views.Base;
using AMS.Domain.Enums;
using NStack;
using Terminal.Gui;
using Attribute = System.Attribute;

namespace AMS.ConsoleUI.Views.Professors;

public class CreateProfessorView : BaseView
{
    private readonly Action _onSuccess;

    public CreateProfessorView(IServiceProvider serviceProvider, Action onSuccess) : base(serviceProvider)
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

        var lblTitle = new Label("Title:") { X = 50, Y = 1 };

        var titleValues = Enum.GetValues<AcademicTitle>();

        var titleNames = titleValues
            .Select(t => (ustring)GetEnumDescription(t))
            .ToArray();

        var radioGroupTitle = new RadioGroup(titleNames)
        {
            X = 50,
            Y = 3,
            SelectedItem = 0
        };

        var btnSave = new Button("Save") { X = 15, Y = 12 };
        var btnCancel = new Button("Cancel") { X = 25, Y = 12 };

        btnSave.Clicked += () =>
        {
            var firstName = txtFirst.Text?.ToString() ?? string.Empty;
            var lastName = txtLast.Text?.ToString() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                DialogHelper.ShowError("First Name and Last Name are required.");
                return;
            }

            var selectedTitle = titleValues[radioGroupTitle.SelectedItem];

            var dto = new CreateProfessorDto(
                firstName,
                lastName,
                txtStreet.Text?.ToString() ?? string.Empty,
                txtCity.Text?.ToString() ?? string.Empty,
                txtZip.Text?.ToString() ?? string.Empty,
                selectedTitle
            );

            ExecuteServiceAction<IProfessorService>(service =>
            {
                var result = service.CreateProfessorAsync(dto, CancellationToken.None).GetAwaiter().GetResult();

                if (result.IsSuccess)
                {
                    DialogHelper.ShowSuccess("Professor added successfully!");
                    _onSuccess?.Invoke();
                }
                else
                {
                    DialogHelper.ShowError($"Failed to create professor:\n{result.Error}");
                }
            });
        };

        btnCancel.Clicked += () => { Terminal.Gui.Application.RequestStop(); };

        Add(lblFirst, txtFirst,
            lblLast, txtLast,
            lblStreet, txtStreet,
            lblCity, txtCity,
            lblZip, txtZip,
            lblTitle, radioGroupTitle,
            btnSave, btnCancel);
    }

    private string GetEnumDescription(Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));

        var description = attribute?.Description ?? value.ToString();

        return string.IsNullOrEmpty(description) ? "None / MSc (student)" : description;
    }
}