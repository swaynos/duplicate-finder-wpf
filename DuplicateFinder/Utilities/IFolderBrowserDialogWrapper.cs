using System.Windows.Forms;

namespace DuplicateFinder.Utilities
{
    /// <summary>
    /// <para>Iterface wrapper of System.Windows.Forms.FolderBrowserDialog</para>
    /// <para>Allows for dependency injection in Unit Tests</para>
    /// </summary>
    internal interface IFolderBrowserDialogWrapper
    {
        /// <summary>
        /// Return a new instance of FolderBrowserDialog
        /// </summary>
        /// <returns></returns>
        CommonDialog GetNewFolderBrowserDialog();

        /// <summary>
        /// A simple wrapper of dialog.ShowDialog()
        /// </summary>
        /// <param name="dialog">The dialog to call ShowDialog() on</param>
        /// <returns>The DialogResult of ShowDialog()</returns>
        DialogResult ShowDialogWrapper(CommonDialog dialog);

        /// <summary>
        /// Return the selected path of the provided FolderBrowserDialog.
        /// </summary>
        /// <param name="dialog">typeof(FolderBrowserDialog) the dialog to retrieve the selected path. An <strong>ArgumentException</strong>
        /// will be thrown if the dialog is null or of the wrong Type.</param>
        /// <returns>The selected path</returns>
        string GetSelectedPathFromDialog(CommonDialog dialog);
    }
}
