namespace DrinkDb_Auth.ProxyRepository.Authentification
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using DataAccess.Model.Authentication;
    using IRepository;

    internal class SessionProxyRepository : ISessionRepository
    {
        public Session CreateSession(Guid userId)
        {
            throw new NotImplementedException();
        }

        public bool EndSession(Guid sessionId)
        {
            throw new NotImplementedException();
        }

        public Session GetSession(Guid sessionId)
        {
            throw new NotImplementedException();
        }

        public Session GetSessionByUserId(Guid userId)
        {
            throw new NotImplementedException();
        }
    }
}
