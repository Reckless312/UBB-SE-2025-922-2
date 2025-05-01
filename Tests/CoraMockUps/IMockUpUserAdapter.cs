using DrinkDb_Auth.Adapter;
using DataAccess.Model;

namespace Tests.CoraMockUps
{
    public interface IMockUpUserAdapter : IUserAdapter
    {
        public bool CreatedUser { get; set; }
        public User? User { get; set; }
        public bool UpdatedUser { get; set; }
    }
}
