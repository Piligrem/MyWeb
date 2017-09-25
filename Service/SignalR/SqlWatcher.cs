using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InSearch.Services.SignalR
{
    public class SqlWatcher : IDisposable
    {
        private string ConnectionString;
        private SqlConnection Connection;
        private SqlCommand Command;
        private SqlDataAdapter Adapter;
        private DataSet Result;
        private SqlWatcherNotificationType NotificationType;

        public SqlWatcher(string ConnectionString, SqlCommand Command, SqlWatcherNotificationType NotificationType)
        {
            this.NotificationType = NotificationType;
            this.ConnectionString = ConnectionString;
            SqlDependency.Start(this.ConnectionString);
            this.Connection = new SqlConnection(this.ConnectionString);
            this.Connection.Open();
            this.Command = Command;
            this.Command.Connection = this.Connection;
            Adapter = new SqlDataAdapter(this.Command);
        }

        public void Start()
        {
            RegisterForChanges();
        }

        public void Stop()
        {
            SqlDependency.Stop(this.ConnectionString);
        }

        public delegate void SqlWatcherEventHandler(DataSet Result);

        public event SqlWatcherEventHandler OnChange;

        public DataSet DataSet
        {
            get { return Result; }
        }

        private void RegisterForChanges()
        {
            //Remove old dependency object
            this.Command.Notification = null;
            //Create new dependency object
            SqlDependency dep = new SqlDependency(this.Command);
            dep.OnChange += new OnChangeEventHandler(Handle_OnChange);
            //Save data
            Result = new DataSet();
            Adapter.Fill(Result);
            //Notify client of change to DataSet
            switch (NotificationType)
            {
                case SqlWatcherNotificationType.Blocking:
                    OnChange(Result);
                    break;
                case SqlWatcherNotificationType.Threaded:
                    ThreadPool.QueueUserWorkItem(ChangeEventWrapper, Result);
                    break;
            }
        }

        public void ChangeEventWrapper(object state)
        {
            DataSet Result = (DataSet)state;
            OnChange(Result);
        }

        private void Handle_OnChange(object sender, SqlNotificationEventArgs e)
        {
            if (e.Type != SqlNotificationType.Change)
                throw new ApplicationException("Failed to create queue notification subscription!");

            //Clean up the old notification
            SqlDependency dep = (SqlDependency)sender;
            dep.OnChange -= Handle_OnChange;

            //Register for the new notification
            RegisterForChanges();
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
