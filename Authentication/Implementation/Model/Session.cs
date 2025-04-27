using System;

namespace DrinkDb_Auth.Model
{
    public class Session
    {
        public Guid SessionId { get; set; }
        public Guid UserId { get; set; }
        public bool IsActive => UserId != Guid.Empty;

        public Session()
        {
            SessionId = Guid.NewGuid();
        }

        public Session(Guid sessionId, Guid userId)
        {
            this.SessionId = sessionId;
            this.UserId = userId;
        }

        public static Session CreateSessionForUser(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                throw new ArgumentException("User ID cannot be empty", nameof(userId));
            }

            var session = new Session();
            session.UserId = userId;
            return session;
        }

        public static Session CreateSessionWithIds(Guid sessionId, Guid userId)
        {
            if (userId == Guid.Empty)
            {
                throw new ArgumentException("User ID cannot be empty", nameof(userId));
            }

            if (sessionId == Guid.Empty)
            {
                throw new ArgumentException("Session ID cannot be empty", nameof(sessionId));
            }

            return new Session(sessionId, userId);
        }

        public void EndSessionForUser(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                throw new ArgumentException("User ID cannot be empty", nameof(userId));
            }

            if (this.UserId != userId)
            {
                throw new InvalidOperationException("Session does not belong to specified user");
            }

            this.UserId = Guid.Empty;
        }

        public override string ToString()
        {
            return $"Session[ID: {SessionId}, UserID: {UserId}, Active: {IsActive}]";
        }

        public override bool Equals(object? obj)
        {
            if (obj is Session other)
            {
                return SessionId == other.SessionId;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return SessionId.GetHashCode();
        }
    }
}