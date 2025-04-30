using DrinkDb_Auth.Repository;
using SharedResources.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using DrinkDb_Auth.Repository.AdminDashboard;
using DrinkDb_Auth.Repository.Authentication;
using SharedResources.Model.Authentication;

namespace Tests
{
    [TestClass]
    public sealed class TestsSessionAdapter
    {
        private static readonly Guid TestUserId = new Guid("11111111-1111-1111-1111-111111111111");
        private static UserRepository userAdapter = new UserRepository();
        private static SessionRepository sessionAdapter = new SessionRepository();
        private Guid sessionId;


        [TestMethod]
        public void TestSessionAdapter()
        {
            this.CreateSession();
            this.GetSessionByUserId();
            this.GetSessionById();
            this.EndSession();
            this.NotFoundSessionByItsId();
            this.NotFoundSessionByUserId();
        }
        
        public void CreateSession()
        {
            userAdapter.CreateUser(new User { UserId = TestUserId, PasswordHash = "passwordHash", TwoFASecret = "twoFactorSecret", Username = "username" });

            sessionAdapter.CreateSession(TestUserId);
        }

        public void GetSessionByUserId()
        {
            Session session = sessionAdapter.GetSessionByUserId(TestUserId);

            this.sessionId = session.SessionId;

            Assert.IsNotNull(session);
        }

        public void GetSessionById()
        {
            Session session = sessionAdapter.GetSession(this.sessionId);

            Assert.AreEqual(this.sessionId, session.SessionId);
        }

        public void EndSession()
        {
            sessionAdapter.EndSession(this.sessionId);

            userAdapter.DeleteUser(TestUserId);
        }

        public void NotFoundSessionByItsId()
        {
            Assert.ThrowsException<Exception>(() =>
            {
                sessionAdapter.GetSession(sessionId);
            });
        }

        public void NotFoundSessionByUserId()
        {
            Assert.ThrowsException<Exception>(() =>
            {
                sessionAdapter.GetSessionByUserId(TestUserId);
            });
        }
    }
}
