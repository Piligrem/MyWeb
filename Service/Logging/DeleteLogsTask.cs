using System;
using InSearch.Core.Domain.Logging;
using InSearch.Core.Logging;
using InSearch.Services.Tasks;

namespace InSearch.Services.Logging
{
    /// <summary>
    /// Represents a task for deleting log entries
    /// </summary>
    public partial class DeleteLogsTask : ITask
    {
        private readonly ILogger _logger;

        public DeleteLogsTask(ILogger logger)
        {
            this._logger = logger;
        }

        /// <summary>
        /// Executes a task
        /// </summary>
        public void Execute()
        {
            var olderThanDays = 7; // TODO: move to settings
            var toUtc = DateTime.UtcNow.AddDays(-olderThanDays);

			_logger.ClearLog(toUtc, LogLevel.Error);
        }
    }
}
