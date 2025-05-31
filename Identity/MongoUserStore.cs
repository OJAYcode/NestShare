using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Thrift.Models;
using Thrift.Repositories;

namespace Thrift.Identity
{    public class MongoUserStore : 
        IUserStore<User>, 
        IUserEmailStore<User>,
        IUserPasswordStore<User>,
        IUserRoleStore<User>,
        IUserPhoneNumberStore<User>
    {
        private readonly UserRepository _userRepository;
        
        public MongoUserStore(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        
        #region IUserStore
        
        public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if (user == null) throw new ArgumentNullException(nameof(user));
            
            await _userRepository.CreateAsync(user);
            
            return IdentityResult.Success;
        }
        
        public async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if (user == null) throw new ArgumentNullException(nameof(user));
            
            await _userRepository.DeleteAsync(user.Id);
            
            return IdentityResult.Success;
        }
        
        public async Task<User> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            return await _userRepository.GetByIdAsync(userId);
        }
        
        public async Task<User> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            return await _userRepository.GetByEmailAsync(normalizedUserName);
        }
        
        public Task<string> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if (user == null) throw new ArgumentNullException(nameof(user));
            
            return Task.FromResult(user.Email.ToUpper());
        }
        
        public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if (user == null) throw new ArgumentNullException(nameof(user));
            
            return Task.FromResult(user.Id);
        }
          public Task<string> GetUserNameAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if (user == null) throw new ArgumentNullException(nameof(user));
            
            return Task.FromResult(user.UserName ?? user.Email);
        }
        
        public Task SetNormalizedUserNameAsync(User user, string normalizedName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if (user == null) throw new ArgumentNullException(nameof(user));
            
            // We use email as username, so normalization is handled in email section
            
            return Task.CompletedTask;
        }
        
        public Task SetUserNameAsync(User user, string userName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if (user == null) throw new ArgumentNullException(nameof(user));
            
            user.UserName = userName;
            user.Email = userName; // Keep email and username in sync
            
            return Task.CompletedTask;
        }
        
        public async Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if (user == null) throw new ArgumentNullException(nameof(user));
            
            await _userRepository.UpdateAsync(user.Id, user);
            
            return IdentityResult.Success;
        }
        
        #endregion
        
        #region IUserEmailStore
        
        public async Task<User> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            return await _userRepository.GetByEmailAsync(normalizedEmail);
        }
        
        public Task<string> GetEmailAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if (user == null) throw new ArgumentNullException(nameof(user));
            
            return Task.FromResult(user.Email);
        }
        
        public Task<bool> GetEmailConfirmedAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if (user == null) throw new ArgumentNullException(nameof(user));
            
            return Task.FromResult(user.EmailConfirmed);
        }
        
        public Task<string> GetNormalizedEmailAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if (user == null) throw new ArgumentNullException(nameof(user));
            
            return Task.FromResult(user.Email.ToUpper());
        }
        
        public Task SetEmailAsync(User user, string email, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if (user == null) throw new ArgumentNullException(nameof(user));
            
            user.Email = email;
            
            return Task.CompletedTask;
        }
        
        public Task SetEmailConfirmedAsync(User user, bool confirmed, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if (user == null) throw new ArgumentNullException(nameof(user));
            
            user.EmailConfirmed = confirmed;
            
            return Task.CompletedTask;
        }
        
        public Task SetNormalizedEmailAsync(User user, string normalizedEmail, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if (user == null) throw new ArgumentNullException(nameof(user));
            
            // We store the original email and use ToUpper() when needed
            
            return Task.CompletedTask;
        }
        
        #endregion
        
        #region IUserPasswordStore
        
        public Task<string> GetPasswordHashAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if (user == null) throw new ArgumentNullException(nameof(user));
            
            return Task.FromResult(user.PasswordHash);
        }
        
        public Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if (user == null) throw new ArgumentNullException(nameof(user));
            
            return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
        }
        
        public Task SetPasswordHashAsync(User user, string passwordHash, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if (user == null) throw new ArgumentNullException(nameof(user));
            
            user.PasswordHash = passwordHash;
            
            return Task.CompletedTask;
        }
        
        #endregion
        
        #region IUserRoleStore
        
        public Task AddToRoleAsync(User user, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if (user == null) throw new ArgumentNullException(nameof(user));
            
            if (!user.Roles.Contains(roleName))
            {
                user.Roles.Add(roleName);
            }
            
            return Task.CompletedTask;
        }
        
        public Task<IList<string>> GetRolesAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if (user == null) throw new ArgumentNullException(nameof(user));
            
            return Task.FromResult<IList<string>>(user.Roles);
        }
        
        public Task<IList<User>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            // This would require a separate query that isn't implemented in our repository
            // For simplicity, returning an empty list
            return Task.FromResult<IList<User>>(new List<User>());
        }
        
        public Task<bool> IsInRoleAsync(User user, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if (user == null) throw new ArgumentNullException(nameof(user));
            
            return Task.FromResult(user.Roles.Contains(roleName));
        }
        
        public Task RemoveFromRoleAsync(User user, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if (user == null) throw new ArgumentNullException(nameof(user));
            
            if (user.Roles.Contains(roleName))
            {
                user.Roles.Remove(roleName);
            }
            
            return Task.CompletedTask;
        }
          #endregion
        
        #region IUserPhoneNumberStore
        
        public Task<string?> GetPhoneNumberAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if (user == null) throw new ArgumentNullException(nameof(user));
            
            return Task.FromResult(user.PhoneNumber);
        }
        
        public Task<bool> GetPhoneNumberConfirmedAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if (user == null) throw new ArgumentNullException(nameof(user));
            
            // For now, we'll assume phone numbers are not confirmed by default
            // You can add a PhoneNumberConfirmed property to the User model if needed
            return Task.FromResult(false);
        }
        
        public Task SetPhoneNumberAsync(User user, string? phoneNumber, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if (user == null) throw new ArgumentNullException(nameof(user));
            
            user.PhoneNumber = phoneNumber;
            
            return Task.CompletedTask;
        }
        
        public Task SetPhoneNumberConfirmedAsync(User user, bool confirmed, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if (user == null) throw new ArgumentNullException(nameof(user));
            
            // For now, we'll just complete the task since we don't have a PhoneNumberConfirmed property
            // You can add this property to the User model if phone number confirmation is needed
            
            return Task.CompletedTask;
        }
        
        #endregion
        
        public void Dispose()
        {
            // Nothing to dispose
        }
    }
}