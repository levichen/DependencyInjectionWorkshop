namespace DependencyInjectionWorkshop.Models
{
    public class FailedCounterDecorator: AuthenticationDecoratorBase
    {
        private readonly IFailedCounter _failedCounter;

        public FailedCounterDecorator(IAuthentication authentication, IFailedCounter failedCounter): base(authentication)
        {
            _failedCounter = failedCounter;
        }

        private void Reset(string accountId)
        {
            _failedCounter.ResetFailedCount(accountId);
        }
        

        public override bool Verify(string accountId, string inputPassword, string otp)
        {
            CheckAccountIsLocked(accountId);
            bool isValid = base.Verify(accountId, inputPassword, otp);

            if (isValid)
            {
                Reset(accountId);
            }
            else
            {
                AddFailedCounter(accountId);
            }

            return isValid;
        }

        private void AddFailedCounter(string accountId)
        {
            _failedCounter.AddFailedCount(accountId);
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