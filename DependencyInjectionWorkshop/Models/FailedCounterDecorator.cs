namespace DependencyInjectionWorkshop.Models
{
    public class FailedCounterDecorator: AuthenticationDecoratorBase
    {
        private readonly IFailedCounter _failedCounter;

        public FailedCounterDecorator(IAuthentication authenticationService, IFailedCounter failedCounter): base(authenticationService)
        {
            _failedCounter = failedCounter;
        }

        public override bool Verify(string accountId, string inputPassword, string otp)
        {
            bool isValid = base.Verify(accountId, inputPassword, otp);
            
            if (isValid)
            {
                _failedCounter.ResetFailedCount(accountId);
            }

            return isValid;
        }
    }
}