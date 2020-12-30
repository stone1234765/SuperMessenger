﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperMessenger.Data;
using SuperMessenger.Models;
using SuperMessenger.Models.EntityFramework;

namespace SuperMessenger.Controllers
{
    public class RoleController : Controller
    {
        private RoleManager<ApplicationRole> _roleManager;
        private UserManager<ApplicationUser> _userManager;
        private readonly SuperMessengerDbContext _context;
        public RoleController(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, SuperMessengerDbContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
        }

        //[Authorize(Policy = "ManageUsers")]
        public async Task<ViewResult> Index()
        {
            var roles = await GetRoles();
            return View(roles);
        }
        private async Task<List<RoleModel>> GetRoles()
        {
            List<RoleModel> roleModels = new List<RoleModel>();
            var roles = await _roleManager.Roles.ToListAsync();
            if (roles != null)
            {
                var users = await _userManager.Users.ToListAsync();
                if (users != null)
                {
                    foreach (var role in roles)
                    {
                        List<string> names = new List<string>();
                        foreach (var user in users)
                        {
                            if (await _userManager.IsInRoleAsync(user, role.Name))
                                names.Add(user.UserName);
                        }
                        roleModels.Add(new RoleModel(role, string.Join(", ", names)));
                    }
                }
            }
            return roleModels;
        }
        //public ViewResult Index() => View(_roleManager.Roles);
        private void Errors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
                ModelState.AddModelError("", error.Description);
        }

        //[Authorize(Policy = "ManageAllRoles")]
        public IActionResult Create() => View();

        [HttpPost]
        //[Authorize(Policy = "ManageAllRoles")]
        public async Task<IActionResult> Create([Required] string name)
        {
            if (ModelState.IsValid)
            {
                IdentityResult result = await _roleManager.CreateAsync(new ApplicationRole() { Name = name });
                if (result.Succeeded)
                    return RedirectToAction("Index");
                else
                    Errors(result);
            }
            return View(name);
        }
        [HttpPost]
        //[Authorize(Policy = "ManageAllRoles")]
        public async Task<IActionResult> Delete(string id)
        {
            ApplicationRole role = await _roleManager.FindByIdAsync(id);
            if (role != null)
            {
                IdentityResult result = await _roleManager.DeleteAsync(role);
                if (result.Succeeded)
                    return RedirectToAction("Index");
                else
                    Errors(result);
            }
            else
                ModelState.AddModelError("", "No role found");
            return View("Index", _roleManager.Roles);
        }
        //[Authorize(Policy = "ManageUsers")]
        public async Task<IActionResult> Update(string id)
        {
            ApplicationRole role = await _roleManager.FindByIdAsync(id);
            //if (CheckPermissionsToUpdate(role.Name))
            //{
            List<ApplicationUser> members = new List<ApplicationUser>();
            List<ApplicationUser> nonMembers = new List<ApplicationUser>();
            foreach (ApplicationUser user in await _userManager.Users.ToListAsync())
            {
                var list = await _userManager.IsInRoleAsync(user, role.Name) ? members : nonMembers;
                list.Add(user);
            }
            return View(new RoleEdit
            {
                Role = role,
                Members = members,
                NonMembers = nonMembers
            });
            //}
            //else
            //{
            //    return View("Index", await GetRoles());
            //}
        }

        //[Authorize(Policy = "ManageUsers")]
        public bool CheckPermissionsToUpdate(string roleName)
        {
            if (roleName == "User")
            {
                return true;
            }
            else if (roleName == "Manager")
            {
                if (User.Claims.Where(claim => claim.Type == "Permission" && claim.Value == "ManageManagers").Count() > 0)
                {
                    return true;
                }
                return false;
            }
            else
            {
                if (User.Claims.Where(claim => claim.Type == "Permission" && claim.Value == "ManageAllRoles").Count() > 0)
                {
                    return true;
                }
                return false;
            }
        }
        [HttpPost]
        //[Authorize(Policy = "ManageUsers")]
        public async Task<IActionResult> Update(RoleModification model)
        {
            //if (CheckPermissionsToUpdate(model.RoleName))
            //{
            IdentityResult result;
            if (ModelState.IsValid)
            {
                foreach (string userId in model.AddIds ?? new string[] { })
                {
                    ApplicationUser user = await _userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        result = await _userManager.AddToRoleAsync(user, model.RoleName);
                        if (!result.Succeeded)
                            Errors(result);
                    }
                }
                foreach (string userId in model.DeleteIds ?? new string[] { })
                {
                    ApplicationUser user = await _userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        result = await _userManager.RemoveFromRoleAsync(user, model.RoleName);
                        if (!result.Succeeded)
                            Errors(result);
                    }
                }
            }

            if (ModelState.IsValid)
                return RedirectToAction(nameof(Index));
            else
                return await Update(model.RoleId);
            //}
            //else
            //{
            //    return View("Index", await GetRoles());
            //}
        }
        //[Authorize(Policy = "ManageAllRoles")]
        public IActionResult CreateClaim() => View();
        [HttpPost]
        [ActionName("CreateClaim")]
        //[Authorize(Policy = "ManageAllRoles")]
        public async Task<IActionResult> CreateClaim(string claimType, string claimValue, string roleName)
        {
            ApplicationRole role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                return RedirectToAction("Index", await GetRoles());
            }
            Claim claim = new Claim(claimType, claimValue, ClaimValueTypes.String);
            IdentityResult result = await _roleManager.AddClaimAsync(role, claim);

            if (result.Succeeded)
                return RedirectToAction("Index", await GetRoles());
            else
                Errors(result);
            return View();
        }
        [HttpPost]
        //[Authorize(Policy = "ManageUsers")]
        public async Task<IActionResult> BanUser(BanModification model)
        {
            //if (CheckPermissionsToUpdate(model.RoleName))
            //{
            if (ModelState.IsValid)
            {
                var users = await _context.Users
                    .Where(user => (model.BanIds == null ? false : model.BanIds.Contains(user.Id))
                    || (model.UnbanIds == null ? false : model.UnbanIds.Contains(user.Id)))
                    .ToListAsync() ?? new List<ApplicationUser>();
                var usersToBan = users
                    .Where(user => model.BanIds == null ? false : model.BanIds.Contains(user.Id))
                    .ToList() ?? new List<ApplicationUser>();
                var usersFromBan = users
                    .Where(user => model.UnbanIds == null ? false : model.UnbanIds.Contains(user.Id))
                    .ToList() ?? new List<ApplicationUser>();
                foreach (var user in usersToBan)
                {
                    user.IsInBan = true;
                }
                foreach (var user in usersFromBan)
                {
                    user.IsInBan = false;
                }
                await _context.SaveChangesAsync();
            }

            if (ModelState.IsValid)
                return RedirectToAction(nameof(Index));
            else
                return await BanUser();
            //}
            //else
            //{
            //    return View("Index", await GetRoles());
            //}
        }
        public async Task<IActionResult> BanUser(string emailPart = "")
        {
            var userBanModel = new UserBanModel();
            var bannedUsers = await _context.Users.Where(user => user.IsInBan && user.Email.Contains(emailPart)).Take(10).ToListAsync();
            var noBannedUsers = await _context.Users.Where(user => !user.IsInBan && user.Email.Contains(emailPart)).Take(10).ToListAsync();
            return View(new UserBanModel()
            {
                BannedUsers = bannedUsers,
                NoBannedUsers = noBannedUsers,
            });
        }
    }
}
