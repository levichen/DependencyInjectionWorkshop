namespace DependencyInjectionWorkshop.Models
{
    public class NotificationDecorator: IAuthentication
    {
        private readonly IAuthentication _authentication;
        private readonly INotification _notification;

        public NotificationDecorator(IAuthentication authentication, INotification notification)
        {
            _authentication = authentication;
            _notification = notification;
        }

        private void Send(string accountId)
        {
            _notification.Send(accountId);
        }

        public bool Verify(string accountId, string inputPassword, string otp)
        {
            bool isValid = _authentication.Verify(accountId, inputPassword, otp);

            if (!isValid)
            {
                Send(accountId);
            }

            return isValid;
        }
    }
}