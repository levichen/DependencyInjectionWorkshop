using System;
using DependencyInjectionWorkshop.Repos;

namespace DependencyInjectionWorkshop.Models
{
    public interface IAuthentication
    {
        [AuditLog]
        bool Verify(string accountId, string inputPassword, string otp);
    }

    public class AuditLogAttribute : Attribute
    {
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
        }

        public bool Verify(string accountId, string inputPassword, string otp)
        {
            var passwordFromDb = _profile.GetPassword(accountId);
            var hashedInputPassword = _hash.Compute(inputPassword);
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