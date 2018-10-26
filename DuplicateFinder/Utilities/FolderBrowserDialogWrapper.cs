using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DuplicateFinder.Utilities
{
    /// <summary>
    /// The implementation of IFolderBrowserDialog
    /// </summary>
    internal class FolderBrowserDialogWrapper : IFolderBrowserDialogWrapper
    {
        public CommonDialog GetNewFolderBrowserDialog()
        {
            return new FolderBrowserDialog();
        }

        public DialogResult ShowDialogWrapper(CommonDialog dialog)
        {
            return dialog.ShowDialog();
        }

        public string GetSelectedPathFromDialog(CommonDialog dialog)
        {
            FolderBrowserDialog folderBrowserDialog = dialog as FolderBrowserDialog;
            if (folderBrowserDialog == null)
            {
                throw new ArgumentException("Please provide a non-null type of FolderBrowserDialog.");
            }
            return folderBrowserDialog.SelectedPath;
        }
    }
}
