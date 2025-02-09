namespace CarModelManagement.Model
{
	public class UserDto
	{
		public string Username { get; set; }
		public string Password { get; set; }
		public string Role { get; set; }

		public UserDto(string username, string password = "", string role = "")
		{
			Username = username;
			Password = password;
			Role = role;
		}
	}
}
