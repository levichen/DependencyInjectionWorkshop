namespace DependencyInjectionWorkshop.Models
{
    public class LogFailedCountDecorator: AuthenticationDecoratorBase
    {
        private readonly IFailedCounter _failedCount;
        private readonly ILogger _logger;

        public LogFailedCountDecorator(IAuthentication authentication, IFailedCounter failedCount, ILogger logger) : base(authentication)
        {
            _failedCount = failedCount;
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
            var failedCount = _failedCount.GetFailedCount(accountId);
            _logger.Info($"accountId:{accountId} failed times:{failedCount}");
        }
    }
}