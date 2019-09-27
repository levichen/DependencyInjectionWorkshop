﻿using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using DependencyInjectionWorkshop.Models;

namespace DependencyInjectionWorkshop.Repositories
{
    public interface IProfile
    {
        [AuditLog]
        string GetPassword(string accountId);
    }

    public class ProfileDao : IProfile
    {
        public string GetPassword(string accountId)
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
}