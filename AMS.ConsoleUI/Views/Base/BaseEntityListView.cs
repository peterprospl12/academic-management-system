using AMS.Application.Common.Models;
using AMS.ConsoleUI.Helpers;
using Terminal.Gui;

namespace AMS.ConsoleUI.Views.Base;

public abstract class BaseEntityListView<TModel, TService> : BaseView
{
    private List<TModel> _items;
    protected ListView ListView;

    protected BaseEntityListView(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        InitListView();
        RefreshList();
    }

    protected virtual bool AllowCreate => true;
    protected virtual bool AllowDelete => true;

    protected abstract string EntityName { get; }

    protected abstract View CreateAddView(Action onSuccessfullyAdded);

    private void InitListView()
    {
        ListView = new ListView
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        };

        ListView.OpenSelectedItem += OnItemClicked;

        ListView.KeyPress += e =>
        {
            if (AllowDelete && e.KeyEvent.Key == Key.DeleteChar)
            {
                DeleteSelectedItem();
                e.Handled = true;
            }
        };

        Add(ListView);
    }

    private void OnItemClicked(ListViewItemEventArgs e)
    {
        if (_items == null) return;

        if (AllowCreate && e.Item == _items.Count)
            OpenAddDialog();
        else if (AllowDelete && e.Item >= 0 && e.Item < _items.Count) DeleteSelectedItem();
    }

    private void DeleteSelectedItem()
    {
        if (!AllowDelete) return;

        if (ListView.SelectedItem < 0 || ListView.SelectedItem >= _items.Count) return;
        var item = _items[ListView.SelectedItem];
        ConfirmAndDelete(item);
    }

    protected void RefreshList()
    {
        var result = GetAllEntities(CancellationToken.None);

        if (result.IsFailure)
        {
            DialogHelper.ShowError(result.Error);
            return;
        }

        _items = result.Value;

        var displayList = _items.Select(FormatEntity).ToList();

        if (AllowCreate) displayList.Add($"[ < Add New {EntityName} > ]");

        ListView.SetSource(displayList);
    }

    private void OpenAddDialog()
    {
        var dialog = new Dialog($"Add New {EntityName}", 60, 20);
        var createView = CreateAddView(() =>
        {
            Terminal.Gui.Application.RequestStop();
            RefreshList();
        });
        createView.X = 0;
        createView.Y = 0;
        createView.Width = Dim.Fill();
        createView.Height = Dim.Fill() - 1;
        dialog.Add(createView);
        Terminal.Gui.Application.Run(dialog);
    }

    private void ConfirmAndDelete(TModel item)
    {
        if (DialogHelper.ConfirmDelete($"Delete {EntityName}", GetDeleteConfirmationMessage(item)))
        {
            var result = DeleteEntity(item, CancellationToken.None);
            if (result.IsSuccess) RefreshList();
            else DialogHelper.ShowError($"Failed to delete: {result.Error}");
        }
    }

    protected abstract Result<List<TModel>> GetAllEntities(CancellationToken token);
    protected abstract Result DeleteEntity(TModel entity, CancellationToken token);
    protected abstract string FormatEntity(TModel entity);
    protected abstract string GetDeleteConfirmationMessage(TModel entity);
}