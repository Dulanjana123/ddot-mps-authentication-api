using DDOT.MPS.Auth.Api.Managers;
using Microsoft.AspNetCore.Mvc;
using Model.Dtos;
using Model.Request;
using Model.Response;

namespace DDOT.MPS.Auth.Api.Controllers
{
    [ApiController]
    public class DatagridStateController : CoreController
    {
        private readonly IDatagridStateManager _datagridStateManager;

        public DatagridStateController(IDatagridStateManager datagridStateManager)
        {
            _datagridStateManager = datagridStateManager;
        }

        [HttpPost]
        public async Task<IActionResult> CreateDatagridState([FromBody] DatagridStateDto datagridStateDto)
        {
            BaseResponse<DatagridStateResponseDto> createdDatagridState = await _datagridStateManager.CreateDatagridState(datagridStateDto);
            return Ok(createdDatagridState);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDatagridStateById(int id)
        {
            BaseResponse<DatagridStateResponseDto> response = await _datagridStateManager.GetDatagridStateById(id);
            return Ok(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDatagridState(int id, [FromBody] DatagridStateDto datagridStateDto)
        {
            BaseResponse<DatagridStateResponseDto> updatedDatagridState = await _datagridStateManager.UpdateDatagridState(id, datagridStateDto);
            return Ok(updatedDatagridState);
        }

        [HttpPost("datagrid")]
        public async Task<IActionResult> GetDatagridStateByUserAndInterface([FromBody] DatagridStateRequest request)
        {
            BaseResponse<DatagridStateResponseDto> response = await _datagridStateManager.GetDatagridStateByUserAndInterface(request);
            return Ok(response);
        }

    }
}
