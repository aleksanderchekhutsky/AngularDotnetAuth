using AuthAPI.Interfaces;
using AuthAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System.Text.RegularExpressions;
using System.Text;
using AuthAPI.Context;
using AuthAPI.Helpers;

namespace AuthAPI.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _dbContext;
        private readonly IJwtService _jwtService;
        public UserService(AppDbContext dbContext, IJwtService jwtService)
        {
            _dbContext = dbContext;
            _jwtService = jwtService;
        }

        public async Task<RegistrationResult> RegisterUser(User userModel)
        {
            try
            {
                if (await IsExistName(userModel.UserName))
                {
                    return new RegistrationResult(false, "Username already exists");
                }

                if (await IsExistEmail(userModel.Email))
                {
                    return new RegistrationResult(false, "Email already exists");
                }

                string passwordResult = CheckPasswordStrength(userModel.Password);
                if (!string.IsNullOrEmpty(passwordResult))
                {
                    return new RegistrationResult(false, passwordResult);
                }

                userModel.Password = PasswordHasher.HashPassword(userModel.Password);
                userModel.Role = "User";
                userModel.Token = "";

                await _dbContext.Users.AddAsync(userModel);
                await _dbContext.SaveChangesAsync();

                return new RegistrationResult(true, "User registered successfully");
            }
            catch (Exception ex)
            {
                return new RegistrationResult(false, $"Registration failed: {ex.Message}");
            }
        }
        public async Task<RegistrationResult> AuthenticateUser(User userModel)
        {
            try
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.UserName == userModel.UserName);
                if (user == null)
                {
                    return new RegistrationResult() { Success = false, Message = "User not fount!" };
                }

                if (!PasswordHasher.VerifyPassword(user.Password, user.Password))
                {
                    return new RegistrationResult() { Success = false, Message = "Password is incorect! " };
                }
                user.Token = _jwtService.CreateJwt((User)user);

                return new RegistrationResult() { Success = true, Token = user.Token, Message = "Login Succes" };

            }
            catch (Exception ex)
            {
                return new RegistrationResult(false, $"Autheticate failed error ==> {ex.Message}");
                throw;
            }
        }

        private Task<bool> IsExistName(string userName)
        {
            return _dbContext.Users.AnyAsync(u => u.UserName == userName);
        }

        private Task<bool> IsExistEmail(string email)
        {
            return _dbContext.Users.AnyAsync(u => u.Email == email);
        }
        private string CheckPasswordStrength(string password)
        {
            StringBuilder str = new StringBuilder();

            if (password.Length < 8)
            {
                str.AppendLine("Minimum password length should be 8");
            }
            if (!Regex.IsMatch(password, "[a-z]") ||
                !Regex.IsMatch(password, "[A-Z]") ||
                !Regex.IsMatch(password, "[0-9]"))
            {
                str.AppendLine("Password should contain at least one lowercase letter, one uppercase letter, and one digit");
            }

            return str.ToString();
        }
    }
}
