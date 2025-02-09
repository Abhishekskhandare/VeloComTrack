using CarModelManagement.Model;

namespace CarModelManagement.Services.IServices
{
	public interface ISalesmanCommissionReportService
	{
		public List<SalesCommissionDto> GetSalesCommission(string? carClass, string? brand);

	}
}
