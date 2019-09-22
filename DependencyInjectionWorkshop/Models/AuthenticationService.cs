using DependencyInjectionWorkshop.Repositories;
using System;

namespace DependencyInjectionWorkshop.Models
{
    public interface IAuthentication
    {
        bool Verify(string accountId, string password, string otp);
    }

    public class AuthenticationService : IAuthentication
    {
        private readonly IFailedCounter _failedCounter;
        //private readonly FailedCounterDecorator _failedCounterDecorator;
        private readonly IHash _hash;
        private readonly ILogger _logger;
        private readonly IOtpService _otpService;
        private readonly IProfile _profile;

        public AuthenticationService(IFailedCounter failedCounter, ILogger logger, IOtpService otpService,
            IProfile profile, IHash hash)
        {
            _failedCounter = failedCounter;
            _logger = logger;
            _otpService = otpService;
            _profile = profile;
            _hash = hash;
        }

        public AuthenticationService()
        {
            _profile = new ProfileDao();
            _hash = new Sha256Adapter();
            _otpService = new OtpService();
            _failedCounter = new FailedCounter();
            _logger = new NLogAdapter();
        }

        public bool Verify(string accountId, string password, string otp)
        {
            var isLocked = _failedCounter.GetAccountIsLocked(accountId);
            if (isLocked)
            {
                throw new FailedTooManyTimesException();
            }

            var passwordFromDb = _profile.GetPassword(accountId);

            var hashedPassword = _hash.Compute(password);

            var currentOtp = _otpService.GetCurrentOtp(accountId);

            if (passwordFromDb == hashedPassword && otp == currentOtp)
            {
                return true;
            }
            else
            {
                _failedCounter.AddFailedCount(accountId);

                var failedCount = _failedCounter.GetFailedCount(accountId);
                _logger.Info($"accountId:{accountId} failed times:{failedCount}");

                return false;
            }
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}