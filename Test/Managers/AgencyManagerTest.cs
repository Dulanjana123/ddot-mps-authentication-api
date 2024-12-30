using AutoMapper;
using DataAccess.Entities;
using DataAccess.Repositories;
using DDOT.MPS.Auth.Api.Managers;
using Model.Dtos;
using Model.Request;
using Model.Request.Generic;
using Model.Response;
using Moq;
using Test.Helpers;
using static Core.Exceptions.UserDefinedException;

namespace Test.Managers
{
    [TestFixture]
    public class AgencyManagerTest
    {
        private Mock<IMpsAgencyRepository> _agencyRepository;
        private Mock<IMpsAgencyCategoryRepository> _agencyCategoryRepository;
        private Mock<IMapper> _mapper;
        private IAgencyManager _agencyManager;

        [SetUp]
        public void SetUp()
        {
            _agencyRepository = new Mock<IMpsAgencyRepository>();
            _agencyCategoryRepository = new Mock<IMpsAgencyCategoryRepository>();
            _mapper = new Mock<IMapper>();
            _agencyManager = new AgencyManager(_agencyRepository.Object, _mapper.Object, _agencyCategoryRepository.Object);
        }

        [Test]
        public async Task CreateAgency_Successful_ReturnsAgencyResponse()
        {
            AgencyDto agencyDto = new AgencyDto { AgencyCategoryCode = "CAT001", AgencyCode = "AG001", AgencyName = "Test Agency" };
            AgencyCategory agencyCategory = new AgencyCategory { AgencyCategoryId = 1 };
            Agency agency = new Agency();
            AgencyResponseDto agencyResponseDto = new AgencyResponseDto();

            _agencyCategoryRepository.Setup(x => x.GetAgencyCategoryByAgencyCategoryCode(agencyDto.AgencyCategoryCode))
                                      .ReturnsAsync(agencyCategory);
            //_agencyRepository.Setup(x => x.CreateAgency(It.IsAny<MpsAgency>())).Returns(Task.CompletedTask);
            _mapper.Setup(m => m.Map<AgencyResponseDto>(It.IsAny<Agency>())).Returns(agencyResponseDto);

            BaseResponse<AgencyResponseDto> result = await _agencyManager.CreateAgency(agencyDto);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(agencyResponseDto, result.Data);
            Assert.AreEqual("AGENCY_CREATED_SUCCESSFULLY", result.Message);
        }

        [Test]
        public async Task CreateAgency_AgencyCategoryCodeNotFound_ReturnsFailureResponse()
        {
            AgencyDto agencyDto = new AgencyDto { AgencyCategoryCode = "CAT001" };

            _agencyCategoryRepository.Setup(x => x.GetAgencyCategoryByAgencyCategoryCode(agencyDto.AgencyCategoryCode))
                                      .ReturnsAsync((AgencyCategory)null);

            BaseResponse<AgencyResponseDto> result = await _agencyManager.CreateAgency(agencyDto);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("AGENCY_CATEGORY_CODE_NOT_FOUND", result.Message);
        }

        [Test]
        public void CreateAgency_ExceptionThrown_RethrowsException()
        {
            AgencyDto agencyDto = new AgencyDto { AgencyCategoryCode = "CAT001" };

            _agencyCategoryRepository.Setup(x => x.GetAgencyCategoryByAgencyCategoryCode(agencyDto.AgencyCategoryCode))
                                      .Throws(new Exception("Database error"));

            Assert.ThrowsAsync<Exception>(() => _agencyManager.CreateAgency(agencyDto));
        }

        [Test]
        public async Task GetAgencyById_AgencyExists_ReturnsAgencyResponse()
        {
            int agencyId = 1;
            Agency mpsAgency = new Agency();
            AgencyResponseDto agencyResponseDto = new AgencyResponseDto();

            _agencyRepository.Setup(x => x.GetAgencyById(agencyId))
                             .ReturnsAsync(mpsAgency);
            _mapper.Setup(m => m.Map<AgencyResponseDto>(mpsAgency))
                   .Returns(agencyResponseDto);

            BaseResponse<AgencyResponseDto> result = await _agencyManager.GetAgencyById(agencyId);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(agencyResponseDto, result.Data);
            Assert.AreEqual("AGENCY_RETRIEVED_SUCCESSFULLY", result.Message);
        }

        [Test]
        public void GetAgencyById_AgencyNotFound_ThrowsNotFoundException()
        {
            int agencyId = 1;

            _agencyRepository.Setup(x => x.GetAgencyById(agencyId))
                             .ReturnsAsync((Agency)null);

            Assert.ThrowsAsync<UDNotFoundException>(() => _agencyManager.GetAgencyById(agencyId));
        }

