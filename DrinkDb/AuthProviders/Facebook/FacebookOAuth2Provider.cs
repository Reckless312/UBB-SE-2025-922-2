using System;
using System.Net.Http;
using System.Text.Json;
using DrinkDb_Auth.OAuthProviders;
using Windows.Networking.Sockets;
using DataAccess.Model.Authentication;
using DrinkDb_Auth.Repository.Authentication;
using IRepository;
using DrinkDb_Auth.Repository.AdminDashboard;
using System.Data.SqlClient;
using System.Configuration;

namespace DrinkDb_Auth.AuthProviders.Facebook
{
    public class FacebookOAuth2Provider : GenericOAuth2Provider
    {
        private static readonly SessionRepository SessionAdapter = new ();
        public AuthenticationResponse Authenticate(string userId, string token)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = $"https://graph.facebook.com/me?fields=id,name,email&access_token={token}";
                    HttpResponseMessage response = client.GetAsync(url).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        string json = response.Content.ReadAsStringAsync().Result;
                        var doc = JsonDocument.Parse(json).RootElement;

                        if (doc.TryGetProperty("id", out var idProp))
                        {
                            string fbId = idProp.GetString() ?? throw new Exception("Facebook ID is null.");
                            string fbName = doc.GetProperty("name").GetString() ?? throw new Exception("Facebook name is null.");
                            string fbEmail = doc.GetProperty("email").GetString() ?? throw new Exception("Facebook email is null.");

                            // store or update user in DB - UserService
                            bool isNewAccount = StoreOrUpdateUserInDb(fbId, fbName, fbEmail);

                            User user = UserRepository.GetUserByUsername(fbName) ?? throw new Exception("User not found");

                            Session session = SessionAdapter.CreateSession(user.UserId);
                            return new AuthenticationResponse
                            {
                                AuthenticationSuccessful = true,
                                OAuthToken = token,
                                SessionId = session.SessionId,
                                NewAccount = isNewAccount
                            };
                        }
                    }
                }
                return new AuthenticationResponse { AuthenticationSuccessful = false, OAuthToken = string.Empty, SessionId = Guid.Empty, NewAccount = false };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Facebook authentication failed: {ex}");
                return new AuthenticationResponse { AuthenticationSuccessful = false, OAuthToken = string.Empty, SessionId = Guid.Empty, NewAccount = false };
            }
        }

        private static readonly IUserRepository UserRepository = new UserRepository();
        private bool StoreOrUpdateUserInDb(string fbId, string fbName, string email)
        {
            bool isNewAccount = false;
            string connectionString = ConfigurationManager.ConnectionStrings["DrinkDbConnection"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Check if a user with this fbId already exists (stored as userName)
                string checkQuery = "SELECT COUNT(*) FROM Users WHERE userName = @fbId";
                using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@fbId", fbId);
                    int count = (int)checkCommand.ExecuteScalar();
                    if (count == 0)
                    {
                        // Insert a new user with email
                        string insertQuery = @"
                            INSERT INTO Users (userId, userName, passwordHash, twoFASecret, emailAddress, numberOfDeletedReviews, hasSubmittedAppeal)
                            VALUES (NEWID(), @fbId, '', NULL, @email, 0, 0)";
                        using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@fbId", fbId);
                            insertCommand.Parameters.AddWithValue("@email", email);
                            int result = insertCommand.ExecuteNonQuery();
                            if (result > 0)
                            {
                                isNewAccount = true;
                            }
                        }
                    }
                    else
                    {
                        // Update email if it's different
                        string updateQuery = "UPDATE Users SET emailAddress = @email WHERE userName = @fbId";
                        using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                        {
                            updateCommand.Parameters.AddWithValue("@fbId", fbId);
                            updateCommand.Parameters.AddWithValue("@email", email);
                            updateCommand.ExecuteNonQuery();
                        }
                    }
                }
            }

            return isNewAccount;
        }
    }
}
