using AMS.Application.Common.Models;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.ConsoleUI.Views.Base;
using Terminal.Gui;

namespace AMS.ConsoleUI.Views.Students;

public class StudentListView : BaseEntityListView<StudentDto, IStudentService>
{
    public StudentListView(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override string EntityName => "Student";

    // Tu podpinamy nasz widok tworzenia!
    protected override View CreateAddView(Action onSuccessfullyAdded)
    {
        return new CreateStudentView(ServiceProvider, onSuccessfullyAdded);
    }

    protected override Result<List<StudentDto>> GetAllEntities(CancellationToken token)
    {
        return ExecuteServiceFunc<IStudentService, Result<List<StudentDto>>>(s =>
            s.GetAllStudentsAsync(token).GetAwaiter().GetResult());
    }

    protected override Result DeleteEntity(StudentDto entity, CancellationToken token)
    {
        return ExecuteServiceFunc<IStudentService, Result>(s =>
            s.DeleteStudentAsync(entity.Id, token).GetAwaiter().GetResult());
    }

    protected override string FormatEntity(StudentDto s)
    {
        return $"{s.UniversityIndex} - {s.FirstName} {s.LastName}";
    }

    protected override string GetDeleteConfirmationMessage(StudentDto s)
    {
        return
            $"Are you sure you want to delete {s.FirstName} {s.LastName}?\nIndex {s.UniversityIndex} will be released.";
    }
}