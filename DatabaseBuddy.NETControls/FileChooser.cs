using Microsoft.WindowsAPICodePack.Dialogs;

namespace DatabaseBuddy.NETControls
{
  public static class FileChooser
  {
    #region - public methods -
    #region [ChooseFile]
    public static CommonFileDialogResult ChooseFile()
    {
      using (var commonOpenFileDialog = new CommonOpenFileDialog { IsFolderPicker = true })
      {
        return commonOpenFileDialog.ShowDialog();
      }
    }
    #endregion
    #endregion
  }
}
