using AMS.Application.Common.Models;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.ConsoleUI.Views.Base;
using Terminal.Gui;

namespace AMS.ConsoleUI.Views.Courses;

public class CourseListView(IServiceProvider sp) : BaseEntityListView<CourseDto, ICourseService>(sp)
{
    protected override string EntityName => "Course";

    protected override View CreateAddView(Action onSuccessfullyAdded)
    {
        return new CreateCourseView(ServiceProvider, onSuccessfullyAdded);
    }

    protected override Result<List<CourseDto>> GetAllEntities(CancellationToken token)
    {
        return ExecuteServiceFunc<ICourseService, Result<List<CourseDto>>>(s =>
            s.GetAllCoursesAsync(token).GetAwaiter().GetResult());
    }

    protected override Result DeleteEntity(CourseDto entity, CancellationToken token)
    {
        return ExecuteServiceFunc<ICourseService, Result>(s =>
            s.DeleteCourseAsync(entity.Id, token).GetAwaiter().GetResult());
    }

    protected override string FormatEntity(CourseDto c)
    {
        var prereqInfo = c.PrerequisitesCount == 0 ? "No prerequisites" : $"{c.PrerequisitesCount} prerequisites";
        return
            $"{c.Name} ({c.Code}) - {c.Ects} ECTS | Dept: {c.DepartmentName} | Lecturer: {c.LecturerName} | {prereqInfo}";
    }

    protected override string GetDeleteConfirmationMessage(CourseDto c)
    {
        return $"Delete course {c.Name} ({c.Code})?";
    }
}