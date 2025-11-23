using AMS.Application.Common.Models;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.ConsoleUI.Helpers;
using AMS.ConsoleUI.Views.Base;
using Terminal.Gui;

namespace AMS.ConsoleUI.Views.Enrollments;

public class CreateEnrollmentView : BaseView
{
    private readonly Action _onSuccess;
    private readonly Guid _studentId;
    private List<CourseDto> _courses = new();

    public CreateEnrollmentView(IServiceProvider serviceProvider, Action onSuccess, Guid studentId) : base(
        serviceProvider)
    {
        _onSuccess = onSuccess;
        _studentId = studentId;
        LoadCourses();
        SetupUi();
    }

    private void LoadCourses()
    {
        var result = ExecuteServiceFunc<ICourseService, Result<List<CourseDto>>>(service =>
            service.GetAllCoursesAsync(CancellationToken.None).GetAwaiter().GetResult());

        if (result.IsSuccess)
            _courses = result.Value.OrderBy(c => c.Name).ToList();
        else
            DialogHelper.ShowError($"Failed to load courses: {result.Error}");
    }

    private void SetupUi()
    {
        var lblCourse = new Label("Course:") { X = 1, Y = 1 };
        var comboCourse = new ComboBox
        {
            X = 15,
            Y = 1,
            Width = 40,
            Height = 6
        };
        comboCourse.SetSource(_courses.Select(c => $"{c.Name} ({c.Code})").ToList());
        if (_courses.Count > 0) comboCourse.SelectedItem = 0;

        var lblSemester = new Label("Semester:") { X = 1, Y = 3 };
        var txtSemester = new TextField(string.Empty) { X = 15, Y = 3, Width = 40 };

        var btnSave = new Button("Save") { X = 15, Y = 5 };
        var btnCancel = new Button("Cancel") { X = 25, Y = 5 };

        btnSave.Clicked += () =>
        {
            var courseIndex = comboCourse.SelectedItem;
            if (courseIndex < 0 || courseIndex >= _courses.Count)
            {
                DialogHelper.ShowError("Please select a course.");
                return;
            }

            var semester = txtSemester.Text?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(semester))
            {
                DialogHelper.ShowError("Semester is required.");
                return;
            }

            var selectedCourse = _courses[courseIndex];
            var dto = new EnrollStudentDto(_studentId, selectedCourse.Id, semester.Trim());

            ExecuteServiceAction<IEnrollmentService>(service =>
            {
                var result = service.EnrollStudentAsync(dto, CancellationToken.None).GetAwaiter().GetResult();
                if (result.IsSuccess)
                {
                    DialogHelper.ShowSuccess("Student enrolled successfully.");
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

        btnSave.Enabled = _courses.Count > 0;

        Add(lblCourse, comboCourse, lblSemester, txtSemester, btnSave, btnCancel);
    }
}