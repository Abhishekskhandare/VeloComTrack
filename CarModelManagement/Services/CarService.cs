using CarModelManagement.Model;
using CarModelManagement.Services.IServices;
using System.Collections.Generic;
using System;
using System.Runtime.ConstrainedExecution;
using Microsoft.Data.SqlClient;
using CarModelManagement.Model.Model;

namespace CarModelManagement.Services
{
	public class CarService(IConfiguration configuration) : ICarService
	{
		private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;

		public ResponseDto AddCar(CarDto car, List<string> imagePaths)
		{
			ResponseDto responseDto = new ResponseDto();
			using (SqlConnection conn = new SqlConnection(_connectionString))
			{
				conn.Open();
				SqlTransaction transaction = conn.BeginTransaction(); // Use transaction to ensure atomicity

				try
				{
					// 1️⃣ Insert Car into `Cars` table and get the generated ID
					string insertCarQuery = @"
                    INSERT INTO Cars (Brand, Class, ModelName, ModelCode, Description, Features, Price, DateOfManufacturing, Active, SortOrder, CreatedDate, UpdatedDate)
                    VALUES (@Brand, @Class, @ModelName, @ModelCode, @Description, @Features, @Price, @DateOfManufacturing, @Active, @SortOrder, @CreatedDate , @UpdatedDate);
                    SELECT SCOPE_IDENTITY();"; // Get the last inserted ID

					SqlCommand carCmd = new SqlCommand(insertCarQuery, conn, transaction);
					carCmd.Parameters.AddWithValue("@Brand", car.Brand);
					carCmd.Parameters.AddWithValue("@Class", car.Class);
					carCmd.Parameters.AddWithValue("@ModelName", car.ModelName);
					carCmd.Parameters.AddWithValue("@ModelCode", car.ModelCode);
					carCmd.Parameters.AddWithValue("@Description", car.Description);
					carCmd.Parameters.AddWithValue("@Features", car.Features);
					carCmd.Parameters.AddWithValue("@Price", car.Price);
					carCmd.Parameters.AddWithValue("@DateOfManufacturing", car.DateOfManufacturing);
					carCmd.Parameters.AddWithValue("@Active", 1);
					carCmd.Parameters.AddWithValue("@SortOrder", 0);
					carCmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
					carCmd.Parameters.AddWithValue("@UpdatedDate", DateTime.Now);

					int carId = Convert.ToInt32(carCmd.ExecuteScalar()); // Get inserted CarId

					string insertImageQuery = "INSERT INTO CarImages (CarId, ImagePath) VALUES (@CarId, @ImagePath)";
					foreach (var imagePath in imagePaths)
					{
						SqlCommand imageCmd = new SqlCommand(insertImageQuery, conn, transaction);
						imageCmd.Parameters.AddWithValue("@CarId", carId);
						imageCmd.Parameters.AddWithValue("@ImagePath", imagePath);
						imageCmd.ExecuteNonQuery();
					}

					transaction.Commit(); // Commit transaction if everything is successful
					responseDto.IsSuccess = true;
					responseDto.Message = "Car added successfully";
					responseDto.Result = "CarId = "+carId;
					return responseDto;
				}
				catch (Exception ex)
				{
					transaction.Rollback(); // Rollback in case of error
					responseDto.IsSuccess = false;
					responseDto.Message = ex.Message;
					return responseDto;
				}
			};
		}

		public ResponseDto GetCars(string SearchByModelName = "", string SearchByModelCode = "", bool OrderbyManufacturingDate = true, bool SortByUpdatedDate = true)
		{
			var response = new ResponseDto();
			var cars = new List<CarModel>();

			using (SqlConnection conn = new SqlConnection(_connectionString))
			{
				conn.Open();
				string query = @"
				   SELECT C.Id, C.Brand, C.Class, C.ModelName, C.ModelCode, C.Description, C.Features, C.Price, 
                   C.DateOfManufacturing, C.Active, C.SortOrder, C.UpdatedDate,
                   STRING_AGG(CI.ImagePath, ', ') AS Images
					FROM Cars C
					LEFT JOIN CarImages CI ON C.Id = CI.CarId
					WHERE (@SearchByModelName = '' OR C.ModelName LIKE '%' + @SearchByModelName + '%')
					  AND (@SearchByModelCode = '' OR C.ModelCode LIKE '%' + @SearchByModelCode + '%')
					GROUP BY C.Id, C.Brand, C.Class, C.ModelName, C.ModelCode, C.Description, 
							 C.Features, C.Price, C.DateOfManufacturing, C.Active, C.SortOrder, C.UpdatedDate
					ORDER BY 
						CASE WHEN @OrderbyManufacturingDate = 1 THEN C.DateOfManufacturing END ASC,
						CASE WHEN @OrderbyManufacturingDate = 0 THEN C.DateOfManufacturing END DESC,
						CASE WHEN @SortByUpdatedDate = 1 THEN C.UpdatedDate END DESC,
						CASE WHEN @SortByUpdatedDate = 0 THEN C.UpdatedDate END ASC;
				";
				using (SqlCommand cmd = new SqlCommand(query, conn))
				{
					cmd.Parameters.AddWithValue("@SearchByModelName", SearchByModelName ?? "");
					cmd.Parameters.AddWithValue("@SearchByModelCode", SearchByModelCode ?? "");
					cmd.Parameters.AddWithValue("@OrderbyManufacturingDate", OrderbyManufacturingDate);
					cmd.Parameters.AddWithValue("@SortByUpdatedDate", SortByUpdatedDate);

					SqlDataReader reader = cmd.ExecuteReader();
					while (reader.Read())
					{
						cars.Add(new CarModel
						{
							Id = reader.GetInt32(0),
							Brand = reader.GetString(1),
							Class = reader.GetString(2),
							ModelName = reader.GetString(3),
							ModelCode = reader.GetString(4),
							Description = reader.GetString(5),
							Features = reader.GetString(6),
							Price = reader.GetDecimal(7),
							DateOfManufacturing = reader.GetDateTime(8),
							Active = reader.GetBoolean(9),
							SortOrder = reader.GetInt32(10),
							UpdatedDate = reader.GetDateTime(11),
							ImagePaths = reader.IsDBNull(12)
											? new List<string> { "/uploads/default-car.jpg" }  // Default image
											: new List<string> { reader.GetString(12).Split(", ").FirstOrDefault() ?? "/uploads/default-car.jpg" }
						});
					}
				}
			}

			response.IsSuccess = true;
			response.Result = cars;
			response.Message = "Cars retrieved successfully.";
			return response;
		}

	}
}
