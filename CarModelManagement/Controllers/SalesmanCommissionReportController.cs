using Microsoft.AspNetCore.Mvc;
using CarModelManagement.Model;
using CarModelManagement.Model.Model;
using CarModelManagement.Services.IServices;

namespace CarModelManagement.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class SalesmanCommissionReportController : ControllerBase
	{
		private readonly ResponseDto _responseDto = new ResponseDto();

		private readonly ISalesmanCommissionReportService _service;
		public SalesmanCommissionReportController(ISalesmanCommissionReportService service)
		{
			_service = service;
		}

		[HttpGet]
		public IActionResult GetSalesCommissionDtoReport([FromQuery] string? carClass, [FromQuery] string? brand)
		{
			try
			{
				List<SalesCommissionDto> report = _service.GetSalesCommission(carClass, brand);
				_responseDto.Result = report;
				_responseDto.IsSuccess = true;
				return Ok(_responseDto);
			}
			catch (Exception ex)
			{
				_responseDto.IsSuccess = false;
				_responseDto.Message = ex.Message;
				return Ok(_responseDto);
			}
		}

		
	}
}
