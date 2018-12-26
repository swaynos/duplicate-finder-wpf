using DuplicateFinder.Framework;
using NLog;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace DuplicateFinder
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ILogger _logger;

        internal App(ILogger logger) : base()
        {
            _logger = logger;
            DispatcherUnhandledException += OnDispatcherUnhandledException;
        }

        public App() : this(LogManager.GetCurrentClassLogger())
        {
        }


        // Maintain a reference to the NavigationService to navigate to pages outside of the code behind
        internal static IPageNavigationService NavigationService { get; set; }

        internal void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow main = new MainWindow();
            NavigationService = new PageNavigationService(main.NavigationService);
            main.Show();
        }

        /// <summary>
        /// Unhandled Application exceptions will call this method for logging
        /// </summary>
        internal void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            _logger.Fatal(e.Exception, "An unhandled error occured.");
            e.Handled = true;
            Shutdown();
        }
    }
}
