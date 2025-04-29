using DrinkDb_Auth.Adapter;
using DrinkDb_Auth.Model;

namespace Tests.CoraMockUps
{
    public interface IMockUpUserAdapter : IUserAdapter
    {
        public bool CreatedUser { get; set; }
        public User? User { get; set; }
        public bool UpdatedUser { get; set; }
    }
}
