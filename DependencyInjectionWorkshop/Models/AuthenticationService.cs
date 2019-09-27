using System;
using System.Net.Http;
using DependencyInjectionWorkshop.Repos;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IProfile _profileDao;
        private readonly IHash _sha256Adapter;
        private readonly IOtpService _otpService;

        public AuthenticationService(IProfile profileDao, IHash sha256Adapter, IOtpService otpService)
        {
            _profileDao = profileDao;
            _sha256Adapter = sha256Adapter;
            _otpService = otpService;
        }

        public bool Verify(string accountId, string inputPassword, string otp)
        {
            var passwordFromDb = _profileDao.GetPassword(accountId);
            var hashedInputPassword = _sha256Adapter.Compute(inputPassword);
            var currentOtp = _otpService.GetCurrentOtp(accountId);

            if (passwordFromDb == hashedInputPassword && otp == currentOtp)
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