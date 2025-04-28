using System;
using CombinedProject.Model;

namespace CombinedProject.Adapter
{
    public interface ISessionAdapter
    {
        public Session CreateSession(Guid userId);
        public bool EndSession(Guid sessionId);
        public Session GetSession(Guid sessionId);
        public Session GetSessionByUserId(Guid userId);
    }
}