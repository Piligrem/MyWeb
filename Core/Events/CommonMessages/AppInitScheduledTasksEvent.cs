using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using InSearch.Core.Domain.Tasks;

namespace InSearch.Core.Events
{
	/// <summary>
	/// to initialize scheduled tasks in Application_Start
	/// </summary>
	/// <remarks></remarks>
	public class AppInitScheduledTasksEvent
	{
		public List<ScheduleTask> ScheduledTasks { get; set; }
	}
}
