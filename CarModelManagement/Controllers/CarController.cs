using CarModelManagement.Model;
using CarModelManagement.Model.Model;
using CarModelManagement.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace CarModelManagement.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CarController(IWebHostEnvironment env, ICarService carService, IConfiguration configuration) : ControllerBase
	{
		private readonly IWebHostEnvironment _env = env;
		private readonly ICarService _carService = carService;
		private readonly string _uploadPath = configuration["UploadSettings:UploadPath"]??string.Empty;

		[HttpPost("add")]
		public async Task<IActionResult> AddCar([FromForm] CarDto model)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}
				if (model.Images != null && model.Images.Count > 4)
				{
					return BadRequest("Maximum of 4 images allowed.");
				}
				List<string> imagePaths = await ConvertImgtoImgPath(model);
				ResponseDto response = _carService.AddCar(model, imagePaths);
				return Ok(response);
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message);
			}
		}


		[HttpGet("list")]
		public IActionResult GetCars([FromQuery] string SearchByModelName = "", [FromQuery] string SearchByModelCode = "",
							 [FromQuery] bool OrderbyManufacturingDate = true, [FromQuery] bool SortByUpdatedDate = true)
		{
			try
			{
				var response = _carService.GetCars(SearchByModelName, SearchByModelCode, OrderbyManufacturingDate, SortByUpdatedDate);
				if (response.IsSuccess && response.Result is List<CarModel> cars)
				{
					foreach (var car in cars)
					{
						if (car.ImagePaths.Any())
						{
							string directory =  Path.Combine(_env.WebRootPath) ??_uploadPath;
							string imagePath = Path.Combine(directory, "wwwroot", car.ImagePaths.First());
							imagePath = directory+imagePath;
							if (System.IO.File.Exists(imagePath))
							{
								byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);
								string base64Image = Convert.ToBase64String(imageBytes);
								car.ImageBase64 = $"data:image/jpeg;base64,{base64Image}"; // Assuming JPEG format
							}
							else
							{
								car.ImageBase64 = null; // Handle missing image
							}
						}
					}
				}
				return Ok(response);
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message);
			}
		}

		#region private Methods
		private async Task<List<string>>  ConvertImgtoImgPath(CarDto model)
		{
			List<string> imagePaths = new List<string>();
			if (model.Images != null)
			{
				foreach (var image in model.Images)
				{
					if (image.Length > 0)
					{
						string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
						if (!Directory.Exists(uploadsFolder))
						{
							Directory.CreateDirectory(uploadsFolder);
						}

						string filePath = Path.Combine(uploadsFolder, image.FileName);
						using (var fileStream = new FileStream(filePath, FileMode.Create))
						{
							await image.CopyToAsync(fileStream);
						}
						imagePaths.Add($"/uploads/{image.FileName}");
					}
				}
			}

			return imagePaths;
		}
		#endregion

	}
}
