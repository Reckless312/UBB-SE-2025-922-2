using System;
using System.Data;
using DrinkDb_Auth.Model;
using Microsoft.Data.SqlClient;

namespace DrinkDb_Auth.Adapter
{
    public class SessionAdapter : ISessionAdapter
    {
        public Session CreateSession(Guid userId)
        {
            using SqlConnection databaseConnection = DrinkDbConnectionHelper.GetConnection();
            using SqlCommand createSessionCommand = new SqlCommand("create_session", databaseConnection);
            createSessionCommand.CommandType = CommandType.StoredProcedure;

            createSessionCommand.Parameters.Add("@userId", SqlDbType.UniqueIdentifier).Value = userId;
            SqlParameter sessionIdParameter = createSessionCommand.Parameters.Add("@sessionId", SqlDbType.UniqueIdentifier);
            sessionIdParameter.Direction = ParameterDirection.Output;

            createSessionCommand.ExecuteNonQuery();

            Guid sessionId = (Guid)sessionIdParameter.Value;
            return Session.CreateSessionWithIds(sessionId, userId);
        }

        public bool EndSession(Guid sessionId)
        {
            using SqlConnection databaseConnection = DrinkDbConnectionHelper.GetConnection();
            using SqlCommand endSessionCommand = new SqlCommand("end_session", databaseConnection);
            endSessionCommand.CommandType = CommandType.StoredProcedure;
            endSessionCommand.Parameters.Add("@sessionId", SqlDbType.UniqueIdentifier).Value = sessionId;

            SqlParameter returnValue = endSessionCommand.Parameters.Add("@RETURN_VALUE", SqlDbType.Int);
            returnValue.Direction = ParameterDirection.ReturnValue;
            endSessionCommand.ExecuteNonQuery();
            return (int)returnValue.Value > 0;
        }

        public Session GetSession(Guid sessionId)
        {
            using SqlConnection databaseConnection = DrinkDbConnectionHelper.GetConnection();
            using SqlCommand getSessionCommand = new SqlCommand("SELECT userId FROM Sessions WHERE sessionId = @sessionId", databaseConnection);
            getSessionCommand.Parameters.Add("@sessionId", SqlDbType.UniqueIdentifier).Value = sessionId;
            using SqlDataReader reader = getSessionCommand.ExecuteReader();
            if (reader.Read())
            {
                int firstColumn = 0;
                return Session.CreateSessionWithIds(sessionId, reader.GetGuid(firstColumn));
            }
            throw new Exception("Session not found.");
        }

        public Session GetSessionByUserId(Guid userId)
        {
            using SqlConnection databaseConnection = DrinkDbConnectionHelper.GetConnection();
            using SqlCommand getSessionCommand = new SqlCommand("SELECT sessionId FROM Sessions WHERE userId = @userId", databaseConnection);
            getSessionCommand.Parameters.Add("@userId", SqlDbType.UniqueIdentifier).Value = userId;
            using SqlDataReader reader = getSessionCommand.ExecuteReader();
            if (reader.Read())
            {
                int firstColumn = 0;
                return Session.CreateSessionWithIds(reader.GetGuid(firstColumn), userId);
            }
            throw new Exception("Session not found.");
        }
    }
}