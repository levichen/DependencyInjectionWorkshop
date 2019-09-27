namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationDecoratorBase : IAuthenticationService
    {
        private readonly IAuthenticationService _authentication;

        protected AuthenticationDecoratorBase(IAuthenticationService authentication)
        {
            _authentication = authentication;
        }

        public virtual bool Verify(string accountId, string inputPassword, string otp)
        {
            return _authentication.Verify(accountId, inputPassword, otp);
        }
    }
}