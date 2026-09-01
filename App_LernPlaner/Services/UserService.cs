using App_LernPlaner.Models;
using App_LernPlaner.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App_LernPlaner.Services
{
    public class UserService
    {
        private readonly UserRepository _userRepository;
        public UserService()
        {
            _userRepository = new UserRepository();
        }

        public async Task<UserProfile?> GetUserProfileByIdAsync(int UserId)
        {
            return await _userRepository.GetUserProfileByIdAsync(UserId);
        }

        public async Task UpdateUserProfile(UserProfile profile)
        {
            await _userRepository.UpdateUserProfileAsync(profile);
        }
    }
}
