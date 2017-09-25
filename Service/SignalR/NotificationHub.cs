using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Diagnostics;
using System.Threading;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using InSearch.Core;
using InSearch.Core.Data;
using InSearch.Core.Domain.Users;
using InSearch.Services.Users;
using InSearch.Services.Extensions;
using InSearch.Core.Logging;
using InSearch.Services.Media;

namespace InSearch.Services.SignalR
{
    [HubName("notificationHub")]
    public class NotificationHub : Hub
    {
        #region Fields
        //private static readonly Object s_lock = new Object();
        //private static NotificationHub instance = null;

        private static readonly List<Client> _clients = new List<Client>();
        private readonly IWorkContext _workContext;
        private object _syncRoot = new object();
        private readonly ISessionService _sessionService;
        private readonly IUserService _userService;
        private readonly ILogger _logger;
        private readonly IPictureService _pictureService;
        #endregion Fields

        #region Constructors
        public NotificationHub() 
        {
            _sessionService = EngineContext.Current.Resolve<ISessionService>();
            _workContext = EngineContext.Current.Resolve<IWorkContext>();
            _userService = EngineContext.Current.Resolve<IUserService>();
            _logger = EngineContext.Current.Resolve<ILogger>();
            _pictureService = EngineContext.Current.Resolve<IPictureService>();
        }
        #endregion Constructors

        #region Methods
        public override Task OnConnected()
        {
            Debug.WriteLine("#{0} - Event: {1}, Method: {2}", DateTime.Now.ToString("hh:mm:ss.fff"), "-", "NotificationHub.OnConnected");

            var currentUser = _userService.GetUserByUsername(Context.User.Identity.Name);

            if (!currentUser.IsRegistered()) return null;

            var connectionId = Context.ConnectionId;
            var client = new Client() { ConnectionId = connectionId, User = currentUser };

            if (_clients.Where(c => c.User == currentUser).Any())
                _clients.FirstOrDefault(c => c.User == currentUser).ConnectionId = connectionId;
            else
                _clients.Add(client);

            lock (_syncRoot)
            {
                Clients.Client(connectionId).connected(currentUser.Username, "connected");
            }
            _sessionService.AddNewSession();

            return base.OnConnected();
        }
        public override Task OnDisconnected(bool stopCalled)
        {
            Debug.WriteLine("#{0} - Event: {1}, Method: {2}", DateTime.Now.ToString("hh:mm:ss.fff"), "-", "NotificationHub.OnDisconnected");

            try
            {
                var currentUser = _userService.GetUserByUsername(Context.User.Identity.Name);
                var connectionId = Context.ConnectionId;
                var client = _clients.FirstOrDefault(c => c.ConnectionId == connectionId);
                if (client != null)
                {
                    _sessionService.CloseSession(client.User);

                    lock (_syncRoot)
                    {
                        Clients.Client(connectionId).disconnected();
                        _clients.Remove(client);
                    }
                }
            }
            catch (Exception ex) { _logger.Error(ex.Message, ex); }
            return base.OnDisconnected(true);
        }
        public override Task OnReconnected()
        {
            //Debug.WriteLine("#{0} - Event: {1}, Method: {2}", DateTime.Now.ToString("hh:mm:ss.fff"), "-", "NotificationHub.OnReconnected");

            var currentUser = _userService.GetUserByUsername(Context.User.Identity.Name);
            if (!currentUser.IsRegistered()) return null;

            var connectionId = Context.ConnectionId;
            try
            {
                var client = _clients.FirstOrDefault(c => c.User == currentUser);
                if (client != null)
                {
                    client.ConnectionId = connectionId;
                }
                else
                {
                    client = new Client() { ConnectionId = connectionId, User = currentUser };
                    _clients.Add(client);
                }
            }
            catch (Exception ex) { _logger.Error(ex.Message, ex); }
            
            lock (_syncRoot)
            {
                Clients.Client(connectionId).reconnected();
            }
            return base.OnReconnected();
        }
        public void Send(string connectionId, object message)
        {
            Clients.Client(connectionId).send(message, GetClient(connectionId).User.Username);
        }
        private Client GetClient(string connectionId)
        {
            return _clients.FirstOrDefault(c => c.ConnectionId == connectionId);
        }
        public IEnumerable<string> GetConnectedClients()
        {
            return _clients.Select(c => c.ConnectionId);
        }
        #endregion Methods

