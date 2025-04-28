// <copyright file="OffensiveWordsRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace App1.AutoChecker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using App1.Infrastructure;
    using Microsoft.Data.SqlClient;

    public class OffensiveWordsRepository : IOffensiveWordsRepository
    {
        private const string SELECTOFFENSIVEWORDSQUERY = "SELECT Word FROM OffensiveWords";
        private const string INSERTOFFENSIVEWORDQUERY =
            "IF NOT EXISTS (SELECT 1 FROM OffensiveWords WHERE Word = @Word) " +
            "INSERT INTO OffensiveWords (Word) VALUES (@Word)";

        private const string DELETEOFFENSIVEWORDQUERY = "DELETE FROM OffensiveWords WHERE Word = @Word";
        private const string WORDPARAMETERNAME = "@Word";
        private readonly IDbConnectionFactory connectionFactory;

        public OffensiveWordsRepository(IDbConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public HashSet<string> LoadOffensiveWords()
        {
            HashSet<string> offensiveWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using var connection = this.connectionFactory.CreateConnection();
            connection.Open();

            using var command = new SqlCommand(SELECTOFFENSIVEWORDSQUERY, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                offensiveWords.Add(reader.GetString(0));
            }

            return offensiveWords;
        }

        public void AddWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return;
            }

            using var connection = this.connectionFactory.CreateConnection();
            connection.Open();

            using var command = new SqlCommand(INSERTOFFENSIVEWORDQUERY, connection);
            command.Parameters.AddWithValue(WORDPARAMETERNAME, word);
            command.ExecuteNonQuery();
        }

        public void DeleteWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return;
            }

            using var connection = this.connectionFactory.CreateConnection();
            connection.Open();

            using var command = new SqlCommand(DELETEOFFENSIVEWORDQUERY, connection);
            command.Parameters.AddWithValue(WORDPARAMETERNAME, word);
            command.ExecuteNonQuery();
        }
    }
}
