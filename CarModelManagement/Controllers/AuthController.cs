using CarModelManagement.Model;
using CarModelManagement.Services;
using CarModelManagement.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Collections;
using System.Data;

namespace CarModelManagement.Controllers
{
	[Route("api")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly IAuthService _authService;
		private readonly ResponseDto _responseDto;

        public AuthController(IAuthService authService)
		{
			_authService = authService;
			_responseDto = new ResponseDto();
        }

		[HttpPost("register")]
		public ActionResult RegisterUser(string username, string password, string role)
		{
			try
			{
				bool IsSuccess = _authService.RegisterUser(username, password, role);
				_responseDto.IsSuccess = IsSuccess;
				_responseDto.Result = new UserDto(username, password, role);
				_responseDto.Message = IsSuccess ? Constants.RegistrationSuccess : Constants.RegistrationFailed;
				return IsSuccess ? Ok(_responseDto) : BadRequest(_responseDto);
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message);
			}
		}

		[HttpPost("login")]
		public ActionResult LoginUser(string username, string password)
		{
			try
			{
				var IsSuccess = _authService.ValidateUser(username, password);
				_responseDto.IsSuccess = IsSuccess;
				_responseDto.Result = new UserDto(username);
				_responseDto.Message = IsSuccess ? Constants.RegistrationSuccess : Constants.RegistrationFailed;
				return IsSuccess ? Ok(_responseDto) : BadRequest(_responseDto);
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message);
			}
		}
		
	}
}
