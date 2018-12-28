using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicateFinder.Utilities
{
    class Process : IProcess
    {
        private ILogger _logger;

        public Process() : this (LogManager.GetCurrentClassLogger())
        { 
        }

        public Process(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Starts <see cref="System.Diagnostics.Process.Start(string)"/> for the given fileName
        /// asynchronously returning the result.
        /// </summary>
        /// <param name="fileName">The fileName to start in a new process</param>
        /// <returns>The result of <see cref="System.Diagnostics.Process.Start(string)"/>, bool if
        /// an exception occurs.</returns>
        public async Task<bool> StartAsync(string fileName)
        {
            Task<bool> task = Task.Run(() =>
            {
                return Start(fileName);
            });
            return await task;

            // The only way we reach here is if an exception occured
            throw task.Exception;
        }

        private bool Start(string fileName)
        {
            try
            {
                var process = new System.Diagnostics.Process();
                process.StartInfo.FileName = fileName;
                return process.Start();
            }
            catch (Win32Exception ex)
            {
                _logger.Log(LogLevel.Error, ex);
                return false;
            }
        }
    }
}
