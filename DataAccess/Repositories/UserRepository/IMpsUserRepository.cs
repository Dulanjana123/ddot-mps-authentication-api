using DataAccess.Entities;
using Model.Dtos;
using Model.Request;

namespace DataAccess.Repositories
{
    public interface IMpsUserRepository
    {        
        Task<UserDto> CreateUser(MpsUser user);
        Task<UserDto> UpdateUser(MpsUser user);
        Task<bool> IsExistingUserByEmail(string email);
        Task<bool> IsInitialPasswordReset(string email);
        Task<bool> IsMigratedAccount(string email);
        Task<bool> IsActiveUser(string email);
        Task<MpsUser?> GetUserByEmail(string email);
        Task<MpsUser?> GetUserById(int id);
        Task<bool> IsResetPasswordLinkUsed(string email);
        IQueryable<MpsUser> GetAll(UserPaginatedRequest request);
        long GetRowCount(UserPaginatedRequest request);
        Task<string?> GetUserType(int userTypeId);
    }
}
