using System;
using System.Threading.Tasks;
using Samples.Models;
using Shiny;
using Shiny.Notifications;


namespace Samples.ShinyDelegates
{
    public class NotificationDelegate : INotificationDelegate
    {
        readonly SampleSqliteConnection conn;
        readonly IMessageBus messageBus;


        public NotificationDelegate(SampleSqliteConnection conn, IMessageBus messageBus)
        {
            this.conn = conn;
            this.messageBus = messageBus;
        }


        public Task OnEntry(Notification notification)
            => this.Store(notification, true);
        public Task OnReceived(Notification notification)
            => this.Store(notification, false);


        async Task Store(Notification notification, bool isEntry)
        {
            var not = new NotificationEvent
            {
                NotificationId = notification.Id,
                NotificationTitle = notification.Title ?? notification.Message,
                IsEntry = isEntry,
                Timestamp = DateTime.Now
            };
            await this.conn.InsertAsync(not);
            this.messageBus.Publish(not);
        }
    }
}
