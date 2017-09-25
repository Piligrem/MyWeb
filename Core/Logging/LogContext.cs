using InSearch.Core.Domain.Users;
using InSearch.Core.Domain.Logging;

namespace InSearch.Core.Logging
{
    public class LogContext
    {
        public string ShortMessage { get; set; }
        public string FullMessage { get; set; }
        public LogLevel LogLevel { get; set; }
        public User User { get; set; }

        public bool HashNotFullMessage { get; set; }
        public bool HashIpAddress { get; set; }
    }
}
