using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Thrift.Models;
using Thrift.Repositories;
using Thrift.ViewModels;

namespace Thrift.Controllers
{
    [Authorize]
    public class ThriftGroupController : Controller
    {
        private readonly ThriftGroupRepository _thriftGroupRepository;
        private readonly UserManager<User> _userManager;

        public ThriftGroupController(
            ThriftGroupRepository thriftGroupRepository,
            UserManager<User> userManager)
        {
            _thriftGroupRepository = thriftGroupRepository;
            _userManager = userManager;
        }

        // GET: ThriftGroup
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
                
                var userGroups = await _thriftGroupRepository.GetGroupsByUserIdAsync(userId);
                var publicGroups = await _thriftGroupRepository.GetPublicGroupsAsync(0, 10);
                
                var viewModel = new ThriftGroupIndexViewModel
                {
                    UserGroups = userGroups,
                    PublicGroups = publicGroups.Where(g => !userGroups.Any(ug => ug.Id == g.Id)).ToList(),
                    UserInvitations = await _thriftGroupRepository.GetUserInvitationsAsync(userId)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return View(new ThriftGroupIndexViewModel());
            }
        }

        // GET: ThriftGroup/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var group = await _thriftGroupRepository.GetGroupByIdAsync(id);
            if (group == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var isMember = await _thriftGroupRepository.IsMemberAsync(id, userId);

            if (group.IsPrivate && !isMember)
            {
                TempData["ErrorMessage"] = "This is a private group.";
                return RedirectToAction(nameof(Index));
            }

            var members = await _thriftGroupRepository.GetGroupMembersAsync(id);
            var transactions = await _thriftGroupRepository.GetGroupTransactionsAsync(id, 0, 20);

            // Get user details for members and transactions
            foreach (var member in members)
            {
                var user = await _userManager.FindByIdAsync(member.UserId);
                member.UserName = user?.UserName ?? "Unknown User";
                member.UserEmail = user?.Email ?? "";
            }

            foreach (var transaction in transactions)
            {
                var user = await _userManager.FindByIdAsync(transaction.UserId);
                transaction.UserName = user?.UserName ?? "Unknown User";
            }

            var viewModel = new ThriftGroupDetailsViewModel
            {
                Group = group,
                Members = members,
                Transactions = transactions,
                IsMember = isMember,
                UserMembership = isMember ? await _thriftGroupRepository.GetMembershipAsync(id, userId) : null
            };

            return View(viewModel);
        }        // GET: ThriftGroup/Create
        public IActionResult Create()
        {
            return View(new CreateThriftGroupViewModel());
        }

        // POST: ThriftGroup/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateThriftGroupViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
                      // Map view model to domain model
                    var group = new ThriftGroup
                    {
                        Name = viewModel.Name,
                        Description = viewModel.Description,
                        GroupType = viewModel.GroupType,
                        MaxMembers = viewModel.MaxMembers,
                        TargetAmount = viewModel.TargetAmount,
                        StartDate = viewModel.StartDate,
                        EndDate = viewModel.EndDate,
                        ContributionFrequency = viewModel.ContributionFrequency,
                        ContributionAmount = viewModel.ContributionAmount,
                        MinimumContribution = viewModel.MinContribution,
                        MaximumContribution = viewModel.MaxContribution,
                        RequireApproval = viewModel.RequireApproval,
                        IsPrivate = !viewModel.IsPublic, // Note: IsPublic is the inverse of IsPrivate
                        Rules = viewModel.Rules,
                        GroupImage = viewModel.GroupImage,
                        CreatedBy = userId
                    };

                    var createdGroup = await _thriftGroupRepository.CreateGroupAsync(group);

                    // Add creator as admin member
                    var creatorMembership = new ThriftGroupMember
                    {
                        GroupId = createdGroup.Id,
                        UserId = userId,
                        Role = GroupRole.Creator
                    };

                    await _thriftGroupRepository.AddMemberAsync(creatorMembership);

