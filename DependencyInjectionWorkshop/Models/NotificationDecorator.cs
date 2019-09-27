namespace DependencyInjectionWorkshop.Models
{
    public class NotificationDecorator: IAuthenticationService
    {
        private readonly IAuthenticationService _authentication;
        private readonly INotification _notification;

        public NotificationDecorator(IAuthenticationService authentication, INotification notification)
        {
            _notification = notification;
            _authentication = authentication;
        }

        private void Send(string accountId)
        {
            _notification.Send(accountId);
        }

        public bool Verify(string accountId, string inputPassword, string otp)
        {
            var isValid = _authentication.Verify(accountId, inputPassword, otp);

            if (!isValid)
            {
                Send(accountId);
            }

            return isValid;
        }
    }
}