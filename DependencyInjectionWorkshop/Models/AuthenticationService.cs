using System;
using System.Net.Http;
using DependencyInjectionWorkshop.Repos;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IProfile _profileDao;
        private readonly IFailedCounter _failedCounter;
        private readonly IHash _sha256Adapter;
        private readonly IOtpService _otpService;
        private readonly ILogger _logAdapter;

        public AuthenticationService(IProfile profileDao, IFailedCounter failedCounter, IHash sha256Adapter, IOtpService otpService, ILogger logAdapter)
        {
            _profileDao = profileDao;
            _failedCounter = failedCounter;
            _sha256Adapter = sha256Adapter;
            _otpService = otpService;
            _logAdapter = logAdapter;
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

                return false;
            }
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}