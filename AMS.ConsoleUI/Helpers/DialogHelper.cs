using Terminal.Gui;

namespace AMS.ConsoleUI.Helpers;

public static class DialogHelper
{
    public static bool ConfirmDelete(string title, string message)
    {
        var index = MessageBox.Query(title, message, "Yes", "No");
        return index == 0;
    }

    public static void ShowError(string message)
    {
        MessageBox.ErrorQuery("Error", message, "Ok");
    }

    public static void ShowSuccess(string message)
    {
        MessageBox.Query("Success", message, "Ok");
    }
}