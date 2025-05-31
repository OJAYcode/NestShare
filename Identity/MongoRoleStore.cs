using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Thrift.Data;
using MongoDB.Driver;

namespace Thrift.Identity
{
    public class MongoRoleStore : IRoleStore<IdentityRole>
    {
        private readonly IMongoCollection<IdentityRole> _roles;

        public MongoRoleStore(MongoDbContext context)
        {
            _roles = context.GetCollection<IdentityRole>("roles");
        }

        public async Task<IdentityResult> CreateAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (role == null) throw new ArgumentNullException(nameof(role));

            await _roles.InsertOneAsync(role, null, cancellationToken);
            
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (role == null) throw new ArgumentNullException(nameof(role));

            await _roles.DeleteOneAsync(r => r.Id == role.Id, cancellationToken);
            
            return IdentityResult.Success;
        }

        public async Task<IdentityRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await _roles.Find(r => r.Id == roleId).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IdentityRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await _roles.Find(r => r.NormalizedName == normalizedRoleName).FirstOrDefaultAsync(cancellationToken);
        }

        public Task<string> GetNormalizedRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (role == null) throw new ArgumentNullException(nameof(role));

            return Task.FromResult(role.NormalizedName);
        }

        public Task<string> GetRoleIdAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (role == null) throw new ArgumentNullException(nameof(role));

            return Task.FromResult(role.Id);
        }

        public Task<string> GetRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (role == null) throw new ArgumentNullException(nameof(role));

            return Task.FromResult(role.Name);
        }

        public Task SetNormalizedRoleNameAsync(IdentityRole role, string normalizedName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (role == null) throw new ArgumentNullException(nameof(role));

            role.NormalizedName = normalizedName;

            return Task.CompletedTask;
        }

        public Task SetRoleNameAsync(IdentityRole role, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (role == null) throw new ArgumentNullException(nameof(role));

            role.Name = roleName;

            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (role == null) throw new ArgumentNullException(nameof(role));

            await _roles.ReplaceOneAsync(r => r.Id == role.Id, role, new ReplaceOptions(), cancellationToken);
            
            return IdentityResult.Success;
        }

        public void Dispose()
        {
            // Nothing to dispose
        }
    }
}
