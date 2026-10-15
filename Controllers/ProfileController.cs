using EITSTMS.Data;
using EITSTMS.Models.Entities;
using EITSTMS.Models.ViewModels.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EITSTMS.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 获取当前登录用户
            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            // 查询当前用户角色
            IList<string> roles = await _userManager.GetRolesAsync(user);

            // 查询用户所属部门
            string departmentName = string.Empty;

            if (user.DepartmentId.HasValue)
            {
                departmentName =
                    await _context.Departments
                        .Where(d => d.DepartmentId == user.DepartmentId.Value)
                        .Select(d => d.DepartmentName)
                        .FirstOrDefaultAsync()
                    ?? string.Empty;
            }

            // 将用户数据转换为个人中心 ViewModel
            ProfileViewModel model = new ProfileViewModel
            {
                UserName = user.UserName ?? string.Empty,
                EmployeeNumber = user.EmployeeNumber,
                Name = user.Name,
                DepartmentName = departmentName,
                Role = roles.FirstOrDefault() ?? string.Empty,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };

            return View(model);
        }

        #region 修改密码
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            // 验证页面提交的数据
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 获取当前登录用户
            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            // 使用当前密码验证身份，并修改成新密码
            IdentityResult result =
                await _userManager.ChangePasswordAsync(
                    user,
                    model.CurrentPassword,
                    model.NewPassword);

            // 修改密码失败
            if (!result.Succeeded)
            {
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty,error.Description);
                }

                return View(model);
            }

            // 修改成功后返回个人中心
            return RedirectToAction("Index");
        }
        #endregion
    }
}