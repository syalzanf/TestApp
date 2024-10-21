using TestApp.Data;
using TestApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestApp.Services  // Tambahkan namespace ini
{
    public class UserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context) 
        {
            _context = context;
        }

        public async Task<List<UsersEntity>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<UsersEntity> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task CreateUserAsync(UsersEntity user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(UsersEntity user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }
    }
}

/*      public async Task<bool> RemoveUserAsync(int id)
      {
          try
          {
              User user = await _context.User.FirstOrDefaultAsync(x => x.Id == id);
              if (user != null)
              {
                  user.DeletedAt = DateTime.UtcNow;
                  _context.User.Update(user);
                  await _context.SaveChangesAsync();
                  return true;
              }
              return false; // User not found
          }
          catch (Exception ex)
          {
              _logger.LogError(ex, $"Error removing user with ID {id}");
              return false; // Indicate failure
          }
      }
  }
}*/