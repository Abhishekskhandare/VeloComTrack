using System.ComponentModel.DataAnnotations;

namespace CarModelManagement.Model
{
	public class CarDto
	{
		[Required(ErrorMessage = "Brand is required.")]
		[BrandValidation(ErrorMessage = "Invalid brand. Allowed values: Audi, Jaguar, Land Rover, Renault.")]
		public string Brand { get; set; }

		[Required(ErrorMessage = "Class is required.")]
		[ClassValidation(ErrorMessage = "Invalid class. Allowed values: A-Class, B-Class, C-Class.")]
		public string Class { get; set; }

		[Required(ErrorMessage = "Model Name is required.")]
		public string ModelName { get; set; }

		[Required(ErrorMessage = "Model Code is required.")]
		[StringLength(10, MinimumLength = 10, ErrorMessage = "Model Code must be exactly 10 alphanumeric characters.")]
		[RegularExpression("^[a-zA-Z0-9]{10}$", ErrorMessage = "Model Code must be alphanumeric and 10 characters long.")]
		public string ModelCode { get; set; }

		[Required(ErrorMessage = "Description is required.")]
		public string Description { get; set; }  // Use FCK Editor in frontend

		[Required(ErrorMessage = "Features are required.")]
		public string Features { get; set; }  // Use FCK Editor in frontend

		[Required(ErrorMessage = "Price is required.")]
		[Range(1, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
		public decimal Price { get; set; }

		[Required(ErrorMessage = "Date of Manufacturing is required.")]
		public DateTime DateOfManufacturing { get; set; }

		//public bool Active { get; set; } = true;  // Default to true

		//[Range(1, int.MaxValue, ErrorMessage = "Sort order must be a positive number.")]
		//public int SortOrder { get; set; }

		[MaxLength(4, ErrorMessage = "Maximum of 4 images allowed.")]
		public List<IFormFile> Images { get; set; }
	}
	
	// Custom Validation for Brand
	public class BrandValidationAttribute : ValidationAttribute
	{
		private readonly string[] allowedBrands = { "Audi", "Jaguar", "Land Rover", "Renault" };

		public override bool IsValid(object value)
		{
			return value is string brand && Array.Exists(allowedBrands, b => b.Equals(brand, StringComparison.OrdinalIgnoreCase));
		}
	}

	// Custom Validation for Class
	public class ClassValidationAttribute : ValidationAttribute
	{
		private readonly string[] allowedClasses = { "A-Class", "B-Class", "C-Class" };

		public override bool IsValid(object value)
		{
			return value is string carClass && Array.Exists(allowedClasses, c => c.Equals(carClass, StringComparison.OrdinalIgnoreCase));
		}
	}

}