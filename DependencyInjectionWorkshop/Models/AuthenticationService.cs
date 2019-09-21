using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using Dapper;

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

            return passwordFromDb == hashedInputPassword && otp == currentOtp;
        }
    }
}