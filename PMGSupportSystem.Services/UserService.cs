﻿using ClosedXML.Excel;
using Google.Apis.Auth;
using PMGSupportSystem.Repositories;
using PMGSupportSystem.Repositories.Models;

namespace PMGSupportSystem.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(IEnumerable<User> Items, int TotalCount)> GetPaginatedUsersAsync(int page, int pageSize)
        {
            var users = await _unitOfWork.UserRepository.GetPagedListAsync(page, pageSize);
            return users;
        }

        public async Task UpdateUserAsync(User user)
        {
            await _unitOfWork.UserRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            return user;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            var user = await _unitOfWork.UserRepository.GetByEmailAsync(email);
            return user;
        }

        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            var users = await _unitOfWork.UserRepository.GetAllAsync();
            return users;
        }

        public async Task<string> LoginAsync(string idToken)
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);
            var googleId = payload.Subject;
            var email = payload.Email;

            var user = await _unitOfWork.UserRepository.GetByGoogleIdAsync(googleId);
            if (user == null)
            {
                user = await _unitOfWork.UserRepository.GetByEmailAsync(email);

                if (user == null) throw new UnauthorizedAccessException("Unexisted user!");
                if (!user.Status!.Value) throw new UnauthorizedAccessException("Your account is locked!");

                if (string.IsNullOrEmpty(user.GoogleId))
                {
                    user.GoogleId = googleId;
                    await _unitOfWork.UserRepository.UpdateAsync(user);
                }
                else
                {
                    if (user.GoogleId != googleId)
                    {
                        throw new UnauthorizedAccessException("Google ID mismatch!");
                    }
                }
            }
            if (user.Email != email)
            {
                user.Email = email;
                await _unitOfWork.UserRepository.UpdateAsync(user);
            }

            var jwt = _unitOfWork.JwtHelper.GenerateToken(user);
            return jwt;
        }

        public async Task<IEnumerable<User>> ImportUsersFromExcelAsync(Stream excelStream)
        {
            var users = new List<User>();

            using (var workbook = new XLWorkbook(excelStream))
            {
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RangeUsed()!.RowsUsed().Skip(1);

                foreach (var row in rows)
                {
                    var user = new User
                    {
                        FullName = row.Cell(1).GetValue<string>()?.Trim(),
                        Email = row.Cell(2).GetValue<string>()?.Trim(),
                        GoogleId = null,
                        Role = row.Cell(3).GetValue<string>()?.Trim(),
                        CreatedAt = DateTime.Now,
                        Code = !string.IsNullOrWhiteSpace(row.Cell(4).GetValue<string>()?.Trim()) ? row.Cell(4).GetValue<string>()?.Trim() : null,
                        Status = true
                    };

                    users.Add(user);
                }
            }

            await _unitOfWork.UserRepository.AddRangeAsync(users);
            await _unitOfWork.SaveChangesAsync();

            return users;
        }
    }
}
