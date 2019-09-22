namespace DependencyInjectionWorkshop.Models
{
    public class LogMethodInfoDecorator: AuthenticationDecoratorBase
    {
        private readonly ILogger _logger;

        public LogMethodInfoDecorator(IAuthentication authentication, ILogger logger) : base(authentication)
        {
            _logger = logger;
        }

        public override bool Verify(string accountId, string inputPassword, string otp)
        {
            _logger.Info($"account: {accountId}, password: {inputPassword}, otp: {otp}");
            
            bool isValid = base.Verify(accountId, inputPassword, otp);

            _logger.Info($"Verify Result: {isValid}");
            return isValid;
        }
    }
}