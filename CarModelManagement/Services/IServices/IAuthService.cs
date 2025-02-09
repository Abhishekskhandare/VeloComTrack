namespace CarModelManagement.Services.IServices
{
	public interface IAuthService
	{
		bool RegisterUser(string username, string password, string role);
		bool ValidateUser(string username, string password);
	}
}
