using EITSTMS.Models.Entities;
using EITSTMS.Models.ViewModels.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EITSTMS.Controllers
{
    public class AccountController : Controller
    {
        // 负责登录，负责登出，负责创建认证 Cookie，负责检查登录相关状态
        private readonly SignInManager<ApplicationUser> _signInManager;
        // 负责用户管理，负责创建用户，负责查找用户，负责修改用户密码，负责分配角色，负责Claims
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        #region 登录
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // 没有通过模型验证，说明用户输入不完整或格式不正确
            // 当前规则有[Required]
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty,"用户名或密码错误。");

                return View(model);
            }

            // 检查用户是否被禁用
            if (!user.IsActive)
            {
                ModelState.AddModelError(string.Empty,"该账号已被禁用。");

                return View(model);
            }

            // 检查密码是否正确，并且在失败时锁定用户
            var result = await _signInManager.CheckPasswordSignInAsync(
                user,
                model.Password,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                // 登录成功的话，Identity 会调用自定义 Claims 工厂生成 Claim
                await _signInManager.SignInAsync(user,model.RememberMe);

                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty,"该账号已被锁定，请稍后再试。");

                return View(model);
            }

            ModelState.AddModelError(string.Empty,"用户名或密码错误。");

            return View(model);
        }
        #endregion

        #region 退出
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }
        #endregion

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}