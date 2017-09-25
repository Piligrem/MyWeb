using System.Collections.Generic;

namespace InSearch
{
    public interface IMergedData
    {
        bool MergedDataIgnore { get; set; }
        Dictionary<string, object> MergedDataValues { get; }
    }
}
