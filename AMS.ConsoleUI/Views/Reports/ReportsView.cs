using AMS.Application.Common.Models;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.ConsoleUI.Helpers;
using AMS.ConsoleUI.Views.Base;
using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace AMS.ConsoleUI.Views.Reports;

public class ReportsView : BaseView
{
    private TextView _resultOutput;

    public ReportsView(IServiceProvider sp) : base(sp)
    {
        SetupUi();
    }

    private void SetupUi()
    {
        var btnHardest = new Button("1. Hardest Plan (Student)") { X = 1, Y = 1 };
        var btnTopProf = new Button("2. Top Professor (Most Students)") { X = 1, Y = 3 };
        var btnCourseGpa = new Button("3. Course GPA (By Department)") { X = 1, Y = 5 };

        _resultOutput = new TextView
        {
            X = 1, Y = 8,
            Width = Dim.Fill() - 1,
            Height = Dim.Fill() - 1,
            ReadOnly = true,
            ColorScheme = new ColorScheme { Normal = new Attribute(Color.Green, Color.Black) }
        };

        btnHardest.Clicked += ShowHardestPlan;
        btnTopProf.Clicked += ShowTopProf;
        btnCourseGpa.Clicked += ShowDepartmentSelector;

        Add(btnHardest, btnTopProf, btnCourseGpa, _resultOutput);
    }

    private void ShowHardestPlan()
    {
        ExecuteServiceAction<IReportsService>(service =>
        {
            var res = service.GetStudentWithHardestPlanAsync(CancellationToken.None).GetAwaiter().GetResult();

            if (res.IsSuccess)
                _resultOutput.Text = $"--- HARDEST PLAN REPORT ---\n\n" +
                                     $"Student: {res.Value.FullName} ({res.Value.IndexNumber})\n" +
                                     $"Semester: {res.Value.Semester}\n" +
                                     $"Current ECTS: {res.Value.CurrentEcts}\n" +
                                     $"Prerequisites ECTS: {res.Value.PrerequisitesEcts}\n" +
                                     $"TOTAL DIFFICULTY: {res.Value.TotalDifficultyScore}";
            else
                _resultOutput.Text = $"Error: {res.Error}";
        });
    }

    private void ShowTopProf()
    {
        ExecuteServiceAction<IReportsService>(service =>
        {
            var res = service.GetMostPopularProfessorAsync(CancellationToken.None).GetAwaiter().GetResult();

            if (res.IsSuccess)
                _resultOutput.Text = $"--- TOP PROFESSOR REPORT ---\n\n" +
                                     $"Name: {res.Value.FullName}\n" +
                                     $"Title: {res.Value.AcademicTitle}\n" +
                                     $"TOTAL STUDENTS: {res.Value.TotalStudentsCount}";
            else
                _resultOutput.Text = $"Error: {res.Error}";
        });
    }

    private void ShowDepartmentSelector()
    {
        var departmentsResult = ExecuteServiceFunc<IDepartmentService, Result<List<DepartmentDto>>>(service =>
            service.GetAllDepartmentsAsync(CancellationToken.None).GetAwaiter().GetResult());

        if (departmentsResult.IsFailure || !departmentsResult.Value.Any())
        {
            DialogHelper.ShowError("No departments found or failed to load.");
            return;
        }

        var departments = departmentsResult.Value;

        var dialog = new Dialog("Select Department", 40, 15);

        var list = new ListView(departments.Select(d => d.Name).ToList())
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        };

        var btnSelect = new Button("Select");
        var btnCancel = new Button("Cancel");

        btnSelect.Clicked += () =>
        {
            var selectedIndex = list.SelectedItem;
            if (selectedIndex < 0 || selectedIndex >= departments.Count) return;

            var selectedDept = departments[selectedIndex];

            Terminal.Gui.Application.RequestStop();

            GenerateGpaReport(selectedDept.Id, selectedDept.Name);
        };

        btnCancel.Clicked += () => Terminal.Gui.Application.RequestStop();

        dialog.Add(list);
        dialog.AddButton(btnSelect);
        dialog.AddButton(btnCancel);

        Terminal.Gui.Application.Run(dialog);
    }

    private void GenerateGpaReport(Guid departmentId, string departmentName)
    {
        ExecuteServiceAction<IReportsService>(service =>
        {
            var report = service.GetCoursesGpaByDepartmentAsync(departmentId, CancellationToken.None)
                .GetAwaiter().GetResult();

            if (report.IsSuccess)
            {
                var text = $"--- GPA REPORT FOR: {departmentName.ToUpper()} ---\n\n";

                if (!report.Value.Any())
                    text += "No graded courses found on this department.";
                else
                    foreach (var item in report.Value)
                    {
                        text += $"Course: {item.CourseName} ({item.CourseCode})\n";
                        text += $"  -> Average GPA: {item.AverageGrade:F2}\n";
                        text += $"  -> Graded Students: {item.GradedStudentsCount}\n\n";
                    }

                _resultOutput.Text = text;
            }
            else
            {
                _resultOutput.Text = $"Error retrieving GPA report: {report.Error}";
            }
        });
    }
}