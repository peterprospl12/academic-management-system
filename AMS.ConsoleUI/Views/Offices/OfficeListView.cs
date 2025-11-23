using AMS.Application.Common.Models;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.ConsoleUI.Extensions;
using AMS.ConsoleUI.Views.Base;
using Terminal.Gui;

namespace AMS.ConsoleUI.Views.Offices;

public class OfficeListView : BaseEntityListView<OfficeDto, IOfficeService>
{
    public OfficeListView(IServiceProvider sp) : base(sp)
    {
    }

    protected override string EntityName => "Office";

    protected override View CreateAddView(Action onSuccessfullyAdded)
    {
        return new CreateOfficeView(ServiceProvider, onSuccessfullyAdded);
    }

    protected override Result<List<OfficeDto>> GetAllEntities(CancellationToken token)
    {
        return ExecuteServiceFunc<IOfficeService, Result<List<OfficeDto>>>(s =>
            s.GetAllOfficesAsync(token).GetAwaiter().GetResult());
    }

    protected override Result DeleteEntity(OfficeDto entity, CancellationToken token)
    {
        return ExecuteServiceFunc<IOfficeService, Result>(s =>
            s.DeleteOfficeAsync(entity.Id, token).GetAwaiter().GetResult());
    }

    protected override string FormatEntity(OfficeDto o)
    {
        var occupant = "Empty";
        if (o.OccupantId != null)
        {
            var profResult = ExecuteServiceFunc<IProfessorService, Result<ProfessorDto>>(s =>
                s.GetProfessorByIdAsync(o.OccupantId.Value, CancellationToken.None).GetAwaiter().GetResult());

            if (profResult.IsSuccess)
            {
                var p = profResult.Value;
                occupant = $"{p.Title.ToDescription()} {p.LastName}";
            }
        }

        return $"Room: {o.RoomNumber} ({o.Building}) - Occupant: {occupant}";
    }

    protected override string GetDeleteConfirmationMessage(OfficeDto o)
    {
        return $"Delete Office {o.RoomNumber}?";
    }
}