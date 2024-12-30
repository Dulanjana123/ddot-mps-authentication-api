using AutoMapper;
using DataAccess.Contexts;
using DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Model.Dtos;
using Model.Request;

namespace DataAccess.Repositories
{
    public class MpsDatagridStateRepository : IMpsDatagridStateRepository
    {
        private readonly MpsDbContext _dbContext;
        private readonly IMapper _mapper;

        public MpsDatagridStateRepository(MpsDbContext mpsDbContext, IMapper mapper)
        {
            _dbContext = mpsDbContext ?? throw new ArgumentNullException(nameof(mpsDbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<DatagridStateDto> CreateDatagridState(SettingsGridState datagridState)
        {
            _dbContext.Add(datagridState);
            await _dbContext.SaveChangesAsync();
            DatagridStateDto newDatagridState = _mapper.Map<DatagridStateDto>(datagridState);
            return newDatagridState;
        }

        public async Task<SettingsGridState> GetDatagridStateById(int id)
        {
            return await _dbContext.SettingsGridStates.Where(t => t.SettingsGridStateId == id).FirstOrDefaultAsync();
        }

        public async Task<SettingsGridState> GetDatagridStateByUserAndInterface(DatagridStateRequest request)
        {
            return await _dbContext.SettingsGridStates.Where(t => t.UserId == request.UserId && t.InterfaceId == request.InterfaceId).FirstOrDefaultAsync();
        }

        public async Task<DatagridStateDto> UpdateDatagridState(SettingsGridState datagridState)
        {
            _dbContext.Update(datagridState);
            await _dbContext.SaveChangesAsync();
            DatagridStateDto updatedDatagridState = _mapper.Map<DatagridStateDto>(datagridState);
            return updatedDatagridState;
        }
    }
}
