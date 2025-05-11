using DataAccess.Model;
using DataAccess.Model.Authentication;
using IRepository;

namespace Tests.CoraMockUps
{
    public interface IMockUpUserAdapter : IUserRepository
    {
        public bool CreatedUser { get; set; }
        public User? User { get; set; }
        public bool UpdatedUser { get; set; }
    }
}
