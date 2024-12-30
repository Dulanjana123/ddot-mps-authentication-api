using AutoMapper;
using DataAccess.Contexts;
using DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Model.Dtos;
using Model.Request;

namespace DataAccess.Repositories
{
    public class MpsUserRepository : IMpsUserRepository
    {
        private readonly MpsDbContext _dbContext;
        private readonly IMapper _mapper;

        public MpsUserRepository(MpsDbContext mpsDbContext, IMapper mapper)
        {
            _dbContext = mpsDbContext ?? throw new ArgumentNullException(nameof(mpsDbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<UserDto> CreateUser(MpsUser user)
        {
            _dbContext.Add(user);
            await _dbContext.SaveChangesAsync();
            var newUser = _mapper.Map<UserDto>(user);
            return newUser;
        }

        public async Task<UserDto> UpdateUser(MpsUser user)
        {
            _dbContext.Update(user);
            await _dbContext.SaveChangesAsync();
            var newUser = _mapper.Map<UserDto>(user);
            return newUser;
        }

        public async Task<bool> IsExistingUserByEmail(string email)
        {
            bool isExsistingUser = await _dbContext.Users.AnyAsync(u => u.EmailAddress.ToLower() == email.ToLower());
            return isExsistingUser;
        }

        public async Task<bool> IsInitialPasswordReset(string email)
        {            
            MpsUser user = await _dbContext.Users.Where(t => t.EmailAddress.ToLower() == email.ToLower()).FirstOrDefaultAsync();
            return !(user?.IsInitialPasswordReset ?? false);
        }
        public async Task<bool> IsMigratedAccount(string email)
        {
            MpsUser user = await _dbContext.Users.Where(t => t.EmailAddress.ToLower() == email.ToLower()).FirstOrDefaultAsync();
            return user?.IsMigratedFromLegacy ?? false;
        }

        public async Task<bool> IsActiveUser(string email)
        {
            MpsUser user = await _dbContext.Users.Where(t => t.EmailAddress.ToLower() == email.ToLower()).FirstOrDefaultAsync();
            return user?.IsActive ?? false;
        }

        public async Task<MpsUser?> GetUserByEmail(string email)
        {
            return await _dbContext.Users.Where(t => t.EmailAddress.ToLower() == email.ToLower()).FirstOrDefaultAsync();
        }

        public async Task<MpsUser?> GetUserById(int id)
        {
            return await _dbContext.Users.Where(t => t.UserId == id).FirstOrDefaultAsync();
        }

        public async Task<bool> IsResetPasswordLinkUsed(string email)
        {
            MpsUser user = await _dbContext.Users.Where(t => t.EmailAddress.ToLower() == email.ToLower()).FirstOrDefaultAsync();
            return user?.IsResetPasswordLinkUsed ?? false;
        }

        public IQueryable<MpsUser> GetAll(UserPaginatedRequest request)
        {
            string firstNameFilter = request.Filters.FirstName.Trim().ToLower();
            string lastNameFilter = request.Filters.LastName.Trim().ToLower();

            IQueryable<MpsUser> users = _dbContext.Users
                .Where(usr => usr.FirstName.ToLower().Contains(firstNameFilter) &&
                              (string.IsNullOrEmpty(lastNameFilter) || usr.LastName.ToLower().Contains(lastNameFilter)))
                .OrderByDescending(usr => usr.ModifiedDate.HasValue ? usr.ModifiedDate : usr.CreatedDate)
                .Skip((request.PagingAndSortingInfo.Paging.PageNo - 1) * request.PagingAndSortingInfo.Paging.PageSize)
                .Take(request.PagingAndSortingInfo.Paging.PageSize);

            return users;
        }

        public long GetRowCount(UserPaginatedRequest request)
        {
            return _dbContext.Users.Where(x =>
                x.FirstName.Contains(request.Filters.FirstName.Trim())
                || x.LastName.Contains(request.Filters.LastName.Trim())
            ).Count();
        }

        public Task<string?> GetUserType(int userTypeId)
        { 
            return _dbContext.UserTypes.Where(x => x.UserTypeId == userTypeId).Select(x => x.UserTypeName).FirstOrDefaultAsync();
        }
    }
}
