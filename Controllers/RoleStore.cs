using LotniskoAPI.Data;
using LotniskoAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LotniskoAPI.Controllers
{
    public class RoleStore : IRoleStore<Role>
    {
        private readonly AppDbContext _context;

        public RoleStore(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IdentityResult> CreateAsync(Role role, CancellationToken cancellationToken)
        {
            _context.Roles.Add(role);
            await _context.SaveChangesAsync(cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<Role?> FindByNameAsync(string roleName, CancellationToken cancellationToken)
        {
            var role = await _context.Roles.SingleOrDefaultAsync(r => r.Name == roleName, cancellationToken);
            return role;
        }










        Task<IdentityResult> IRoleStore<Role>.DeleteAsync(Role role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        void IDisposable.Dispose()
        {
            throw new NotImplementedException();
        }

        Task<Role?> IRoleStore<Role>.FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<string?> IRoleStore<Role>.GetNormalizedRoleNameAsync(Role role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<string> IRoleStore<Role>.GetRoleIdAsync(Role role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<string?> IRoleStore<Role>.GetRoleNameAsync(Role role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task IRoleStore<Role>.SetNormalizedRoleNameAsync(Role role, string? normalizedName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task IRoleStore<Role>.SetRoleNameAsync(Role role, string? roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<IdentityResult> IRoleStore<Role>.UpdateAsync(Role role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        // Implement other methods like DeleteAsync, UpdateAsync, etc.
    }

}
