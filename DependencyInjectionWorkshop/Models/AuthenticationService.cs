using System;
using System.Net.Http;
using DependencyInjectionWorkshop.Repos;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly IProfile _profileDao;
        private readonly IFailedCounter _failedCounter;
        private readonly IHash _sha256Adapter;
        private readonly IOtpService _otpService;
        private readonly INotification _slackAdapter;
        private readonly ILogger _logAdapter;


        public AuthenticationService(IProfile profileDao, IFailedCounter failedCounter, IHash sha256Adapter, IOtpService otpService, INotification slackAdapter, ILogger logAdapter)
        {
            _profileDao = profileDao;
            _failedCounter = failedCounter;
            _sha256Adapter = sha256Adapter;
            _otpService = otpService;
            _slackAdapter = slackAdapter;
            _logAdapter = logAdapter;
        }

        public AuthenticationService()
        {
            _failedCounter = new FailedCounter();
            _profileDao = new ProfileDao();
            _sha256Adapter = new Sha256Adapter();
            _otpService = new OtpService();
            _slackAdapter = new SlackAdapter();
            _logAdapter = new NLogAdapter();
        }

        public bool Verify(string accountId, string inputPassword, string otp)
        {
            var isLocked = _failedCounter.GetAccountIsLocked(accountId);
            if (isLocked)
            {
                throw new FailedTooManyTimesException();
            }
            
            var passwordFromDb = _profileDao.GetPassword(accountId);
            var hashedInputPassword = _sha256Adapter.Compute(inputPassword);
            var currentOtp = _otpService.GetCurrentOtp(accountId);

            if (passwordFromDb == hashedInputPassword && otp == currentOtp)
            {
                _failedCounter.ResetFailedCount(accountId);

                return true;
            }
            else
            {
                _failedCounter.AddFailedCount(accountId);

                var failedCount = _failedCounter.GetFailedCount(accountId);
                _logAdapter.Info($"accountId:{accountId} failed times:{failedCount}");
                _slackAdapter.Send(accountId);

                return false;
            } 
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}