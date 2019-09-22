namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationDecoratorBase : IAuthentication
    {
        private readonly IAuthentication _authentication;

        protected AuthenticationDecoratorBase(IAuthentication authentication)
        {
            _authentication = authentication;
        }

        public virtual bool Verify(string accountId, string inputPassword, string otp)
        {
            return _authentication.Verify(accountId, inputPassword, otp);
        }
    }

    public class NotificationDecorator: AuthenticationDecoratorBase
    {
        private readonly IAuthentication _authentication;
        private readonly INotification _notification;

        public NotificationDecorator(IAuthentication authentication, INotification notification) : base(authentication)
        {
            _authentication = authentication;
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
                Send(accountId);
            }

            return isValid;
        }
    }
}