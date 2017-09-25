using System;
using System.Collections.Generic;
using InSearch.Core.Localization;

namespace InSearch.Core.Logging
{
    public enum NotifyType
    {
        Info,
        Success,
        Warning,
        Error
    }

    [Serializable]
    public class NotifyEntry : ComparableObject<NotifyEntry>
    {
        [ObjectSignature]
        public NotifyType Type { get; set; }

        [ObjectSignature]
        public LocalizedString Message { get; set; }

        public bool Durable { get; set; }
    }
}
