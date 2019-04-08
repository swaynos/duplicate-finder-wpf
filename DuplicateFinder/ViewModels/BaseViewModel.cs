using DuplicateFinder.Framework;
using FileHashRepository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DuplicateFinder.ViewModels
{
    public abstract class BaseViewModel : BindableBase
    {
        const string _dataFileName = "data.json"; // Consider making this configurable vs const
        protected IScannedFileStore _scannedFileStore;
        protected string _userAppDataPath;

        public BaseViewModel(IScannedFileStore scannedFileStore, string userAppDataPath)
        {
            _scannedFileStore = scannedFileStore;
        }

        public BaseViewModel() : this(new ScannedFileStore(), Application.UserAppDataPath) 
        {
        }

        /// <summary>
        /// Set the ScannedFileStore for this ScanPageViewModel
        /// </summary>
        internal void SetScannedFileStore(IScannedFileStore scannedFileStore)
        {
            _scannedFileStore = scannedFileStore;
        }

        // ToDo: Unit Test
        public string GetDataFilePath()
        {
            return Path.Combine(_userAppDataPath, _dataFileName);
        }
    }
}
