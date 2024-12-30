using DataAccess.Entities;
using Model.Dtos;
using Model.Request;

namespace DataAccess.Repositories
{
    public interface IMpsDatagridStateRepository
    {
        Task<DatagridStateDto> CreateDatagridState(SettingsGridState datagridState);

        Task<DatagridStateDto> UpdateDatagridState(SettingsGridState datagridState);

        Task<SettingsGridState> GetDatagridStateById(int id);

        Task<SettingsGridState> GetDatagridStateByUserAndInterface(DatagridStateRequest request);        
    }
}
