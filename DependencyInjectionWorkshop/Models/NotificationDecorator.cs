using System.Net.Sockets;

namespace DependencyInjectionWorkshop.Models
{
    public class NotificationDecorator: AuthenticationDecoratorBase
    {
        private readonly INotification _notification;

        public NotificationDecorator(IAuthentication authentication, INotification notification): base
            (authentication)
        {
            _notification = notification;
        }

        private void Send(string accountId)
        {
            _notification.Send(accountId);
        }

        public override bool Verify(string accountId, string inputPassword, string otp)
        {
            bool isValid = base.Verify(accountId, inputPassword, otp);

            if (!isValid)
            {
               _notification.Send(accountId); 
            }

            return isValid;
        }
    }
}