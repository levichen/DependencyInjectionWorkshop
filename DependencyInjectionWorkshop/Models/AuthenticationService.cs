using DependencyInjectionWorkshop.Repositories;
using System;

namespace DependencyInjectionWorkshop.Models
{
    public class LogFailedCountDecorator : AuthenticationBaseDecorator
    {
        private readonly IFailedCounter _failedCounter;
        private readonly ILogger _logger;

        public LogFailedCountDecorator(IAuthentication authenticationService, ILogger logger,
            IFailedCounter failedCounter) : base(authenticationService)
        {
            _logger = logger;
            _failedCounter = failedCounter;
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            var isValid = base.Verify(accountId, password, otp);
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

    public class AuthenticationService : IAuthentication
    {
        private readonly IHash _hash;
        private readonly IOtpService _otpService;
        private readonly IProfile _profile;

        public AuthenticationService(IOtpService otpService, IProfile profile, IHash hash)
        {
            _otpService = otpService;
            _profile = profile;
            _hash = hash;
        }

        public AuthenticationService()
        {
            _profile = new ProfileDao();
            _hash = new Sha256Adapter();
            _otpService = new OtpService();
            new FailedCounter();
            new NLogAdapter();
        }

        public bool Verify(string accountId, string password, string otp)
        {
            var passwordFromDb = _profile.GetPassword(accountId);

            var hashedPassword = _hash.Compute(password);

            var currentOtp = _otpService.GetCurrentOtp(accountId);

            if (passwordFromDb == hashedPassword && otp == currentOtp)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}