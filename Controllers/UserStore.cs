using LotniskoAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LotniskoAPI.Data;
using LotniskoAPI.Controllers;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace LotniskoAPI.Controllers
{
    public class CustomUserStore : IUserStore<User>, IUserPasswordStore<User>
    {
        private readonly AppDbContext _context;

        public CustomUserStore(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<User> FindByNameAsync(string userName, CancellationToken cancellationToken)
        {
            return await _context.Users.SingleOrDefaultAsync(u => u.UserName == userName, cancellationToken);
        }

        public async Task<string> GetPasswordHashAsync(User user, CancellationToken cancellationToken)
        {
            return user.Password;
        }

        public async Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken)
        {
            return !string.IsNullOrEmpty(user.Password);
        }







        Task<IdentityResult> IUserStore<User>.DeleteAsync(User user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        void IDisposable.Dispose()
        {
            throw new NotImplementedException();
        }

        Task<User?> IUserStore<User>.FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<string?> IUserStore<User>.GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<string> IUserStore<User>.GetUserIdAsync(User user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<string?> IUserStore<User>.GetUserNameAsync(User user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task IUserStore<User>.SetNormalizedUserNameAsync(User user, string? normalizedName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task IUserPasswordStore<User>.SetPasswordHashAsync(User user, string? passwordHash, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task IUserStore<User>.SetUserNameAsync(User user, string? userName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<IdentityResult> IUserStore<User>.UpdateAsync(User user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        // Implement other methods like DeleteAsync, UpdateAsync, etc.
    }

}
