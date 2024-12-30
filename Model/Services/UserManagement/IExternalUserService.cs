using Core.Enums;
using Microsoft.Graph.Models;
using Microsoft.Identity.Client;
using Model.Dtos;
using System.Threading.Tasks;

namespace Model.Services.UserManagement
{
    public interface IExternalUserService
    {
        Task<User> CreateNewUser(UserRegistrationB2cDto user, UserType userType = UserType.Client);
        User UpdateExistingUser(UserDto user);
        User GetByLoginEmail(string email);
        void DeleteUser(User user);
        Task<B2CTokenResponse> InitiateLogin(UserLoginDto loginDto, UserType userType = UserType.Client);
        Task ResetUserPassword(string email, string newPassword, UserType userType = UserType.Client);
        Task DeactivateUser(string email, UserType userType = UserType.Client);
        Task<User> GetUserBySignInName(string email, UserType userType = UserType.Client);

    }
}