        [Test]
        public async Task UpdateAgency_AgencyExistsAndCategoryExists_ReturnsSuccessResponse()
        {
            int agencyid = 1;
            AgencyDto agencyDto = new AgencyDto { AgencyCategoryCode = "CAT001", AgencyCode = "AG001", AgencyName = "Updated Agency" };
            Agency mpsAgency = new Agency();
            AgencyCategory mpsAgencyCategory = new AgencyCategory { AgencyCategoryId = 1 };
            AgencyResponseDto agencyResponseDto = new AgencyResponseDto();

            _agencyRepository.Setup(x => x.GetAgencyById(agencyid))
                             .ReturnsAsync(mpsAgency);
            _agencyCategoryRepository.Setup(x => x.GetAgencyCategoryByAgencyCategoryCode(agencyDto.AgencyCategoryCode))
                                      .ReturnsAsync(mpsAgencyCategory);
            _mapper.Setup(m => m.Map<AgencyResponseDto>(mpsAgency))
                   .Returns(agencyResponseDto);

            BaseResponse<AgencyResponseDto> result = await _agencyManager.UpdateAgency(agencyid, agencyDto);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(agencyResponseDto, result.Data);
            Assert.AreEqual("AGENCY_UPDATED_SUCCESSFULLY", result.Message);
        }

        [Test]
        public void UpdateAgency_AgencyNotFound_ThrowsNotFoundException()
        {
            int agencyId = 1;
            AgencyDto agencyDto = new AgencyDto { AgencyCode = "AG001" };

            _agencyRepository.Setup(x => x.GetAgencyById(agencyId))
                             .ReturnsAsync((Agency)null);

            Assert.ThrowsAsync<UDNotFoundException>(() => _agencyManager.UpdateAgency(agencyId, agencyDto));
        }

        [Test]
        public async Task UpdateAgency_AgencyCategoryCodeNotFound_ReturnsFailureResponse()
        {
            int agencyId = 1;
            AgencyDto agencyDto = new AgencyDto { AgencyCategoryCode = "CAT001", AgencyCode = "AG001" };
            Agency mpsAgency = new Agency();

            _agencyRepository.Setup(x => x.GetAgencyById(agencyId))
                             .ReturnsAsync(mpsAgency);
            _agencyCategoryRepository.Setup(x => x.GetAgencyCategoryByAgencyCategoryCode(agencyDto.AgencyCategoryCode))
                                      .ReturnsAsync((AgencyCategory)null);

            BaseResponse<AgencyResponseDto> result = await _agencyManager.UpdateAgency(agencyId, agencyDto);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("AGENCY_CATEGORY_CODE_NOT_FOUND", result.Message);
        }

        [Test]
        public void UpdateAgency_ExceptionThrown_RethrowsException()
        {
            int agencyId = 1;
            AgencyDto agencyDto = new AgencyDto { AgencyCategoryCode = "CAT001", AgencyCode = "AG001" };
            Agency mpsAgency = new Agency();

            _agencyRepository.Setup(x => x.GetAgencyById(agencyId))
                             .ReturnsAsync(mpsAgency);
            _agencyCategoryRepository.Setup(x => x.GetAgencyCategoryByAgencyCategoryCode(agencyDto.AgencyCategoryCode))
                                      .Throws(new Exception("Database error"));

            Assert.ThrowsAsync<Exception>(() => _agencyManager.UpdateAgency(agencyId, agencyDto));
        }

        [Test]
        public async Task GetPaginatedList_ValidRequest_ReturnsPaginatedResponse()
        {
            AgencyPaginatedRequest request = new AgencyPaginatedRequest
            {
                PagingAndSortingInfo = new PagingAndSortingInfo
                {
                    Paging = new PagingInfo
                    {
                        PageSize = 10,
                        PageNo = 1
                    }
                }
            };

            IQueryable<Agency> mpsAgencies = new List<Agency> { new Agency(), new Agency() }.AsQueryable();
            List<AgencyResponseDto> agencyResponseDtos = new List<AgencyResponseDto> { new AgencyResponseDto(), new AgencyResponseDto() };

            TestAsyncEnumerable<Agency> mockAgencyQueryable = new TestAsyncEnumerable<Agency>(mpsAgencies);

            _agencyRepository.Setup(x => x.GetAll(It.IsAny<AgencyPaginatedRequest>()))
                             .Returns(mockAgencyQueryable);

            _agencyRepository.Setup(x => x.GetRowCount(It.IsAny<AgencyPaginatedRequest>()))
                             .Returns(mpsAgencies.Count());

            _mapper.Setup(m => m.Map<AgencyResponseDto>(It.IsAny<Agency>()))
                   .Returns(new AgencyResponseDto());

            BaseResponse<Result<AgencyResponseDto>> result = await _agencyManager.GetPaginatedList(request);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(agencyResponseDtos.Count, result.Data.Entities.Length);
            Assert.AreEqual(mpsAgencies.Count(), result.Data.Pagination.Length);
            Assert.AreEqual(request.PagingAndSortingInfo.Paging.PageSize, result.Data.Pagination.PageSize);
        }
    }
}
