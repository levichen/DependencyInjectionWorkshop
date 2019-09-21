using System;
using DependencyInjectionWorkshop.Repos;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly IProfile _profile;
        private readonly Sha256Adapter _sha256Adapter;
        private readonly OtpService _otpService;
        private readonly SlackAdapter _slackAdapter;
        private readonly FailedCounter _failedCounter;
        private readonly NLogAdapter _nlogAdapter;

        public AuthenticationService(IProfile profile, Sha256Adapter sha256Adapter, OtpService otpService, SlackAdapter slackAdapter, FailedCounter failedCounter, NLogAdapter nlogAdapter)
        {
            _profile = profile;
            _sha256Adapter = sha256Adapter;
            _otpService = otpService;
            _slackAdapter = slackAdapter;
            _failedCounter = failedCounter;
            _nlogAdapter = nlogAdapter;
        }

        public AuthenticationService()
        {
            _profile = new ProfileDao();
            _sha256Adapter = new Sha256Adapter();
            _otpService = new OtpService();
            _slackAdapter = new SlackAdapter();
            _failedCounter = new FailedCounter();
            _nlogAdapter = new NLogAdapter();
        }

        public bool Verify(string accountId, string inputPassword, string otp)
        {
            // check is lock before verify
            var isLocked = _failedCounter.GetAccountIsLocked(accountId);
            if (isLocked)
            {
                throw new FailedTooManyTimesException();
            }
            
            var passwordFromDb = _profile.GetPassword(accountId);
            var hashedInputPassword = _sha256Adapter.GetHashedInputPassword(inputPassword);
            var currentOtp = _otpService.GetCurrentOtp(accountId);

            if (passwordFromDb == hashedInputPassword && otp == currentOtp)
            {
                // login success, reset failed counter
                _failedCounter.ResetFailedCounter(accountId);

                return true;
            }
            else
            {
                _failedCounter.AddFailedCount(accountId);
                LogFailedCount(accountId);
                _slackAdapter.Notify(accountId);

                return false;
            } 
        }

        private void LogFailedCount(string accountId)
        {
            var failedCount = _failedCounter.GetFailedCount(accountId);
            
            _nlogAdapter.LogMessage($"accountId:{accountId} failed times:{failedCount}");
        }
    }



    public class FailedTooManyTimesException : Exception
    {
    }
}