using AutoMapper;
using DataAccess.Entities;
using DataAccess.Repositories;
using Model.Dtos;
using Model.Request;
using Model.Response;
using static Core.Exceptions.UserDefinedException;

namespace DDOT.MPS.Auth.Api.Managers
{
    public class DatagridStateManager : IDatagridStateManager
    {
        private readonly IMapper _mapper;
        private readonly IMpsDatagridStateRepository _datagridStateRepository;

        public DatagridStateManager(IMpsDatagridStateRepository datagridStateRepository, IMapper mapper)
        {
            _datagridStateRepository = datagridStateRepository;
            _mapper = mapper;
        }

        public async Task<BaseResponse<DatagridStateResponseDto>> CreateDatagridState(DatagridStateDto datagridState)
        {
            try
            {
                SettingsGridState mpsDatagridState = new SettingsGridState()
                {
                    SettingsGridStateId = datagridState.SettingsGridStateId,
                    UserId = datagridState.UserId,
                    InterfaceId = datagridState.InterfaceId,
                    GridObjectJson = datagridState.GridObjectJson
                };
                await _datagridStateRepository.CreateDatagridState(mpsDatagridState);
                DatagridStateResponseDto datagridStateResponseDto = _mapper.Map<DatagridStateResponseDto>(mpsDatagridState);
                return new BaseResponse<DatagridStateResponseDto> { Success = true, Data = datagridStateResponseDto, Message = "DATAGRID_STATE_CREATED_SUCCESSFULLY" };

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<BaseResponse<DatagridStateResponseDto>> GetDatagridStateById(int id)
        {
            SettingsGridState? mpsDatagridState = await _datagridStateRepository.GetDatagridStateById(id);
            if (mpsDatagridState == null)
            {
                throw new UDNotFoundException("DATAGRID_NOT_FOUND");
            }
            DatagridStateResponseDto datagridStateResponseDto = _mapper.Map<DatagridStateResponseDto>(mpsDatagridState);
            return new BaseResponse<DatagridStateResponseDto> { Success = true, Data = datagridStateResponseDto, Message = "DATAGRID_STATE_RETRIEVED_SUCCESSFULLY" };
        }

        public async Task<BaseResponse<DatagridStateResponseDto>> GetDatagridStateByUserAndInterface(DatagridStateRequest request)
        {
            SettingsGridState? mpsDatagridState = await _datagridStateRepository.GetDatagridStateByUserAndInterface(request);
            if (mpsDatagridState == null)
            {
                throw new UDNotFoundException("DATAGRID_NOT_FOUND");
            }
            DatagridStateResponseDto datagridStateResponseDto = _mapper.Map<DatagridStateResponseDto>(mpsDatagridState);
            return new BaseResponse<DatagridStateResponseDto> { Success = true, Data = datagridStateResponseDto, Message = "DATAGRID_STATE_RETRIEVED_SUCCESSFULLY" };
        }

        public async Task<BaseResponse<DatagridStateResponseDto>> UpdateDatagridState(int id, DatagridStateDto datagridState)
        {
            try
            {
                SettingsGridState? mpsDatagridState = await _datagridStateRepository.GetDatagridStateById(id);
                if (mpsDatagridState == null)
                {
                    throw new UDNotFoundException("DATAGRID_STATE_NOT_FOUND");
                }

                mpsDatagridState.GridObjectJson = datagridState.GridObjectJson;
                mpsDatagridState.IsActive = datagridState.IsActive;

                await _datagridStateRepository.UpdateDatagridState(mpsDatagridState);

                DatagridStateResponseDto datagridStateResponseDto = _mapper.Map<DatagridStateResponseDto>(mpsDatagridState);
                return new BaseResponse<DatagridStateResponseDto> { Success = true, Data = datagridStateResponseDto, Message = "DATAGRID_STATE_UPDATED_SUCCESSFULLY" };
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
