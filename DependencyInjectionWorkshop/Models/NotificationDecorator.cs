namespace DependencyInjectionWorkshop.Models
{
    public class NotificationDecorator: IAuthentication
    {
        private readonly IAuthentication _authenticationService;
        private readonly INotification _notification;

        public NotificationDecorator(IAuthentication authenticationService, INotification notification)
        {
            _authenticationService = authenticationService;
            _notification = notification;
        }

        private void Send(string accountId)
        {
            _notification.Send(accountId);
        }

        public bool Verify(string accountId, string inputPassword, string otp)
        {
            bool isValid = _authenticationService.Verify(accountId, inputPassword, otp);

            if (!isValid)
            {
                Send(accountId);
            }

            return isValid;
        }
    }
}