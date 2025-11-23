using AMS.Application.Common.Models;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.ConsoleUI.Extensions;
using AMS.ConsoleUI.Views.Base;
using Terminal.Gui;

namespace AMS.ConsoleUI.Views.Students;

public class MasterStudentListView(IServiceProvider sp)
    : BaseEntityListView<MasterStudentDto, IMasterStudentService>(sp)
{
    protected override string EntityName => "Master Student";

    protected override View CreateAddView(Action onSuccessfullyAdded)
    {
        return new CreateMasterStudentView(ServiceProvider, onSuccessfullyAdded);
    }

    protected override Result<List<MasterStudentDto>> GetAllEntities(CancellationToken token)
    {
        return ExecuteServiceFunc<IMasterStudentService, Result<List<MasterStudentDto>>>(s =>
            s.GetAllMasterStudentsAsync(token).GetAwaiter().GetResult());
    }

    protected override Result DeleteEntity(MasterStudentDto entity, CancellationToken token)
    {
        return ExecuteServiceFunc<IMasterStudentService, Result>(s =>
            s.DeleteMasterStudentAsync(entity.Id, token).GetAwaiter().GetResult());
    }

    protected override string FormatEntity(MasterStudentDto s)
    {
        return
            $"{s.Index} - {s.FirstName} {s.LastName} | Thesis: {s.ThesisTopic}" +
            $" (Sup: {s.Promoter?.Title.ToDescription()} {s.Promoter?.FirstName} {s.Promoter?.LastName})";
    }

    protected override string GetDeleteConfirmationMessage(MasterStudentDto s)
    {
        return $"Delete Master Student {s.FirstName} {s.LastName}?\nThis will also remove their thesis assignment.";
    }
}