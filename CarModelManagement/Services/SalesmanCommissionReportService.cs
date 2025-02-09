using CarModelManagement.Model;
using CarModelManagement.Services.IServices;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CarModelManagement.Services
{
	public class SalesmanCommissionReportService(IConfiguration configuration) : ISalesmanCommissionReportService
	{
		private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;

		public List<SalesCommissionDto> GetSalesCommission(string carClass = "", string brand = "")
		{
			List<SalesCommissionDto> report = new List<SalesCommissionDto>();
			using (SqlConnection conn = new SqlConnection(_connectionString))
			{
				conn.Open();
				string query = @"SELECT s.SalesmanName, b.BrandName, sal.Class, sal.NumberOfCarsSold,
                            (CASE sal.Class 
                                WHEN 'A' THEN b.ClassA_Commission
                                WHEN 'B' THEN b.ClassB_Commission
                                WHEN 'C' THEN b.ClassC_Commission
                            END) AS ClassCommission,
                            b.FixedCommission,
                            (CASE WHEN s.LastYearTotalSales > 500000 AND sal.Class = 'A' THEN 'Yes' ELSE 'No' END) AS AdditionalCommission,
                            (sal.NumberOfCarsSold * (b.FixedCommission + 
                            (CASE sal.Class 
                                WHEN 'A' THEN b.ClassA_Commission
                                WHEN 'B' THEN b.ClassB_Commission
                                WHEN 'C' THEN b.ClassC_Commission
                            END))) AS FinalSaleAmount
                            FROM Sales sal
                            JOIN Salesmen s ON sal.SalesmanID = s.SalesmanID
                            JOIN Brands b ON sal.BrandID = b.BrandID
                            WHERE (@CarClass IS NULL OR sal.Class = @CarClass)
                            AND (@Brand IS NULL OR b.BrandName = @Brand)";
				using (SqlCommand cmd = new SqlCommand(query, conn))
				{
					cmd.Parameters.AddWithValue("@CarClass", (object)carClass ?? DBNull.Value);
					cmd.Parameters.AddWithValue("@Brand", (object)brand ?? DBNull.Value);
					using (SqlDataReader reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							report.Add(new SalesCommissionDto
							{
								SalesmanName = reader["SalesmanName"].ToString(),
								CarBrand = reader["BrandName"].ToString(),
								CarClass = reader["Class"].ToString(),
								NumberOfCarsSold = Convert.ToInt32(reader["NumberOfCarsSold"]),
								ClassWiseCommission = Convert.ToDecimal(reader["ClassCommission"]),
								FixedCommission = Convert.ToDecimal(reader["FixedCommission"]),
								AdditionalCommission = reader["AdditionalCommission"].ToString(),
								FinalSaleAmount = Convert.ToDecimal(reader["FinalSaleAmount"])
							});
						}
					}
				}
			}
			return report;
		}
	}
}
