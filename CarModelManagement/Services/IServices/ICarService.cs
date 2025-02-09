using CarModelManagement.Model;

namespace CarModelManagement.Services.IServices
{
	public interface ICarService
	{
		ResponseDto AddCar(CarDto model, List<string> imagePaths);
		ResponseDto GetCars(string SearchByModelName = "", string SearchByModelCode = "", bool OrderbyManufacturingDate = true, bool SortByUpdatedDate = true);

	}
}
