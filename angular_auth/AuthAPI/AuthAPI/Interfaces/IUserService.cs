using AuthAPI.Models;

namespace AuthAPI.Interfaces
{
    public interface IUserService
    {
        Task<RegistrationResult> RegisterUser(User userModel);
        Task<RegistrationResult> AuthenticateUser(User userModel);
    }
}
