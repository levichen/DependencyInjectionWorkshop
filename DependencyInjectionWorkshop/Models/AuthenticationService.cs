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
    public class AuthenticationService
    {
        public bool Verify(string accountId, string inputPassword, string otp)
        {
            string passwordFromDb;

            // Get Password
            using (var connection = new SqlConnection("my connection string"))
            {
                passwordFromDb = connection.Query<string>("spGetUserPassword", new {Id = accountId},
                     commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
            
            // GetHash
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(inputPassword));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            var hashedInputPassword = hash.ToString();
            
            // Get OTP
            var httpClient = new HttpClient() {BaseAddress = new Uri("http://joey.com/")};
            var response = httpClient.PostAsJsonAsync("api/otps", accountId).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"web api error, accountId:{accountId}");
            }
            
            var currentOtp = response.Content.ReadAsAsync<string>().Result;

            if (passwordFromDb == hashedInputPassword && otp == currentOtp)
            {
                var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", accountId).Result;
                resetResponse.EnsureSuccessStatusCode();
                
                return true;
            }
            else
            {
                var slackClient = new SlackClient("my api token");
                slackClient.PostMessage(response1 => { }, "my channel", $"{accountId} try to login failed", "my bot name");
                
                var addFailedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add", accountId).Result;
                addFailedCountResponse.EnsureSuccessStatusCode();
                
                return false;
            } 
        }
    }
}