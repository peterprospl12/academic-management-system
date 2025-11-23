using AMS.Application.Interfaces;
using AMS.ConsoleUI.Helpers;
using AMS.ConsoleUI.Views.Base;
using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace AMS.ConsoleUI.Views.System;

public class DataGeneratorView : BaseView
{
    public DataGeneratorView(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        SetupUi();
    }

    private void SetupUi()
    {
        var lblTitle = new Label("Data Generator (Bogus)")
        {
            X = 1, Y = 1,
            ColorScheme = new ColorScheme { Normal = new Attribute(Color.Magenta, Color.Black) }
        };

        var lblStudents = new Label("Students to add:") { X = 1, Y = 3 };
        var txtStudents = new TextField("10") { X = 20, Y = 3, Width = 10 };

        var lblProfs = new Label("Professors to add:") { X = 1, Y = 5 };
        var txtProfs = new TextField("2") { X = 20, Y = 5, Width = 10 };

        var btnGenerate = new Button("GENERATE") { X = 10, Y = 8 };

        var txtLog = new TextView
        {
            X = 1, Y = 11,
            Width = Dim.Fill() - 1,
            Height = Dim.Fill() - 1,
            ReadOnly = true,
            ColorScheme = new ColorScheme { Normal = new Attribute(Color.Green, Color.Black) }
        };

        btnGenerate.Clicked += () =>
        {
            if (!int.TryParse(txtStudents.Text.ToString(), out var sCount) || sCount < 0) return;
            if (!int.TryParse(txtProfs.Text.ToString(), out var pCount) || pCount < 0) return;

            txtLog.Text = "Initializing...\n";
            Terminal.Gui.Application.MainLoop.Invoke(() => Terminal.Gui.Application.Refresh());

            Task.Run(async () =>
            {
                using var scope = ServiceProvider.CreateScope();
                var seeder = scope.ServiceProvider.GetRequiredService<IDataSeederService>();

                await seeder.GenerateDataAsync(sCount, pCount, msg =>
                {
                    Terminal.Gui.Application.MainLoop.Invoke(() =>
                    {
                        txtLog.Text += msg + "\n";
                        txtLog.ScrollTo(txtLog.Lines - 1);
                    });
                }, CancellationToken.None);

                Terminal.Gui.Application.MainLoop.Invoke(() =>
                {
                    DialogHelper.ShowSuccess("Data generation completed!");
                });
            });
        };

        Add(lblTitle, lblStudents, txtStudents, lblProfs, txtProfs, btnGenerate, txtLog);
    }
}