using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public interface INotification
    {
        void Send(string accountId);
    }

    public class SlackAdapter : INotification
    {
        public void Send(string accountId)
        {
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(response1 => { }, "my channel", $"{accountId} try to login failed", "my bot name");
        }
    }
}