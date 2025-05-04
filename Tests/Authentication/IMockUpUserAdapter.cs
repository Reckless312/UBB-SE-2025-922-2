using DataAccess.Model.Authentication;
using IRepository;

namespace Tests.Authentication
{
    internal interface IMockUpUserAdapter : IUserRepository
    {
        User User { get; set; }
        bool UpdatedUser { get; set; }
    }
}