using System;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using CarModelManagement.Services.IServices;

namespace CarModelManagement.Services
{
	
	public class AuthService(IConfiguration configuration) : IAuthService
	{
		private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")??string.Empty;

		public bool ValidateUser(string username, string password)
		{
			using (SqlConnection conn = new SqlConnection(_connectionString))
			{
				conn.Open();
				string query = "SELECT PasswordHash, Salt FROM Users WHERE Username = @Username";
				using (SqlCommand cmd = new SqlCommand(query, conn))
				{
					cmd.Parameters.AddWithValue("@Username", username);
					using (SqlDataReader reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							string storedHash = reader["PasswordHash"].ToString();
							string salt = reader["Salt"].ToString();
							string hashedPassword = HashPassword(password, salt);

							return storedHash == hashedPassword;
						}
					}
				}
			}
			return false;
		}

		public bool RegisterUser(string username, string password, string role)
		{
			string salt = GenerateSalt();
			string passwordHash = HashPassword(password, salt);

			using (SqlConnection conn = new SqlConnection(_connectionString))
			{
				conn.Open();
				SqlTransaction transaction = conn.BeginTransaction();
				try
				{
					// Insert User
					string insertUserQuery = "INSERT INTO Users (Username, PasswordHash, Salt) OUTPUT INSERTED.Id VALUES (@Username, @PasswordHash, @Salt)";
					int userId;

					using (SqlCommand cmd = new SqlCommand(insertUserQuery, conn, transaction))
					{
						cmd.Parameters.AddWithValue("@Username", username);
						cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
						cmd.Parameters.AddWithValue("@Salt", salt);
						userId = (int)cmd.ExecuteScalar();
					}

					// Get RoleId
					string getRoleIdQuery = "SELECT Id FROM Roles WHERE RoleName = @Role";
					int roleId;

					using (SqlCommand cmd = new SqlCommand(getRoleIdQuery, conn, transaction))
					{
						cmd.Parameters.AddWithValue("@Role", role);
						object result = cmd.ExecuteScalar();
						if (result == null)
							throw new Exception("Role not found.");
						roleId = (int)result;
					}

					// Assign Role to User
					string insertUserRoleQuery = "INSERT INTO UserRoles (UserId, RoleId) VALUES (@UserId, @RoleId)";
					using (SqlCommand cmd = new SqlCommand(insertUserRoleQuery, conn, transaction))
					{
						cmd.Parameters.AddWithValue("@UserId", userId);
						cmd.Parameters.AddWithValue("@RoleId", roleId);
						cmd.ExecuteNonQuery();
					}

					transaction.Commit();
					return true;
				}
				catch
				{
					transaction.Rollback();
					return false;
				}
			}
		}

		public List<string> GetUserRoles(string username)
		{
			List<string> roles = new List<string>();

			using (SqlConnection conn = new SqlConnection(_connectionString))
			{
				conn.Open();
				string query = @"
                SELECT r.RoleName FROM Roles r
                JOIN UserRoles ur ON r.Id = ur.RoleId
                JOIN Users u ON ur.UserId = u.Id
                WHERE u.Username = @Username";

				using (SqlCommand cmd = new SqlCommand(query, conn))
				{
					cmd.Parameters.AddWithValue("@Username", username);
					using (SqlDataReader reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							roles.Add(reader["RoleName"].ToString());
						}
					}
				}
			}
			return roles;
		}

		#region Private methods


		private string HashPassword(string password, string salt)
		{
			using (SHA256 sha256 = SHA256.Create())
			{
				byte[] bytes = Encoding.UTF8.GetBytes(password + salt);
				byte[] hash = sha256.ComputeHash(bytes);
				return Convert.ToBase64String(hash);
			}
		}
		private string GenerateSalt()
		{
			byte[] saltBytes = new byte[16];
			using (var rng = new RNGCryptoServiceProvider())
			{
				rng.GetBytes(saltBytes);
			}
			return Convert.ToBase64String(saltBytes);
		}



		#endregion
	}

}
