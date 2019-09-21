using DependencyInjectionWorkshop.Repositories;
using SlackAPI;
using System;
using System.Net.Http;
using System.Text;

namespace DependencyInjectionWorkshop.Models
{
    public class Sha256Adapter
    {
        public Sha256Adapter()
        {
        }

        public string GetHashedPassword(string password)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            var hashedPassword = hash.ToString();
            return hashedPassword;
        }
    }

    public class OtpService
    {
        public OtpService()
        {
        }

        public string GetCurrentOtp(string accountId)
        {
            var response = new HttpClient() {BaseAddress = new Uri("http://joey.com/")}
                           .PostAsJsonAsync("api/otps", accountId).Result;
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
        public SlackAdapter()
        {
        }

        public void Notify(string accountId)
        {
            string message = $"{accountId} try to login failed";
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(response1 => { }, "my channel", message, "my bot name");
        }
    }

    public class FailedCounterForAbTesting : FailedCounter
    {
        public FailedCounterForAbTesting()
        {
            JoeyClient = new HttpClient() {BaseAddress = new Uri("http://joey.com/")};
        }
    }

    public class FailedCounter
    {
        protected HttpClient JoeyClient { get; set; } = new HttpClient() {BaseAddress = new Uri("http://joey.com/")};

        public FailedCounter()
        {
        }

        public void AddFailedCount(string accountId)
        {
            var addFailedCountResponse = JoeyClient
                                         .PostAsJsonAsync("api/failedCounter/Add", accountId).Result;
            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        public bool GetAccountIsLocked(string accountId)
        {
            var isLockedResponse = JoeyClient
                                   .PostAsJsonAsync("api/failedCounter/IsLocked", accountId).Result;

            isLockedResponse.EnsureSuccessStatusCode();
            var isLocked = isLockedResponse.Content.ReadAsAsync<bool>().Result;
            return isLocked;
        }

        public int GetFailedCount(string accountId)
        {
            var failedCountResponse =
                JoeyClient
                    .PostAsJsonAsync("api/failedCounter/GetFailedCount", accountId).Result;
            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }

        public void ResetFailedCount(string accountId)
        {
            var resetResponse = JoeyClient
                                .PostAsJsonAsync("api/failedCounter/Reset", accountId).Result;
            resetResponse.EnsureSuccessStatusCode();
        }
    }

    public class NLogAdapter
    {
        public NLogAdapter()
        {
        }

        public void LogMessage(string message)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }
    }

    public class AuthenticationService
    {
        private readonly FailedCounter _failedCounter;
        private readonly NLogAdapter _nLogAdapter;
        private readonly OtpService _otpService;
        private readonly IProfile _profile;
        private readonly Sha256Adapter _sha256Adapter;
        private readonly SlackAdapter _slackAdapter;

        public AuthenticationService(FailedCounter failedCounter, NLogAdapter nLogAdapter, OtpService otpService,
            IProfile profile, Sha256Adapter sha256Adapter, SlackAdapter slackAdapter)
        {
            _failedCounter = failedCounter;
            _nLogAdapter = nLogAdapter;
            _otpService = otpService;
            _profile = profile;
            _sha256Adapter = sha256Adapter;
            _slackAdapter = slackAdapter;
        }

        public AuthenticationService()
        {
            _profile = new ProfileDao();
            _sha256Adapter = new Sha256Adapter();
            _otpService = new OtpService();
            _slackAdapter = new SlackAdapter();
            _failedCounter = new FailedCounter();
            _nLogAdapter = new NLogAdapter();
        }

        public bool Verify(string accountId, string password, string otp)
        {
            var isLocked = _failedCounter.GetAccountIsLocked(accountId);
            if (isLocked)
            {
                throw new FailedTooManyTimesException();
            }

            var passwordFromDb = _profile.GetPassword(accountId);

            var hashedPassword = _sha256Adapter.GetHashedPassword(password);

            var currentOtp = _otpService.GetCurrentOtp(accountId);

            if (passwordFromDb == hashedPassword && otp == currentOtp)
            {
                _failedCounter.ResetFailedCount(accountId);

                return true;
            }
            else
            {
                _failedCounter.AddFailedCount(accountId);

                var failedCount = _failedCounter.GetFailedCount(accountId);
                _nLogAdapter.LogMessage($"accountId:{accountId} failed times:{failedCount}");

                _slackAdapter.Notify(accountId);

                return false;
            }
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}