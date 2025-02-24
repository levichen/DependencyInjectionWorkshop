using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public interface IFailedCounter
    {
        void ResetFailedCount(string accountId);
        void AddFailedCount(string accountId);
        bool GetAccountIsLocked(string accountId);
        int GetFailedCount(string accountId);
    }

    public class FailedCounter : IFailedCounter
    {
        public void ResetFailedCount(string accountId)
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
}