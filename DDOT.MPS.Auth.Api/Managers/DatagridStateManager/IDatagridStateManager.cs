using Model.Dtos;
using Model.Request;
using Model.Response;

namespace DDOT.MPS.Auth.Api.Managers
{
    public interface IDatagridStateManager
    {
        Task<BaseResponse<DatagridStateResponseDto>> CreateDatagridState(DatagridStateDto datagridState);

        Task<BaseResponse<DatagridStateResponseDto>> GetDatagridStateById(int id);

        Task<BaseResponse<DatagridStateResponseDto>> UpdateDatagridState(int id, DatagridStateDto datagridState);

        Task<BaseResponse<DatagridStateResponseDto>> GetDatagridStateByUserAndInterface(DatagridStateRequest request);
    }
}