                    TempData["SuccessMessage"] = "Thrift group created successfully!";
                    return RedirectToAction(nameof(Details), new { id = createdGroup.Id });
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return View(viewModel);
            }
        }        // GET: ThriftGroup/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var group = await _thriftGroupRepository.GetGroupByIdAsync(id);
            if (group == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var membership = await _thriftGroupRepository.GetMembershipAsync(id, userId);

            if (membership?.Role != GroupRole.Creator && membership?.Role != GroupRole.Admin)
            {
                TempData["ErrorMessage"] = "You don't have permission to edit this group.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Map ThriftGroup to CreateThriftGroupViewModel
            var viewModel = new CreateThriftGroupViewModel
            {
                Name = group.Name,
                Description = group.Description,
                GroupType = group.GroupType,
                MaxMembers = group.MaxMembers,
                TargetAmount = group.TargetAmount,
                StartDate = group.StartDate,
                EndDate = group.EndDate,
                ContributionFrequency = group.ContributionFrequency,
                ContributionAmount = group.ContributionAmount,
                MinContribution = group.MinimumContribution,
                MaxContribution = group.MaximumContribution,
                IsPublic = !group.IsPrivate, // Note: IsPublic is the inverse of IsPrivate
                RequireApproval = group.RequireApproval,
                Rules = group.Rules,
                GroupImage = group.GroupImage
            };

            return View(viewModel);
        }

        // POST: ThriftGroup/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, CreateThriftGroupViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
                    var membership = await _thriftGroupRepository.GetMembershipAsync(id, userId);

                    if (membership?.Role != GroupRole.Creator && membership?.Role != GroupRole.Admin)
                    {
                        TempData["ErrorMessage"] = "You don't have permission to edit this group.";
                        return RedirectToAction(nameof(Details), new { id });
                    }

                    // Get the existing group to preserve certain fields
                    var existingGroup = await _thriftGroupRepository.GetGroupByIdAsync(id);
                    if (existingGroup == null)
                    {
                        return NotFound();
                    }

                    // Map view model to domain model, preserving key fields
                    existingGroup.Name = viewModel.Name;
                    existingGroup.Description = viewModel.Description;
                    existingGroup.GroupType = viewModel.GroupType;
                    existingGroup.MaxMembers = viewModel.MaxMembers;
                    existingGroup.TargetAmount = viewModel.TargetAmount;
                    existingGroup.StartDate = viewModel.StartDate;
                    existingGroup.EndDate = viewModel.EndDate;
                    existingGroup.ContributionFrequency = viewModel.ContributionFrequency;
                    existingGroup.ContributionAmount = viewModel.ContributionAmount;
                    existingGroup.MinimumContribution = viewModel.MinContribution;
                    existingGroup.MaximumContribution = viewModel.MaxContribution;
                    existingGroup.IsPrivate = !viewModel.IsPublic; // Note: IsPublic is the inverse of IsPrivate
                    existingGroup.RequireApproval = viewModel.RequireApproval;
                    existingGroup.Rules = viewModel.Rules;
                    existingGroup.GroupImage = viewModel.GroupImage;

                    await _thriftGroupRepository.UpdateGroupAsync(existingGroup);
                    TempData["SuccessMessage"] = "Group updated successfully!";
                    return RedirectToAction(nameof(Details), new { id });
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return View(viewModel);
            }
        }

        // POST: ThriftGroup/Join
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(string groupId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
                var group = await _thriftGroupRepository.GetGroupByIdAsync(groupId);

                if (group == null)
                {
                    TempData["ErrorMessage"] = "Group not found.";
                    return RedirectToAction(nameof(Index));
                }

                if (await _thriftGroupRepository.IsMemberAsync(groupId, userId))
                {
                    TempData["InfoMessage"] = "You are already a member of this group.";
                    return RedirectToAction(nameof(Details), new { id = groupId });
                }

                var memberCount = await _thriftGroupRepository.GetMemberCountAsync(groupId);
                if (memberCount >= group.MaxMembers)
                {
                    TempData["ErrorMessage"] = "This group has reached its maximum number of members.";
                    return RedirectToAction(nameof(Details), new { id = groupId });
                }

                var newMember = new ThriftGroupMember
                {
                    GroupId = groupId,
                    UserId = userId,
                    Role = GroupRole.Member
                };

                await _thriftGroupRepository.AddMemberAsync(newMember);
                TempData["SuccessMessage"] = "Successfully joined the group!";

                return RedirectToAction(nameof(Details), new { id = groupId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: ThriftGroup/Leave
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Leave(string groupId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
                
                await _thriftGroupRepository.RemoveMemberAsync(groupId, userId);
                TempData["SuccessMessage"] = "Successfully left the group.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id = groupId });
            }
        }

        // GET: ThriftGroup/Search
        public async Task<IActionResult> Search(string searchTerm, int page = 1)
        {
            const int pageSize = 12;
            var skip = (page - 1) * pageSize;

            List<ThriftGroup> groups;
            
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                groups = await _thriftGroupRepository.GetPublicGroupsAsync(skip, pageSize);
            }
            else
            {
                groups = await _thriftGroupRepository.SearchGroupsAsync(searchTerm, skip, pageSize);
            }

            var viewModel = new ThriftGroupSearchViewModel
            {
                SearchTerm = searchTerm,
                Groups = groups,
                CurrentPage = page,
                PageSize = pageSize
            };

            return View(viewModel);
        }

        // GET: ThriftGroup/JoinByCode
        public IActionResult JoinByCode()
        {
            return View();
        }

        // POST: ThriftGroup/JoinByCode
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> JoinByCode(string groupCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(groupCode))
                {
                    TempData["ErrorMessage"] = "Please enter a valid group code.";
                    return View();
                }

                var group = await _thriftGroupRepository.GetGroupByCodeAsync(groupCode.ToUpper());
                if (group == null)
                {
                    TempData["ErrorMessage"] = "Invalid group code.";
                    return View();
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
                
                if (await _thriftGroupRepository.IsMemberAsync(group.Id, userId))
                {
                    TempData["InfoMessage"] = "You are already a member of this group.";
                    return RedirectToAction(nameof(Details), new { id = group.Id });
                }

                var memberCount = await _thriftGroupRepository.GetMemberCountAsync(group.Id);
                if (memberCount >= group.MaxMembers)
                {
                    TempData["ErrorMessage"] = "This group has reached its maximum number of members.";
                    return View();
                }

                var newMember = new ThriftGroupMember
                {
                    GroupId = group.Id,
                    UserId = userId,
                    Role = GroupRole.Member
                };

                await _thriftGroupRepository.AddMemberAsync(newMember);
                TempData["SuccessMessage"] = $"Successfully joined '{group.Name}'!";

                return RedirectToAction(nameof(Details), new { id = group.Id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return View();
            }
        }

        // POST: ThriftGroup/Contribute
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contribute(string groupId, decimal amount, string description)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

                if (!await _thriftGroupRepository.IsMemberAsync(groupId, userId))
                {
                    TempData["ErrorMessage"] = "You must be a member to contribute to this group.";
                    return RedirectToAction(nameof(Details), new { id = groupId });
                }

                if (amount <= 0)
                {
                    TempData["ErrorMessage"] = "Contribution amount must be greater than zero.";
                    return RedirectToAction(nameof(Details), new { id = groupId });
                }

                var transaction = new GroupTransaction
                {
                    GroupId = groupId,
                    UserId = userId,
                    Amount = amount,
                    TransactionType = GroupTransactionType.Contribution,
                    Description = description ?? "Group contribution",
                    Status = TransactionStatus.Completed
                };

                await _thriftGroupRepository.CreateTransactionAsync(transaction);
                TempData["SuccessMessage"] = $"Successfully contributed {amount:C} to the group!";

                return RedirectToAction(nameof(Details), new { id = groupId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id = groupId });
            }
        }
    }
}
