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
            CheckAccountIsLocked(accountId);
            
            bool isValid = base.Verify(accountId, inputPassword, otp);

            if (isValid)
            {
                ResetFailedCount(accountId);
            }
            else
            {
                AddFailedCount(accountId);
            }

            return isValid;
        }

        private void AddFailedCount(string accountId)
        {
            _failedCounter.AddFailedCount(accountId);
        }

        private void ResetFailedCount(string accountId)
        {
            _failedCounter.ResetFailedCount(accountId);
        }

        private void CheckAccountIsLocked(string accountId)
        {
            var isLocked = _failedCounter.GetAccountIsLocked(accountId);
            if (isLocked)
            {
                throw new FailedTooManyTimesException();
            }
        }
    }
}