        #region Commented work
        // It's work
        //[HubMethodName("sendNotifications")]
        //public object SendNotifications()
        //{
        //    var client = GetClient(Context.ConnectionId);
        //    var count = 0;
        //    try
        //    {
        //        using (var connection = new SqlConnection(DataSettings.Current.DataConnectionString))
        //        {
        //            string query = string.Format("SELECT [Message], [DateOnUtc] FROM [dbo].[SignalR_] WHERE [UserGuid]='{0}'", client.User.UserGuid);
        //            connection.Open();
        //            using (SqlCommand command = new SqlCommand(query, connection))
        //            {
        //                try
        //                {
        //                    command.Notification = null;
        //                    DataTable dt = new DataTable();
        //                    SqlDependency dependency = new SqlDependency(command);
        //                    dependency.OnChange += new OnChangeEventHandler(OnDependencyChange);
        //                    if (connection.State == ConnectionState.Closed)
        //                        connection.Open();
        //                    var reader = command.ExecuteReader();
        //                    dt.Load(reader);
        //                    count = dt.Rows.Count;
        //                    if (dt.Rows.Count > 0)
        //                    {
        //                        //var totalNewMessages = Int16.Parse(dt.Rows[0]["NewMessageCount"].ToString());
        //                        //var totalNewCircles = Int16.Parse(dt.Rows[0]["NewCircleRequestCount"].ToString());
        //                        //var totalNewJobs = Int16.Parse(dt.Rows[0]["NewJobNotificationsCount"].ToString());
        //                        //var totalNewNotification = Int16.Parse(dt.Rows[0]["NewNotificationsCount"].ToString());
        //                    }
        //                    connection.Close();
        //                }
        //                catch (Exception ex)
        //                {
        //                    Clients.Client(client.ConnectionId).error(ex.Message, client.User.Username);
        //                }
        //            }
        //        }
        //        IHubContext context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
        //        return context.Clients.All.RecieveNotification(count, client != null ? client.User.Username : "unknown");
        //    }
        //    catch (Exception ex) 
        //    {
        //        Clients.Client(client.ConnectionId).error(ex.Message, client.User.Username);
        //    }
        //    return null;
        //}

        //[HubMethodName("sendNotifications")]
        //public string SendNotifications()
        //{
        //}
        #endregion Commented work

        #region Static Hub methods
        public static void Show()
        {
            Debug.WriteLine("#{0} - Event: {1}, Method: {2}", DateTime.Now.ToString("hh:mm:ss.fff"), "-", "NotificationHub.Show");

            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            context.Clients.All.displayStatus();
        }
        public static void SendNotifyToUser(int userId, object position)
        {
            Debug.WriteLine("#{0} - Event: {1}, Method: {2}", DateTime.Now.ToString("hh:mm:ss.fff"), "-", "NotificationHub.SendNotifyToUser");

            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            var client = _clients.FirstOrDefault(c => c.User.Id == userId);

            context.Clients.Client(client.ConnectionId).send(position, client.User.Username);
            //context.Clients.Client(connectionId).displayStatus();
        }

        public static void SendPositionToUser(int userId, object position)
        {
            if (!_clients.Any()) return;
            Debug.WriteLine("#{0} - Event: {1}, Method: {2}", DateTime.Now.ToString("hh:mm:ss.fff"), "-", "NotificationHub.SendPositionToUser");

            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            var client = _clients.FirstOrDefault(c => c.User.Id == userId);

            context.Clients.Client(client.ConnectionId).sendPosition(position, client.User.Username);
        }
        public static void SendIndicatorToUser(int userId, object indicator)
        {
            Debug.WriteLine("#{0} - Event: {1}, Method: {2}", DateTime.Now.ToString("hh:mm:ss.fff"), "-", "NotificationHub.SendIndicatorToUser");

            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            var client = _clients.FirstOrDefault(c => c.User.Id == userId);

            context.Clients.Client(client.ConnectionId).sendIndicator(indicator, client.User.Username);
        }
        #endregion Static Hub methods
    }

    public class Client
    {
        public string ConnectionId { get; set; }
        public User User { get; set; }
    }
}