// <copyright file="UpgradeRequestsRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DrinkDb_Auth.Repository.AdminDashboard
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using DataAccess.Model.AdminDashboard;
    using IRepository;
    using Microsoft.Data.SqlClient;

    public class UpgradeRequestsRepository : IUpgradeRequestsRepository
    {
        private const string SELECTALLUPGRADEREQUESTSQUERY = "SELECT RequestId, RequestingUserId, RequestingUserName FROM UpgradeRequests";
        private const string SELECTUPGRADEREQUESTBYIDENTIFIERQUERY = "SELECT RequestId, RequestingUserId, RequestingUserName FROM UpgradeRequests WHERE RequestId = @upgradeRequestIdentifier";
        private const string DELETEUPGRADEREQUESTQUERY = "DELETE FROM UpgradeRequests WHERE RequestId=@upgradeRequestIdentifier";

        private readonly IDbConnectionFactory connectionFactory;

        public UpgradeRequestsRepository(IDbConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        // Legacy constructor for backward compatibility
        public UpgradeRequestsRepository(string databaseConnectionString)
            : this(new SqlConnectionFactory(databaseConnectionString))
        {
        }

        public List<UpgradeRequest> RetrieveAllUpgradeRequests()
        {
            List<UpgradeRequest> upgradeRequestsList = new List<UpgradeRequest>();

            using (var connection = connectionFactory.CreateConnection())
            {
                try
                {
                    connection.Open();
                    using var command = new SqlCommand(SELECTALLUPGRADEREQUESTSQUERY, connection);
                    using var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        UpgradeRequest upgradeRequest = new UpgradeRequest(
                            reader.GetInt32(0),
                            Guid.NewGuid(),
                            reader.GetString(2));

                        upgradeRequestsList.Add(upgradeRequest);
                    }
                }
                catch (Exception databaseException)
                {
                    Console.WriteLine("Database Error: " + databaseException.Message);
                }
            }

            return upgradeRequestsList;
        }

        public void RemoveUpgradeRequestByIdentifier(int upgradeRequestIdentifier)
        {
            using (var connection = connectionFactory.CreateConnection())
            {
                try
                {
                    connection.Open();
                    using var command = new SqlCommand(DELETEUPGRADEREQUESTQUERY, connection);
                    command.Parameters.AddWithValue("@upgradeRequestIdentifier", upgradeRequestIdentifier);
                    command.ExecuteNonQuery();
                }
                catch (Exception databaseException)
                {
                    Console.WriteLine("Database Error: " + databaseException.Message);
                }
            }
        }

        public UpgradeRequest RetrieveUpgradeRequestByIdentifier(int upgradeRequestIdentifier)
        {
            UpgradeRequest? retrievedUpgradeRequest = null;

            using (var connection = connectionFactory.CreateConnection())
            {
                try
                {
                    connection.Open();
                    using var command = new SqlCommand(SELECTUPGRADEREQUESTBYIDENTIFIERQUERY, connection);
                    command.Parameters.AddWithValue("@upgradeRequestIdentifier", upgradeRequestIdentifier);

                    using var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        retrievedUpgradeRequest = new UpgradeRequest(
                            reader.GetInt32(0),
                            Guid.NewGuid(),
                            reader.GetString(2));
                    }
                }
                catch (Exception databaseException)
                {
                    Console.WriteLine("Database Error: " + databaseException.Message);
                }
            }

            return retrievedUpgradeRequest;
        }
    }
}