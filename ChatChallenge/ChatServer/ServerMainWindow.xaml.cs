using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;
using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using ChatDatabase;

namespace ChatServer
{
    /// <summary>
    /// WPF host for a SignalR server. The host can stop and start the SignalR
    /// server, report errors when trying to start the server on a URI where a
    /// server is already being hosted, and monitor when clients connect and disconnect. 
    /// The hub used in this server is a simple echo service, and has the same 
    /// functionality as the other hubs in the SignalR Getting Started tutorials.
    /// For simplicity, MVVM will not be used for this sample.
    /// </summary>
    public partial class ServerMainWindow : Window
    {
        public IDisposable SignalR { get; set; }
        const string ServerURI = "http://localhost:8080";

        public ServerMainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Calls the StartServer method with Task.Run to not
        /// block the UI thread. 
        /// </summary>
        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            WriteToConsole("Starting server...");
            ButtonStart.IsEnabled = false;
            Task.Run(() => StartServer());
        }

        /// <summary>
        /// Stops the server and closes the form. Restart functionality omitted
        /// for clarity.
        /// </summary>
        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            SignalR.Dispose();
            Close();
        }

        /// <summary>
        /// Starts the server and checks for error thrown when another server is already 
        /// running. This method is called asynchronously from Button_Start.
        /// </summary>
        private void StartServer()
        {
            try
            {
                SignalR = WebApp.Start(ServerURI);
            }
            catch (TargetInvocationException)
            {
                WriteToConsole("A server is already running at " + ServerURI);
                Dispatcher.Invoke(() => ButtonStart.IsEnabled = true);
                return;
            }
            Dispatcher.Invoke(() => ButtonStop.IsEnabled = true);
            WriteToConsole("Server started at " + ServerURI);
        }

        ///This method adds a line to the RichTextBoxConsole control, using Dispatcher.Invoke if used
        /// from a SignalR hub thread rather than the UI thread.
        public void WriteToConsole(string message)
        {
            if (!(RichTextBoxConsole.CheckAccess()))
            {
                Dispatcher.Invoke(() =>
                    WriteToConsole(message)
                );
                return;
            }
            RichTextBoxConsole.AppendText(message + "\r");
        }
    }

    /// <summary>
    /// Used by OWIN's startup process. 
    /// </summary>
    class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR();
        }
    }

    /// <summary>
    /// Echoes messages sent using the Send message by calling the
    /// addMessage method on the client. Also reports to the console
    /// when clients connect and disconnect.
    /// </summary>
    public class MyHub : Hub
    {
        private static readonly ConcurrentDictionary<string, List<string>> userConnections = 
            new ConcurrentDictionary<string, List<string>>();

        public void Send(string name, string message)
        {
            // unused -- was an earlier iteration of the user to user message
            Clients.All.addMessage(name, message);
        }

        public void Send(string source, string target, string message)
        {
            List<string> sourceConnections;
            List<string> targetConnections;
            userConnections.TryGetValue(source, out sourceConnections);
            userConnections.TryGetValue(target, out targetConnections);
            
            if (sourceConnections != null)
            {
                Clients.Clients(sourceConnections).addMessage(source, message);
                //ChatHistoryService.LogMessage(source, message);
            }                

            if (targetConnections != null)
                Clients.Clients(targetConnections).addMessage(source, message);
            else
                Clients.Clients(sourceConnections).addMessage(source, "<Failed to find user>");
        }

        public void FetchHistory(string name)
        {
            List<string> nameConnections;
            userConnections.TryGetValue(name, out nameConnections);

            if (nameConnections != null)
            {
                List<ChatRecord> chatHistory = ChatHistoryService.FetchChatHistory(name);
                if (chatHistory != null)
                {
                    chatHistory = chatHistory.OrderBy(cr => cr.TimeStamp).ToList();
                    foreach(var record in chatHistory)
                        Clients.Clients(nameConnections).addMessage(record.User, record.Message);
                }
                    
            }
        }

        public override Task OnConnected()
        {
            var connections = userConnections.GetOrAdd(Context.QueryString["user"], new List<string>());
            lock (connections)
            {
                connections.Add(Context.ConnectionId);
            }

            //Use Application.Current.Dispatcher to access UI thread from outside the MainWindow class
            Application.Current.Dispatcher.Invoke(() =>
                ((ServerMainWindow)Application.Current.MainWindow).WriteToConsole("Client connected: " + Context.ConnectionId));

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            List<string> connections;
            string user = Context.QueryString["user"];
            userConnections.TryGetValue(user, out connections);

            if (connections != null)
            {
                lock (connections)
                {
                    connections.Remove(Context.ConnectionId);

                    if (!connections.Any())
                    {
                        List<string> removedConnections;
                        userConnections.TryRemove(user, out removedConnections);
                    }
                }
                
            }

            //Use Application.Current.Dispatcher to access UI thread from outside the MainWindow class
            Application.Current.Dispatcher.Invoke(() =>
                ((ServerMainWindow)Application.Current.MainWindow).WriteToConsole("Client disconnected: " + Context.ConnectionId));

            return base.OnDisconnected(stopCalled);
        }

    }
}
