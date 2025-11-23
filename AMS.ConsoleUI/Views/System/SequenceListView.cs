using AMS.Application.Common.Models;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.ConsoleUI.Views.Base;
using Terminal.Gui;

namespace AMS.ConsoleUI.Views.System;

public class SequenceListView(IServiceProvider sp) : BaseEntityListView<SequenceDto, ISequenceService>(sp)
{
    protected override string EntityName => "Sequence Counter";

    protected override View CreateAddView(Action onSuccessfullyAdded)
    {
        return new CreateSequenceView(ServiceProvider, onSuccessfullyAdded);
    }

    protected override Result<List<SequenceDto>> GetAllEntities(CancellationToken token)
    {
        return ExecuteServiceFunc<ISequenceService, Result<List<SequenceDto>>>(s =>
            s.GetAllSequencesAsync(token).GetAwaiter().GetResult());
    }

    protected override Result DeleteEntity(SequenceDto entity, CancellationToken token)
    {
        return ExecuteServiceFunc<ISequenceService, Result>(s =>
            s.DeleteSequenceAsync(entity.Prefix, token).GetAwaiter().GetResult());
    }

    protected override string FormatEntity(SequenceDto s)
    {
        return $"Prefix: '{s.Prefix}' | Current Value: {s.InitialValue}";
    }

    protected override string GetDeleteConfirmationMessage(SequenceDto s)
    {
        return $"Are you sure you want to delete sequence with Prefix '{s.Prefix}'?\n" +
               "Generating numbers for this prefix will no longer be possible.";
    }
}