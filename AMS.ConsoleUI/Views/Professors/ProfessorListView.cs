using AMS.Application.Common.Models;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.ConsoleUI.Views.Base;
using AMS.ConsoleUI.Views.Offices;
using Terminal.Gui;

namespace AMS.ConsoleUI.Views.Professors;

public class ProfessorListView : BaseEntityListView<ProfessorDto, IProfessorService>
{
    public ProfessorListView(IServiceProvider sp) : base(sp)
    {
        ListView.KeyPress += OnKeyPress;
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
        var officeInfo = string.IsNullOrEmpty(p.OfficeRoom) ? "No Office" : $"Room {p.OfficeRoom}";
        return $"{p.Title} {p.FirstName} {p.LastName} ({p.UniversityIndex}) - {officeInfo}";
    }

    protected override string GetDeleteConfirmationMessage(ProfessorDto p)
    {
        return $"Delete Professor {p.LastName}?";
    }

    private void OnKeyPress(KeyEventEventArgs e)
    {
        if (e.KeyEvent.Key == Key.o || e.KeyEvent.Key == (Key.O | Key.ShiftMask))
        {
            var selectedItemIndex = ListView.SelectedItem;

            if (Items != null && selectedItemIndex >= 0 && selectedItemIndex < Items.Count)
            {
                var prof = Items[selectedItemIndex];
                OpenAssignOfficeDialog(prof);
                e.Handled = true;
            }
        }
    }

    private void OpenAssignOfficeDialog(ProfessorDto professor)
    {
        var dialog = new Dialog("Assign Office", 50, 14);

        var view = new AssignOfficeView(ServiceProvider, professor, () =>
        {
            Terminal.Gui.Application.RequestStop();
            RefreshList();
        });

        view.X = 0;
        view.Y = 0;
        view.Width = Dim.Fill();
        view.Height = Dim.Fill();

        dialog.Add(view);
        Terminal.Gui.Application.Run(dialog);
    }
}