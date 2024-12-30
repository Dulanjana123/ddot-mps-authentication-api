using AutoMapper;
using Core.CommonDtos;
using Core.Utilities;
using DataAccess.Entities;
using DataAccess.Repositories;
using Microsoft.Graph.Models;
using Model.Dtos;
using Model.Response;
using Model.Services.UserManagement;
using static Core.Exceptions.UserDefinedException;

namespace DDOT.MPS.Auth.Api.Managers
{
    public class LoginHistoryManager : ILoginHistoryManager
    {
        private readonly IMapper _mapper;
        private readonly IMpsUserRepository _userRepository;
        private readonly IMpsLoginHistoryRepository _mpsLoginHistoryRepository;
        private readonly IExternalUserService _externalUserService;

        public LoginHistoryManager(IMapper mapper, IMpsUserRepository userRepository, IExternalUserService externalUserService, IMpsLoginHistoryRepository mpsLoginHistoryRepository)
        {
            _mapper = mapper;
            _userRepository = userRepository;
            _externalUserService = externalUserService;
            _mpsLoginHistoryRepository = mpsLoginHistoryRepository;
        }
        public async Task<BaseResponse<FullLoginHistoryDto>> CreateLoginHistory(LoginHistoryBaseDto loginHistory, LoginHistoryHeaderDto headers)
        {
           
            MpsUser? dbUser = await _userRepository.GetUserByEmail(loginHistory.UserEmail);

            if (dbUser == null)
            {
                throw new UDValiationException("NO_ACCOUNT_WITH_EMAIL");
            }

            User user;
            if (dbUser.IsAdmin)
            {
                user = await _externalUserService.GetUserBySignInName(loginHistory.UserEmail, Core.Enums.UserType.Admin);
            }
            else
            {
                user = await _externalUserService.GetUserBySignInName(loginHistory.UserEmail, Core.Enums.UserType.Client);
            }

            if (user == null)
            {
                throw new UDValiationException("NO_ACCOUNT_WITH_EMAIL");
            }

            LoginHistoryRequestInfo loginHistoryRequestInfo = AuthUtils.GetDeviceInfo(headers.Agent);
            FullLoginHistoryDto fullLoginHistoryObject = new FullLoginHistoryDto
            {Browser= loginHistoryRequestInfo.Browser,
            Device= loginHistoryRequestInfo.Device,
            Detaileddescription = loginHistoryRequestInfo.Detaileddescription,
            Os = loginHistoryRequestInfo.Os,
            IpAddress = headers.IpAddress,
            Timestamp = loginHistory.Timestamp,
            UserId = dbUser.UserId,
            Userintractionid = loginHistory.Userintractionid

            };
            FullLoginHistoryDto result = await _mpsLoginHistoryRepository.CreateLoginHistory(fullLoginHistoryObject);

            return new BaseResponse<FullLoginHistoryDto> { Success = true, Data = result, Message = "LOGIN_HISTORY_CREATED_SUCCESSFULLY" };
        }
    }
}
