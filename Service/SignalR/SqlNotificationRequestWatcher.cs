using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace InSearch.Services.SignalR
{
    class SqlNotificationRequestWatcher : DispatcherObject
    {
        #region Fields
        private readonly string connectionString;
        private readonly string listenerSQL;
        private readonly string serviceName;
        private SqlCommand command = null;
        private int NotificationTimeout = 600;
        private string messageText;
        #endregion Fields

        public SqlNotificationRequestWatcher(string connectionString, string listenerSQL, string serviceName)
        {
            this.connectionString = connectionString;
            this.listenerSQL = listenerSQL;
            this.serviceName = serviceName;
        }

        private void StartListener()
        {
            // A separate listener thread is needed to 
            // monitor the queue for notifications.
            Thread listener = new Thread(Listen);
            listener.Name = "Query Notification Watcher";
            listener.Start();
        }

        private void Listen()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(listenerSQL, connection))
                {
                    connection.Open();
                    // Make sure we don't time out before the
                    // notification request times out.
                    command.CommandTimeout = NotificationTimeout + 15;
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        messageText = System.Text.ASCIIEncoding.ASCII.GetString((byte[])reader.GetValue(13)).ToString();
                        // Empty queue of messages.
                        // Application logic could parse
                        // the queue data and 
                        // change its notification logic.
                    }

                    object[] args = { this, EventArgs.Empty };
                    EventHandler notify = new EventHandler(OnNotificationComplete);
                    // Notify the UI thread that a notification
                    // has occurred.
                    OnNotificationComplete(notify, (EventArgs)(args[1]));
                    //this.BeginInvoke(notify, args);
                }
            }
        }

        private void OnNotificationComplete(object sender, EventArgs e)
        {
            messageText = messageText.Replace("??", "").Replace("\0", "");
            // The user can decide to register
            // and request a new notification by
            // checking the CheckBox on the form.
            GetData();
        }

        private void GetData()
        {
            // Make sure the command object does not already have
            // a notification object associated with it.
            command.Notification = null;
            SqlNotificationRequest snr =
            new SqlNotificationRequest();
            snr.UserData = new Guid().ToString();
            snr.Options = "Service=" + serviceName;
            // If a time-out occurs, a notification
            // will indicate that is the 
            // reason for the notification.
            snr.Timeout = NotificationTimeout;
            command.Notification = snr;
            // Start the background listener.
            StartListener();
        }
    }
}
