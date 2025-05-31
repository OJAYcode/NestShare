using MongoDB.Driver;
using Thrift.Data;
using Thrift.Models;

namespace Thrift.Repositories
{
    public class ThriftGroupRepository
    {
        private readonly IMongoCollection<ThriftGroup> _groupsCollection;
        private readonly IMongoCollection<ThriftGroupMember> _membersCollection;
        private readonly IMongoCollection<GroupTransaction> _transactionsCollection;
        private readonly IMongoCollection<GroupInvitation> _invitationsCollection;

        public ThriftGroupRepository(MongoDbContext context)
        {
            _groupsCollection = context.GetCollection<ThriftGroup>("ThriftGroups");
            _membersCollection = context.GetCollection<ThriftGroupMember>("ThriftGroupMembers");
            _transactionsCollection = context.GetCollection<GroupTransaction>("GroupTransactions");
            _invitationsCollection = context.GetCollection<GroupInvitation>("GroupInvitations");
        }

        #region ThriftGroup CRUD Operations

        public async Task<List<ThriftGroup>> GetAllGroupsAsync()
        {
            return await _groupsCollection.Find(_ => true).ToListAsync();
        }

        public async Task<ThriftGroup?> GetGroupByIdAsync(string id)
        {
            return await _groupsCollection.Find(g => g.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<ThriftGroup>> GetGroupsByUserIdAsync(string userId)
        {
            var userMemberships = await _membersCollection
                .Find(m => m.UserId == userId && m.IsActive)
                .ToListAsync();

            var groupIds = userMemberships.Select(m => m.GroupId).ToList();

            return await _groupsCollection
                .Find(g => groupIds.Contains(g.Id) && g.IsActive)
                .ToListAsync();
        }

        public async Task<List<ThriftGroup>> GetPublicGroupsAsync(int skip = 0, int limit = 20)
        {
            return await _groupsCollection
                .Find(g => !g.IsPrivate && g.IsActive)
                .Skip(skip)
                .Limit(limit)
                .SortByDescending(g => g.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<ThriftGroup>> SearchGroupsAsync(string searchTerm, int skip = 0, int limit = 20)
        {
            var filter = Builders<ThriftGroup>.Filter.And(
                Builders<ThriftGroup>.Filter.Eq(g => g.IsActive, true),
                Builders<ThriftGroup>.Filter.Eq(g => g.IsPrivate, false),
                Builders<ThriftGroup>.Filter.Or(
                    Builders<ThriftGroup>.Filter.Regex(g => g.Name, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                    Builders<ThriftGroup>.Filter.Regex(g => g.Description, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
                )
            );

            return await _groupsCollection
                .Find(filter)
                .Skip(skip)
                .Limit(limit)
                .SortByDescending(g => g.CreatedAt)
                .ToListAsync();
        }

        public async Task<ThriftGroup?> GetGroupByCodeAsync(string groupCode)
        {
            return await _groupsCollection
                .Find(g => g.GroupCode == groupCode && g.IsActive)
                .FirstOrDefaultAsync();
        }

        public async Task<ThriftGroup> CreateGroupAsync(ThriftGroup group)
        {
            group.GroupCode = GenerateGroupCode();
            group.CreatedAt = DateTime.UtcNow;
            group.UpdatedAt = DateTime.UtcNow;

            await _groupsCollection.InsertOneAsync(group);
            return group;
        }

        public async Task<bool> UpdateGroupAsync(ThriftGroup group)
        {
            group.UpdatedAt = DateTime.UtcNow;

            var result = await _groupsCollection.ReplaceOneAsync(
                g => g.Id == group.Id,
                group
            );

            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteGroupAsync(string id)
        {
            var update = Builders<ThriftGroup>.Update
                .Set(g => g.IsActive, false)
                .Set(g => g.UpdatedAt, DateTime.UtcNow);

            var result = await _groupsCollection.UpdateOneAsync(
                g => g.Id == id,
                update
            );

            return result.ModifiedCount > 0;
        }

        #endregion

        #region ThriftGroupMember Operations

        public async Task<List<ThriftGroupMember>> GetGroupMembersAsync(string groupId)
        {
            return await _membersCollection
                .Find(m => m.GroupId == groupId && m.IsActive)
                .SortBy(m => m.JoinedAt)
                .ToListAsync();
        }

        public async Task<ThriftGroupMember?> GetMembershipAsync(string groupId, string userId)
        {
            return await _membersCollection
                .Find(m => m.GroupId == groupId && m.UserId == userId)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> IsMemberAsync(string groupId, string userId)
        {
            var member = await _membersCollection
                .Find(m => m.GroupId == groupId && m.UserId == userId && m.IsActive)
                .FirstOrDefaultAsync();

            return member != null;
        }

        public async Task<ThriftGroupMember> AddMemberAsync(ThriftGroupMember member)
        {
            member.JoinedAt = DateTime.UtcNow;
            await _membersCollection.InsertOneAsync(member);
            return member;
        }

        public async Task<bool> UpdateMemberAsync(ThriftGroupMember member)
        {
            var result = await _membersCollection.ReplaceOneAsync(
                m => m.Id == member.Id,
                member
            );

            return result.ModifiedCount > 0;
        }

        public async Task<bool> RemoveMemberAsync(string groupId, string userId)
        {
            var update = Builders<ThriftGroupMember>.Update.Set(m => m.IsActive, false);

            var result = await _membersCollection.UpdateOneAsync(
                m => m.GroupId == groupId && m.UserId == userId,
                update
            );

            return result.ModifiedCount > 0;
        }

        public async Task<int> GetMemberCountAsync(string groupId)
        {
            return (int)await _membersCollection
                .CountDocumentsAsync(m => m.GroupId == groupId && m.IsActive);
        }

        #endregion

        #region GroupTransaction Operations

        public async Task<List<GroupTransaction>> GetGroupTransactionsAsync(string groupId, int skip = 0, int limit = 50)
        {
            return await _transactionsCollection
                .Find(t => t.GroupId == groupId)
                .Skip(skip)
                .Limit(limit)
                .SortByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<List<GroupTransaction>> GetUserTransactionsAsync(string groupId, string userId)
        {
            return await _transactionsCollection
                .Find(t => t.GroupId == groupId && t.UserId == userId)
                .SortByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<GroupTransaction> CreateTransactionAsync(GroupTransaction transaction)
        {
            transaction.TransactionDate = DateTime.UtcNow;
            await _transactionsCollection.InsertOneAsync(transaction);

            // Update group current amount if it's a contribution
            if (transaction.TransactionType == GroupTransactionType.Contribution && 
                transaction.Status == TransactionStatus.Completed)
            {
                await UpdateGroupAmountAsync(transaction.GroupId, transaction.Amount);
                await UpdateMemberContributionAsync(transaction.GroupId, transaction.UserId, transaction.Amount);
            }

            return transaction;
        }

        public async Task<decimal> GetGroupTotalContributionsAsync(string groupId)
        {
            var result = await _transactionsCollection
                .Aggregate()
                .Match(t => t.GroupId == groupId && 
                           t.TransactionType == GroupTransactionType.Contribution && 
                           t.Status == TransactionStatus.Completed)
                .Group(new MongoDB.Bson.BsonDocument
                {
                    { "_id", "$GroupId" },
                    { "total", new MongoDB.Bson.BsonDocument("$sum", "$Amount") }
                })
                .FirstOrDefaultAsync();

            return result?["total"]?.AsDecimal ?? 0;
        }

        #endregion

        #region GroupInvitation Operations

        public async Task<List<GroupInvitation>> GetGroupInvitationsAsync(string groupId)
        {
            return await _invitationsCollection
                .Find(i => i.GroupId == groupId)
                .SortByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<GroupInvitation>> GetUserInvitationsAsync(string userId)
        {
            return await _invitationsCollection
                .Find(i => i.InvitedUserId == userId && i.Status == InvitationStatus.Pending)
                .SortByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task<GroupInvitation?> GetInvitationByCodeAsync(string invitationCode)
        {
            return await _invitationsCollection
                .Find(i => i.InvitationCode == invitationCode)
                .FirstOrDefaultAsync();
        }

        public async Task<GroupInvitation> CreateInvitationAsync(GroupInvitation invitation)
        {
            invitation.InvitationCode = GenerateInvitationCode();
            invitation.CreatedAt = DateTime.UtcNow;
            invitation.ExpiresAt = DateTime.UtcNow.AddDays(7);

            await _invitationsCollection.InsertOneAsync(invitation);
            return invitation;
        }

        public async Task<bool> UpdateInvitationStatusAsync(string invitationId, InvitationStatus status)
        {
            var update = Builders<GroupInvitation>.Update.Set(i => i.Status, status);

            var result = await _invitationsCollection.UpdateOneAsync(
                i => i.Id == invitationId,
                update
            );

            return result.ModifiedCount > 0;
        }

        #endregion

        #region Helper Methods

        private async Task UpdateGroupAmountAsync(string groupId, decimal amount)
        {
            var update = Builders<ThriftGroup>.Update
                .Inc(g => g.CurrentAmount, amount)
                .Set(g => g.UpdatedAt, DateTime.UtcNow);

            await _groupsCollection.UpdateOneAsync(g => g.Id == groupId, update);
        }

        private async Task UpdateMemberContributionAsync(string groupId, string userId, decimal amount)
        {
            var update = Builders<ThriftGroupMember>.Update
                .Inc(m => m.TotalContributions, amount)
                .Set(m => m.LastContributionDate, DateTime.UtcNow);

            await _membersCollection.UpdateOneAsync(
                m => m.GroupId == groupId && m.UserId == userId,
                update
            );
        }

        private string GenerateGroupCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private string GenerateInvitationCode()
        {
            return Guid.NewGuid().ToString("N")[..12].ToUpper();
        }

        #endregion
    }
}
