using AMS.Application.Common.Models;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.ConsoleUI.Views.Base;
using Terminal.Gui;

namespace AMS.ConsoleUI.Views.System;

public class SequenceListView(IServiceProvider sp) : BaseEntityListView<SequenceDto, ISequenceService>(sp)
{
    protected override bool AllowCreate => false;
    protected override bool AllowDelete => false;

    protected override string EntityName => "Sequence";

    protected override Result<List<SequenceDto>> GetAllEntities(CancellationToken token)
    {
        return ExecuteServiceFunc<ISequenceService, Result<List<SequenceDto>>>(s =>
            s.GetAllSequencesAsync(token).GetAwaiter().GetResult());
    }

    protected override string FormatEntity(SequenceDto s)
    {
        return $"Prefix: '{s.Prefix}' | Next Value: {s.InitialValue}";
    }


    protected override View CreateAddView(Action onSuccessfullyAdded)
    {
        return null!;
    }

    protected override Result DeleteEntity(SequenceDto entity, CancellationToken token)
    {
        return Result.Failure("Operation not supported");
    }

    protected override string GetDeleteConfirmationMessage(SequenceDto entity)
    {
        return string.Empty;
    }
}