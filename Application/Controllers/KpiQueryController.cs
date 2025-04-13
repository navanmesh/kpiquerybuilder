using Core.Entities;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Application.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class KpiQueryController : ControllerBase
    {
        private readonly IKpiQueryService _kpiQueryService;

        public KpiQueryController(IKpiQueryService kpiQueryService)
        {
            _kpiQueryService = kpiQueryService;
        }

        [HttpPost("generate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<string> GenerateQuery([FromBody] KpiRequest request)
        {
            try
            {
                var query = _kpiQueryService.GenerateQuery(request);
                return Ok(new { query = query });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}