using AMS.Application.Common.Models;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.ConsoleUI.Helpers;
using AMS.ConsoleUI.Views.Base;
using Terminal.Gui;

namespace AMS.ConsoleUI.Views.Enrollments;

public class StudentSelectionForEnrollmentView : BaseView
{
    private ListView? _studentListView;
    private List<StudentDto> _students = new();

    public StudentSelectionForEnrollmentView(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        LoadStudents();
        SetupUi();
    }

    private void LoadStudents()
    {
        var result = ExecuteServiceFunc<IStudentService, Result<List<StudentDto>>>(service =>
            service.GetAllStudentsAsync(CancellationToken.None).GetAwaiter().GetResult());

        if (result.IsSuccess)
            _students = result.Value
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .ToList();
        else
            DialogHelper.ShowError($"Failed to load students: {result.Error}");
    }

    private void SetupUi()
    {
        var lblInfo = new Label("Select a student to view their enrollments:")
        {
            X = 1,
            Y = 1
        };

        _studentListView = new ListView(_students.Select(FormatStudent).ToList())
        {
            X = 1,
            Y = 3,
            Width = Dim.Fill(1),
            Height = Dim.Fill(2),
            AllowsMarking = false
        };

        var btnView = new Button("View Enrollments")
        {
            X = Pos.Center(),
            Y = Pos.Bottom(_studentListView)
        };
        btnView.Clicked += OnViewEnrollments;

        Add(lblInfo, _studentListView, btnView);
    }

    private void OnViewEnrollments()
    {
        if (_studentListView?.SelectedItem < 0 || _studentListView.SelectedItem >= _students.Count)
        {
            DialogHelper.ShowError("Please select a student from the list.");
            return;
        }

        var selectedStudent = _students[_studentListView.SelectedItem];
        var enrollmentView = new EnrollmentListView(ServiceProvider, selectedStudent.Id);

        // Zastąp bieżący widok nowym widokiem wewnątrz nadrzędnego kontenera
        var parent = SuperView;
        if (parent != null)
        {
            parent.Remove(this);
            parent.Add(enrollmentView);
            parent.SetFocus();
        }
    }

    private string FormatStudent(StudentDto student)
    {
        return $"{student.LastName}, {student.FirstName} ({student.UniversityIndex})";
    }
}