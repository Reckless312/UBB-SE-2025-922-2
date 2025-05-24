namespace DataAccess.Model.Authentication
{
    public class Session
    {
        public Guid SessionId { get; set; }
        public Guid UserId { get; set; }

        public Session()
        {
            this.SessionId = Guid.NewGuid();
        }

        public Session(Guid sessionId, Guid userId)
        {
            this.SessionId = sessionId;
            this.UserId = userId;
        }

        public bool IsActive()
        {
            return this.UserId != Guid.Empty;
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