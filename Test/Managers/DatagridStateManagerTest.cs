using AutoMapper;
using DataAccess.Entities;
using DataAccess.Repositories;
using DDOT.MPS.Auth.Api.Managers;
using Model.Dtos;
using Model.Request;
using Model.Response;
using Moq;
using static Core.Exceptions.UserDefinedException;

namespace Test.Managers
{
    [TestFixture]
    public class DatagridStateManagerTest
    {
        private Mock<IMpsDatagridStateRepository> _datagridStateRepository;
        private Mock<IMapper> _mapper;
        private IDatagridStateManager _datagridStateManager;

        [SetUp]
        public void SetUp()
        {
            _datagridStateRepository = new Mock<IMpsDatagridStateRepository>();
            _mapper = new Mock<IMapper>();
            _datagridStateManager = new DatagridStateManager(_datagridStateRepository.Object, _mapper.Object);
        }

        [Test]
        public async Task CreateDatagridState_Successful_ReturnsDatagridStateResponse()
        {
            // Arrange
            DatagridStateDto datagridStateDto = new DatagridStateDto { InterfaceId = 1, UserId = 2, GridObjectJson = { } };
            SettingsGridState datagridState = new SettingsGridState();
            DatagridStateResponseDto responseDto = new DatagridStateResponseDto();

            _datagridStateRepository.Setup(x => x.CreateDatagridState(It.IsAny<SettingsGridState>()))
                                    .ReturnsAsync(datagridStateDto);
            _mapper.Setup(m => m.Map<DatagridStateResponseDto>(It.IsAny<SettingsGridState>()))
                   .Returns(responseDto);

            // Act
            BaseResponse<DatagridStateResponseDto> result = await _datagridStateManager.CreateDatagridState(datagridStateDto);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(responseDto, result.Data);
            Assert.AreEqual("DATAGRID_STATE_CREATED_SUCCESSFULLY", result.Message);
        }

        [Test]
        public async Task GetDatagridStateById_DatagridStateExists_ReturnsDatagridStateResponse()
        {
            // Arrange
            int datagridStateId = 1;
            SettingsGridState datagridState = new SettingsGridState();
            DatagridStateResponseDto responseDto = new DatagridStateResponseDto();

            _datagridStateRepository.Setup(x => x.GetDatagridStateById(datagridStateId))
                                    .ReturnsAsync(datagridState);
            _mapper.Setup(m => m.Map<DatagridStateResponseDto>(datagridState))
                   .Returns(responseDto);

            // Act
            BaseResponse<DatagridStateResponseDto> result = await _datagridStateManager.GetDatagridStateById(datagridStateId);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(responseDto, result.Data);
            Assert.AreEqual("DATAGRID_STATE_RETRIEVED_SUCCESSFULLY", result.Message);
        }

        [Test]
        public void GetDatagridStateById_DatagridStateNotFound_ThrowsNotFoundException()
        {
            // Arrange
            int datagridStateId = 1;

            _datagridStateRepository.Setup(x => x.GetDatagridStateById(datagridStateId))
                                    .ReturnsAsync((SettingsGridState)null);

            // Act & Assert
            Assert.ThrowsAsync<UDNotFoundException>(() => _datagridStateManager.GetDatagridStateById(datagridStateId));
        }

        [Test]
        public async Task GetDatagridStateByUserAndInterface_DatagridStateExists_ReturnsDatagridStateResponse()
        {
            // Arrange
            DatagridStateRequest request = new DatagridStateRequest { InterfaceId = 1, UserId = 2 };
            SettingsGridState datagridState = new SettingsGridState();
            DatagridStateResponseDto responseDto = new DatagridStateResponseDto();

            _datagridStateRepository.Setup(x => x.GetDatagridStateByUserAndInterface(request))
                                    .ReturnsAsync(datagridState);
            _mapper.Setup(m => m.Map<DatagridStateResponseDto>(datagridState))
                   .Returns(responseDto);

            // Act
            BaseResponse<DatagridStateResponseDto> result = await _datagridStateManager.GetDatagridStateByUserAndInterface(request);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(responseDto, result.Data);
            Assert.AreEqual("DATAGRID_STATE_RETRIEVED_SUCCESSFULLY", result.Message);
        }

        [Test]
        public void GetDatagridStateByUserAndInterface_DatagridStateNotFound_ThrowsNotFoundException()
        {
            // Arrange
            DatagridStateRequest request = new DatagridStateRequest { InterfaceId = 1, UserId = 2 };

            _datagridStateRepository.Setup(x => x.GetDatagridStateByUserAndInterface(request))
                                    .ReturnsAsync((SettingsGridState)null);

            // Act & Assert
            Assert.ThrowsAsync<UDNotFoundException>(() => _datagridStateManager.GetDatagridStateByUserAndInterface(request));
        }

        [Test]
        public async Task UpdateDatagridState_DatagridStateExists_ReturnsUpdatedDatagridState()
        {
            // Arrange
            int datagridStateId = 1;
            DatagridStateDto datagridStateDto = new DatagridStateDto { GridObjectJson = { } };
            SettingsGridState datagridState = new SettingsGridState();
            DatagridStateResponseDto responseDto = new DatagridStateResponseDto();

            _datagridStateRepository.Setup(x => x.GetDatagridStateById(datagridStateId))
                                    .ReturnsAsync(datagridState);
            _mapper.Setup(m => m.Map<DatagridStateResponseDto>(datagridState))
                   .Returns(responseDto);

            // Act
            BaseResponse<DatagridStateResponseDto> result = await _datagridStateManager.UpdateDatagridState(datagridStateId, datagridStateDto);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(responseDto, result.Data);
            Assert.AreEqual("DATAGRID_STATE_UPDATED_SUCCESSFULLY", result.Message);
        }

        [Test]
        public void UpdateDatagridState_DatagridStateNotFound_ThrowsNotFoundException()
        {
            // Arrange
            int datagridStateId = 1;
            DatagridStateDto datagridStateDto = new DatagridStateDto();

            _datagridStateRepository.Setup(x => x.GetDatagridStateById(datagridStateId))
                                    .ReturnsAsync((SettingsGridState)null);

            // Act & Assert
            Assert.ThrowsAsync<UDNotFoundException>(() => _datagridStateManager.UpdateDatagridState(datagridStateId, datagridStateDto));
        }
    }
}
