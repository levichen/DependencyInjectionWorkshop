using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using Dapper;
using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class ProfileDao
    {
        public string GetPasswordFromDb(string accountId)
        {
            string passwordFromDb;

            using (var connection = new SqlConnection("my connection string"))
            {
                passwordFromDb = connection.Query<string>("spGetUserPassword", new {Id = accountId},
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            return passwordFromDb;
        }
    }
    
    public class Sha256Adapter
    {
        public string GetHashedInputPassword(string inputPassword)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(inputPassword));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            var hashedInputPassword = hash.ToString();
            return hashedInputPassword;
        }
    }


    public class OtpService
    {
        public string GetCurrentOtp(string accountId)
        {
            var response = new HttpClient() {BaseAddress = new Uri("http://joey.com/")}.PostAsJsonAsync("api/otps", accountId).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"web api error, accountId:{accountId}");
            }

            var currentOtp = response.Content.ReadAsAsync<string>().Result;
            return currentOtp;
        }
    }


    public class SlackAdapter
    {
        public void Notify(string accountId)
        {
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(response1 => { }, "my channel", $"{accountId} try to login failed", "my bot name");
        }
    }
    
    public class FailedCounter
    {
        public void ResetFailedCounter(string accountId)
        {
            var resetResponse = new HttpClient() {BaseAddress = new Uri("http://joey.com/")}.PostAsJsonAsync("api/failedCounter/Reset", accountId).Result;
            resetResponse.EnsureSuccessStatusCode();
        }

        public void AddFailedCount(string accountId)
        {
            var addFailedCountResponse = new HttpClient() {BaseAddress = new Uri("http://joey.com/")}.PostAsJsonAsync("api/failedCounter/Add", accountId).Result;
            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        public bool GetAccountIsLocked(string accountId)
        {
            var isLockedResponse = new HttpClient() {BaseAddress = new Uri("http://joey.com/")}.PostAsJsonAsync("api/failedCounter/IsLocked", accountId).Result;
            isLockedResponse.EnsureSuccessStatusCode();
            bool isLocked = isLockedResponse.Content.ReadAsAsync<bool>().Result;
            return isLocked;
        }

        public int GetFailedCount(string accountId)
        {
            var failedCountResponse =
                new HttpClient() {BaseAddress = new Uri("http://joey.com/")}.PostAsJsonAsync("api/failedCounter/GetFailedCount", accountId).Result;

            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }
    }

    public class AuthenticationService
    {
        private readonly ProfileDao _profileDao;
        private readonly Sha256Adapter _sha256Adapter;
        private readonly OtpService _otpService;
        private readonly SlackAdapter _slackAdapter;
        private readonly FailedCounter _failedCounter;

        public AuthenticationService()
        {
            _profileDao = new ProfileDao();
            _sha256Adapter = new Sha256Adapter();
            _otpService = new OtpService();
            _slackAdapter = new SlackAdapter();
            _failedCounter = new FailedCounter();
        }

        public bool Verify(string accountId, string inputPassword, string otp)
        {
            // check is lock before verify
            var isLocked = _failedCounter.GetAccountIsLocked(accountId);
            if (isLocked)
            {
                throw new FailedTooManyTimesException();
            }
            
            var passwordFromDb = _profileDao.GetPasswordFromDb(accountId);
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
            
            LogMessage($"accountId:{accountId} failed times:{failedCount}");
        }

        private static void LogMessage(string message)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }
    }


    public class FailedTooManyTimesException : Exception
    {
    }
}