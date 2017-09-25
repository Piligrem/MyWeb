using InSearch.Core.Configuration;

namespace InSearch.Core.Domain.Common
{
    public class AdminAreaSettings : ISettings
    {
		public AdminAreaSettings()
		{
			GridPageSize = 10;
			RichEditorFlavor = "RichEditor";
		}
		
		public int GridPageSize { get; set; }

        public string RichEditorFlavor { get; set; }
    }
}