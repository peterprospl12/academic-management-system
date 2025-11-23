using AMS.Application.Common.Models;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.ConsoleUI.Views.Base;
using Terminal.Gui;

namespace AMS.ConsoleUI.Views.Departments;

public class DepartmentListView(IServiceProvider sp) : BaseEntityListView<DepartmentDto, IDepartmentService>(sp)
{
    protected override string EntityName => "Department";

    protected override View CreateAddView(Action onSuccessfullyAdded)
    {
        return new CreateDepartmentView(ServiceProvider, onSuccessfullyAdded);
    }

    protected override Result<List<DepartmentDto>> GetAllEntities(CancellationToken token)
    {
        return ExecuteServiceFunc<IDepartmentService, Result<List<DepartmentDto>>>(s =>
            s.GetAllDepartmentsAsync(token).GetAwaiter().GetResult());
    }

    protected override Result DeleteEntity(DepartmentDto entity, CancellationToken token)
    {
        return ExecuteServiceFunc<IDepartmentService, Result>(s =>
            s.DeleteDepartmentAsync(entity.Id, token).GetAwaiter().GetResult());
    }

    protected override string FormatEntity(DepartmentDto d)
    {
        return d.Name;
    }

    protected override string GetDeleteConfirmationMessage(DepartmentDto d)
    {
        return $"Delete department {d.Name}?";
    }
}