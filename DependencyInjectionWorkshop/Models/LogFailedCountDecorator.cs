namespace DependencyInjectionWorkshop.Models
{
    public class LogFailedCountDecorator: AuthenticationDecoratorBase
    {
        private readonly IFailedCounter _failedCounter;
        private readonly ILogger _logger;

        public LogFailedCountDecorator(IAuthenticationService authenticationService, IFailedCounter failedCounter, ILogger logger): base(authenticationService)
        {
            _failedCounter = failedCounter;
            _logger = logger;
        }

        public override bool Verify(string accountId, string inputPassword, string otp)
        {
            bool isValid = base.Verify(accountId, inputPassword, otp);

            if (!isValid)
            {
               LogFailedCount(accountId); 
            }

            return isValid;
        }

        private void LogFailedCount(string accountId)
        {
            var failedCount = _failedCounter.GetFailedCount(accountId);
            _logger.Info($"accountId:{accountId} failed times:{failedCount}");
        }
    }
}