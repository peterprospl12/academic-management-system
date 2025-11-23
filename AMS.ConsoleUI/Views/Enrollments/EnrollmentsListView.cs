using AMS.Application.Common.Models;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.ConsoleUI.Views.Base;
using Terminal.Gui;

namespace AMS.ConsoleUI.Views.Enrollments;

public class EnrollmentListView(IServiceProvider sp, Guid studentId)
    : BaseEntityListView<EnrollmentDto, IEnrollmentService>(sp)
{
    private readonly Guid _studentId = studentId;

    protected override string EntityName => "Enrollment";

    protected override View CreateAddView(Action onSuccessfullyAdded)
    {
        return new CreateEnrollmentView(ServiceProvider, onSuccessfullyAdded, _studentId);
    }

    protected override Result<List<EnrollmentDto>> GetAllEntities(CancellationToken token)
    {
        return ExecuteServiceFunc<IEnrollmentService, Result<List<EnrollmentDto>>>(s =>
            s.GetStudentEnrollmentsAsync(_studentId, token).GetAwaiter().GetResult());
    }

    protected override Result DeleteEntity(EnrollmentDto entity, CancellationToken token)
    {
        return ExecuteServiceFunc<IEnrollmentService, Result>(s =>
            s.UnenrollStudentAsync(entity.Id, token).GetAwaiter().GetResult());
    }

    protected override string FormatEntity(EnrollmentDto e)
    {
        var grade = e.Grade.HasValue ? e.Grade.Value.ToString("F1") : "N/A";
        return $"{e.CourseName} ({e.CourseCode}) - Semester: {e.Semester}, Grade: {grade} | Ects: {e.Ects}";
    }

    protected override string GetDeleteConfirmationMessage(EnrollmentDto e)
    {
        return $"Unenroll from course {e.CourseName}?";
    }
}