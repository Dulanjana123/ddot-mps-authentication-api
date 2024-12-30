using AutoMapper;
using DataAccess.Entities;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Model.Dtos;
using Model.Request;
using Model.Response;
using static Core.Exceptions.UserDefinedException;

namespace DDOT.MPS.Auth.Api.Managers
{
    public class AgencyManager : IAgencyManager
    {
        private readonly IMapper _mapper;
        private readonly IMpsAgencyRepository _agencyRepository;
        private readonly IMpsAgencyCategoryRepository _agencyCategoryRepository;

        public AgencyManager(IMpsAgencyRepository agencyRepository, IMapper mapper, IMpsAgencyCategoryRepository agencyCategoryRepository)
        {
            _agencyRepository = agencyRepository;
            _mapper = mapper;
            _agencyCategoryRepository = agencyCategoryRepository;
        }

        public async Task<BaseResponse<AgencyResponseDto>> CreateAgency(AgencyDto agency)
        {
            try
            {
                Agency? mpsAgencyRetrived = await _agencyRepository.GetAgencyByAgencyCode(agency.AgencyCode);
                if (mpsAgencyRetrived != null)
                {
                    return new BaseResponse<AgencyResponseDto> { Success = false, Message = "AGENCY_IS_ALREADY_AVAILABLE" };
                }
                AgencyCategory? mpsAgencyCategory = await _agencyCategoryRepository.GetAgencyCategoryByAgencyCategoryCode(agency.AgencyCategoryCode);
                if (mpsAgencyCategory != null)
                {
                    Agency mpsAgency = new Agency()
                    {
                        AgencyCategoryId = mpsAgencyCategory.AgencyCategoryId,
                        AgencyCode = agency.AgencyCode,
                        AgencyName = agency.AgencyName,
                        AgencyAddress = agency.AgencyAddress,
                        AgencyTelephone = agency.AgencyTelephone,
                        ContactName = agency.ContactName,
                        ContactTelephone = agency.ContactTelephone,
                        ContactEmail = agency.ContactEmail,
                        IsActive = agency.IsActive,
                    };
                    await _agencyRepository.CreateAgency(mpsAgency);
                    AgencyResponseDto agencyResponseDto = _mapper.Map<AgencyResponseDto>(mpsAgency);
                    return new BaseResponse<AgencyResponseDto> { Success = true, Data = agencyResponseDto, Message = "AGENCY_CREATED_SUCCESSFULLY" };
                }
                return new BaseResponse<AgencyResponseDto> { Success = false, Message = "AGENCY_CATEGORY_CODE_NOT_FOUND" };

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<BaseResponse<AgencyResponseDto>> GetAgencyById(int id)
        {
            Agency? mpsAgency = await _agencyRepository.GetAgencyById(id);
            if (mpsAgency == null)
            {
                throw new UDNotFoundException("AGENCY_NOT_FOUND");
            }
            AgencyResponseDto agencyResponseDto = _mapper.Map<AgencyResponseDto>(mpsAgency);
            return new BaseResponse<AgencyResponseDto> { Success = true, Data = agencyResponseDto, Message = "AGENCY_RETRIEVED_SUCCESSFULLY" };
        }

        public async Task<BaseResponse<AgencyResponseDto>> UpdateAgency(int id, AgencyDto agency)
        {
            try
            {
                Agency? mpsAgency = await _agencyRepository.GetAgencyById(id);
                if (mpsAgency == null)
                {
                    throw new UDNotFoundException("AGENCY_NOT_FOUND");
                }
                AgencyCategory? mpsAgencyCategory = await _agencyCategoryRepository.GetAgencyCategoryByAgencyCategoryCode(agency.AgencyCategoryCode);
                if (mpsAgencyCategory != null)
                {
                    mpsAgency.AgencyCategoryId = mpsAgencyCategory.AgencyCategoryId;
                    mpsAgency.AgencyName = agency.AgencyName;
                    mpsAgency.AgencyAddress = agency.AgencyAddress;
                    mpsAgency.AgencyTelephone = agency.AgencyTelephone;
                    mpsAgency.ContactName = agency.ContactName;
                    mpsAgency.ContactTelephone = agency.ContactTelephone;
                    mpsAgency.ContactEmail = agency.ContactEmail;
                    mpsAgency.IsActive = agency.IsActive;

                    await _agencyRepository.UpdateAgency(mpsAgency);

                    AgencyResponseDto agencyResponseDto = _mapper.Map<AgencyResponseDto>(mpsAgency);
                    return new BaseResponse<AgencyResponseDto> { Success = true, Data = agencyResponseDto, Message = "AGENCY_UPDATED_SUCCESSFULLY" };
                }
                return new BaseResponse<AgencyResponseDto> { Success = false, Message = "AGENCY_CATEGORY_CODE_NOT_FOUND" };

            }
            catch (Exception)
            {
                throw;
            }
            throw new NotImplementedException();
        }

        public async Task<BaseResponse<Result<AgencyResponseDto>>> GetPaginatedList(AgencyPaginatedRequest request)
        {
            IQueryable<Agency> agencyResponseDtos = _agencyRepository.GetAll(request);
            List<AgencyResponseDto> agencyResponsesList = await agencyResponseDtos.Select(x => _mapper.Map<AgencyResponseDto>(x)).ToListAsync();

            BaseResponse<Result<AgencyResponseDto>> response = new BaseResponse<Result<AgencyResponseDto>>
            {
                Success = true,
                Data = new Result<AgencyResponseDto>
                {
                    Entities = agencyResponsesList.ToArray(),
                    Pagination = new Pagination()
                    {
                        Length = _agencyRepository.GetRowCount(request),
                        PageSize = request.PagingAndSortingInfo.Paging.PageSize
                    }
                }
            };

            return response;
        }
    }
}
