namespace CarModelManagement.Model
{
	public class SalesCommissionDto
	{
		public string SalesmanName { get; set; }
		public string CarBrand { get; set; }
		public string CarClass { get; set; }
		public int NumberOfCarsSold { get; set; }
		public decimal ClassWiseCommission { get; set; }
		public decimal FixedCommission { get; set; }
		public string AdditionalCommission { get; set; }
		public decimal FinalSaleAmount { get; set; }
	}
}
