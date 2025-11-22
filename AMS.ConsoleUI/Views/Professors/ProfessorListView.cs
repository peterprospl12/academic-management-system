using AMS.Application.Common.Models;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.ConsoleUI.Extensions;
using AMS.ConsoleUI.Views.Base;
using Terminal.Gui;

// Potrzebne do zwracania View

namespace AMS.ConsoleUI.Views.Professors;

public class ProfessorListView : BaseEntityListView<ProfessorDto, IProfessorService>
{
    public ProfessorListView(IServiceProvider sp) : base(sp)
    {
    }

    protected override string EntityName => "Professor";

    protected override View CreateAddView(Action onSuccessfullyAdded)
    {
        return new CreateProfessorView(ServiceProvider, onSuccessfullyAdded);
    }

    protected override Result<List<ProfessorDto>> GetAllEntities(CancellationToken token)
    {
        return ExecuteServiceFunc<IProfessorService, Result<List<ProfessorDto>>>(s =>
            s.GetAllProfessorsAsync(token).GetAwaiter().GetResult());
    }

    protected override Result DeleteEntity(ProfessorDto entity, CancellationToken token)
    {
        return ExecuteServiceFunc<IProfessorService, Result>(s =>
            s.DeleteProfessorAsync(entity.Id, token).GetAwaiter().GetResult());
    }

    protected override string FormatEntity(ProfessorDto p)
    {
        return $"{p.Title.ToDescription()} {p.FirstName} {p.LastName} ({p.UniversityIndex}) - Office: {p.OfficeRoom}";
    }

    protected override string GetDeleteConfirmationMessage(ProfessorDto p)
    {
        return $"Delete Professor {p.LastName}?\nThis will unassign their courses (resetting instructor field).";
    }
}