using EITSTMS.Models.Entities;
using EITSTMS.Models.ViewModels.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EITSTMS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        // Identity 用户管理器，用于查询、创建、修改用户以及管理用户角色
        private readonly UserManager<ApplicationUser> _userManager;

        // Identity 角色管理器，用于查询和判断角色是否存在
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // 用户管理首页，显示系统中的用户列表
        public async Task<IActionResult> Index()
        {
            // 获取当前登录的 Admin 用户
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
            if(currentUser == null)
            {
                return Challenge();
            }

            // 查询 AspNetUsers 中所有用户，排除当前登录的 Admin 用户
            List<ApplicationUser> users = 
                await _userManager.Users
                    .Where(u => u.Id != currentUser.Id)
                    .ToListAsync();

            // 创建用于页面显示的 ViewModel 集合
            List<UserListItemViewModel> model = new List<UserListItemViewModel>();

            foreach (ApplicationUser user in users)
            {
                // 查询当前被编辑用户拥有的角色
                IList<string> roles = await _userManager.GetRolesAsync(user);
                // 判断当前被编辑用户是否拥有 Admin 角色
                bool isAdmin = roles.Contains("Admin");

                // System Admin 不能被编辑；普通 Admin 只能编辑非 Admin 用户，同时只有 Admin 身份才能进入到这个控制类
                bool canEdit = !user.IsSystemAdmin && (currentUser.IsSystemAdmin || !isAdmin);

                // 将 ApplicationUser 转换为列表页面需要的 ViewModel
                model.Add(new UserListItemViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    EmployeeNumber = user.EmployeeNumber,
                    Name = user.Name,
                    IsActive = user.IsActive,
                    Roles = roles,
                    CanEdit = canEdit,
                });
            }

            // 将用户列表传给 Views/Users/Index.cshtml
            return View(model);
        }

        #region 新建用户
        // 显示新建用户页面
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // 获取当前登录的 Admin 用户
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);

            // 如果当前登录用户不存在，则要求重新登录
            if (currentUser == null)
            {
                return Challenge();
            }

            // 保存当前管理员可以选择的角色
            List<string> availableRoles;
            // System Admin 可以创建 Employee、Technician、Admin
            if (currentUser.IsSystemAdmin)
            {
                availableRoles =
                    await _roleManager.Roles
                        .Select(r => r.Name!)
                        .ToListAsync();
            }
            else
            {
                // 普通 Admin 不能创建 Admin，因此角色下拉框只显示 Employee 和 Technician
                availableRoles =
                    await _roleManager.Roles
                        .Where(r => r.Name != "Admin")
                        .Select(r => r.Name!)
                        .ToListAsync();
            }

            // 创建页面需要的 ViewModel
            CreateUserViewModel model =
                new CreateUserViewModel
                {
                    // 将可选择的角色传给页面下拉框
                    AvailableRoles = availableRoles
                };

            return View(model);
        }

        // 接收新建用户页面提交的数据
        [HttpPost]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            // 获取当前登录的 Admin 用户
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
            // 如果当前登录用户不存在，则要求重新登录
            if (currentUser == null)
            {
                return Challenge();
            }

            // 检查 ViewModel 中的验证规则是否通过
            if (!ModelState.IsValid)
            {
                // System Admin 可以选择全部角色
                if (currentUser.IsSystemAdmin)
                {
                    model.AvailableRoles =
                        await _roleManager.Roles
                            .Select(r => r.Name!)
                            .ToListAsync();
                }
                else
                {
                    // 普通 Admin 只能选择 Employee 和 Technician
                    model.AvailableRoles =
                        await _roleManager.Roles
                            .Where(r => r.Name != "Admin")
                            .Select(r => r.Name!)
                            .ToListAsync();
                }

                // 验证失败，重新显示页面和错误信息
                return View(model);
            }

            // 检查用户名是否已存在
            ApplicationUser? existingUser = await _userManager.FindByNameAsync(model.UserName);
            if (existingUser != null)
            {
                ModelState.AddModelError(
                    "UserName",
                    "该用户名已存在，请使用其他用户名。");

                if (currentUser.IsSystemAdmin)
                {
                    model.AvailableRoles =
                        await _roleManager.Roles
                            .Select(r => r.Name!)
                            .ToListAsync();
                }
                else
                {
                    model.AvailableRoles =
                        await _roleManager.Roles
                            .Where(r => r.Name != "Admin")
                            .Select(r => r.Name!)
                            .ToListAsync();
                }

                return View(model);
            }

            // 检查员工编号是否已存在
            bool employeeNumberExists =
                await _userManager.Users
                    .AnyAsync(u => u.EmployeeNumber == model.EmployeeNumber);

            if (employeeNumberExists)
            {
                ModelState.AddModelError("EmployeeNumber","该员工编号已存在，请使用其他员工编号。");

                if (currentUser.IsSystemAdmin)
                {
                    model.AvailableRoles =
                        await _roleManager.Roles
                            .Select(r => r.Name!)
                            .ToListAsync();
                }
                else
                {
                    model.AvailableRoles =
                        await _roleManager.Roles
                            .Where(r => r.Name != "Admin")
                            .Select(r => r.Name!)
                            .ToListAsync();
                }

                return View(model);
            }

            // 检查用户提交的角色是否真实存在
            bool roleExists = await _roleManager.RoleExistsAsync(model.Role);
            if (!roleExists)
            {
                // 将错误绑定到 Role 字段
                ModelState.AddModelError("Role","请选择有效的角色。");
                // 验证失败后要重新加载角色列表
                if (currentUser.IsSystemAdmin)
                {
                    model.AvailableRoles =
                        await _roleManager.Roles
                            .Select(r => r.Name!)
                            .ToListAsync();
                }
                else
                {
                    model.AvailableRoles =
                        await _roleManager.Roles
                            .Where(r => r.Name != "Admin")
                            .Select(r => r.Name!)
                            .ToListAsync();
                }

                return View(model);
            }

            // 普通 Admin 即使伪造请求提交 Admin 角色，也禁止创建
            if (model.Role == "Admin" && !currentUser.IsSystemAdmin)
            {
                return Forbid();
            }

            // 创建新的 ApplicationUser 对象
            ApplicationUser user = new ApplicationUser
            {
                UserName = model.UserName,
                EmployeeNumber = model.EmployeeNumber,
                Name = model.Name,
                IsActive = true,
                IsSystemAdmin = false
            };

            // 使用 Identity 创建用户，并自动处理密码哈希
            IdentityResult result = await _userManager.CreateAsync(user,model.Password);
            if (!result.Succeeded)
            {
                // 将 Identity 返回的错误加入 ModelState
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty,error.Description);
                }

                // 重新加载角色列表
                if (currentUser.IsSystemAdmin)
                {
                    model.AvailableRoles =
                        await _roleManager.Roles
                            .Select(r => r.Name!)
                            .ToListAsync();
                }
                else
                {
                    model.AvailableRoles =
                        await _roleManager.Roles
                            .Where(r => r.Name != "Admin")
                            .Select(r => r.Name!)
                            .ToListAsync();
                }

                return View(model);
            }

            // 将新创建的用户加入页面选择的角色
            IdentityResult roleResult = await _userManager.AddToRoleAsync(user,model.Role);
            if (!roleResult.Succeeded)
            {
                foreach (IdentityError error in roleResult.Errors)
                {
                    ModelState.AddModelError(string.Empty,error.Description);
                }

                // 重新加载角色列表
                if (currentUser.IsSystemAdmin)
                {
                    model.AvailableRoles =
                        await _roleManager.Roles
                            .Select(r => r.Name!)
                            .ToListAsync();
                }
                else
                {
                    model.AvailableRoles =
                        await _roleManager.Roles
                            .Where(r => r.Name != "Admin")
                            .Select(r => r.Name!)
                            .ToListAsync();
                }

                return View(model);
            }

            // 创建成功后返回用户列表
            return RedirectToAction("Index");
        }

        #endregion

        #region 编辑

        // 显示用户编辑页面
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            // 获取当前登录的 Admin 用户
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
            // 如果当前登录用户不存在，则要求重新登录
            if (currentUser == null)
            {
                return Challenge();
            }

            // 根据 Id 查询需要编辑的用户
            ApplicationUser? user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // 获取被编辑用户当前拥有的角色
            IList<string> roles = await _userManager.GetRolesAsync(user);
            // 普通 Admin 禁止编辑其他 Admin，System Admin 不受这个限制
            if (roles.Contains("Admin") && !currentUser.IsSystemAdmin)
            {
                return Forbid();
            }

            // 保存编辑页面中允许选择的角色
            List<string> availableRoles;
            // System Admin 可以选择全部角色
            if (currentUser.IsSystemAdmin)
            {
                availableRoles =
                    await _roleManager.Roles
                        .Select(r => r.Name!)
                        .ToListAsync();
            }
            else
            {
                // 普通 Admin 只能让用户在 Employee 和 Technician 之间转换
                availableRoles =
                    await _roleManager.Roles
                        .Where(r => r.Name != "Admin")
                        .Select(r => r.Name!)
                        .ToListAsync();
            }

            // 将数据库中的用户数据转换为 EditUserViewModel
            EditUserViewModel model = new EditUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                EmployeeNumber = user.EmployeeNumber,
                Name = user.Name,

                // 当前项目规定一个普通账号只保留一个业务角色
                Role = roles.FirstOrDefault() ?? string.Empty,

                IsActive = user.IsActive,
                AvailableRoles = availableRoles
            };

            // 返回编辑页面
            return View(model);
        }

        // 接收编辑页面提交的数据
        [HttpPost]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            // 获取当前登录的 Admin 用户
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
            // 如果当前登录用户不存在，则要求重新登录
            if (currentUser == null)
            {
                return Challenge();
            }

            // 验证页面提交的数据
            if (!ModelState.IsValid)
            {
                // System Admin 可以选择全部角色
                if (currentUser.IsSystemAdmin)
                {
                    model.AvailableRoles =
                        await _roleManager.Roles
                            .Select(r => r.Name!)
                            .ToListAsync();
                }
                else
                {
                    // 普通 Admin 只能选择 Employee 和 Technician
                    model.AvailableRoles =
                        await _roleManager.Roles
                            .Where(r => r.Name != "Admin")
                            .Select(r => r.Name!)
                            .ToListAsync();
                }

                return View(model);
            }

            // 根据页面隐藏字段中的 Id 查询被编辑用户
            ApplicationUser? user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return NotFound();
            }

            // 获取被编辑用户当前的角色
            IList<string> currentRoles = await _userManager.GetRolesAsync(user);
            // 普通 Admin 禁止编辑任何 Admin 账号
            if (currentRoles.Contains("Admin") && !currentUser.IsSystemAdmin)
            {
                return Forbid();
            }

            // 检查页面提交的新角色是否真实存在
            bool roleExists = await _roleManager.RoleExistsAsync(model.Role);
            if (!roleExists)
            {
                // 将错误绑定到 Role 字段
                ModelState.AddModelError("Role","请选择有效的角色。");

                // 重新加载角色列表
                if (currentUser.IsSystemAdmin)
                {
                    model.AvailableRoles =
                        await _roleManager.Roles
                            .Select(r => r.Name!)
                            .ToListAsync();
                }
                else
                {
                    model.AvailableRoles =
                        await _roleManager.Roles
                            .Where(r => r.Name != "Admin")
                            .Select(r => r.Name!)
                            .ToListAsync();
                }

                return View(model);
            }

            // 普通 Admin 禁止把 Employee 或 Technician 提升成 Admin
            // 只有 System Admin 才能执行这种角色提升
            if (model.Role == "Admin" && !currentUser.IsSystemAdmin)
            {
                return Forbid();
            }

            // System Admin 账号不能被禁用
            // 防止系统失去最高权限账号
            if (user.IsSystemAdmin && !model.IsActive)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "系统初始管理员不能被禁用。");

                // System Admin 编辑页面可以显示全部角色
                model.AvailableRoles =
                    await _roleManager.Roles
                        .Select(r => r.Name!)
                        .ToListAsync();

                return View(model);
            }

            // System Admin 账号必须始终保持 Admin 角色
            // 防止把唯一最高权限账号降级成 Employee 或 Technician
            if (user.IsSystemAdmin && model.Role != "Admin")
            {
                ModelState.AddModelError(
                    string.Empty,
                    "系统初始管理员不能修改为其他角色。");

                model.AvailableRoles =
                    await _roleManager.Roles
                        .Select(r => r.Name!)
                        .ToListAsync();

                return View(model);
            }

            // 将页面修改后的数据赋值给 ApplicationUser
            user.EmployeeNumber = model.EmployeeNumber;
            user.Name = model.Name;
            user.IsActive = model.IsActive;

            // 保存用户基本信息到 AspNetUsers
            IdentityResult updateResult = await _userManager.UpdateAsync(user);

            // 用户基本信息更新失败
            if (!updateResult.Succeeded)
            {
                // 将 Identity 返回的错误加入 ModelState
                foreach (IdentityError error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty,error.Description);
                }

                // 重新加载角色列表
                if (currentUser.IsSystemAdmin)
                {
                    model.AvailableRoles =
                        await _roleManager.Roles
                            .Select(r => r.Name!)
                            .ToListAsync();
                }
                else
                {
                    model.AvailableRoles =
                        await _roleManager.Roles
                            .Where(r => r.Name != "Admin")
                            .Select(r => r.Name!)
                            .ToListAsync();
                }

                return View(model);
            }

            // 如果当前角色和页面选择的新角色不同，才修改角色
            if (!currentRoles.Contains(model.Role))
            {
                // 先删除用户原来的角色关系
                IdentityResult removeRoleResult = await _userManager.RemoveFromRolesAsync(user,currentRoles);
                if (!removeRoleResult.Succeeded)
                {
                    foreach (IdentityError error in removeRoleResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty,error.Description);
                    }

                    // 重新加载角色列表
                    if (currentUser.IsSystemAdmin)
                    {
                        model.AvailableRoles =
                            await _roleManager.Roles
                                .Select(r => r.Name!)
                                .ToListAsync();
                    }
                    else
                    {
                        model.AvailableRoles =
                            await _roleManager.Roles
                                .Where(r => r.Name != "Admin")
                                .Select(r => r.Name!)
                                .ToListAsync();
                    }

                    return View(model);
                }

                // 将用户加入新的角色
                IdentityResult addRoleResult = await _userManager.AddToRoleAsync(user,model.Role);
                if (!addRoleResult.Succeeded)
                {
                    foreach (IdentityError error in addRoleResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty,error.Description);
                    }

                    // 重新加载角色列表
                    if (currentUser.IsSystemAdmin)
                    {
                        model.AvailableRoles =
                            await _roleManager.Roles
                                .Select(r => r.Name!)
                                .ToListAsync();
                    }
                    else
                    {
                        model.AvailableRoles =
                            await _roleManager.Roles
                                .Where(r => r.Name != "Admin")
                                .Select(r => r.Name!)
                                .ToListAsync();
                    }

                    return View(model);
                }
            }

            // 编辑成功后返回用户列表页面
            return RedirectToAction("Index");
        }
        #endregion

        #region 启用或禁用用户

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            // 获取当前登录的 Admin 用户
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            // 根据 Id 查找目标用户
            ApplicationUser? user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // System Admin 不能被禁用或启用操作
            if (user.IsSystemAdmin)
            {
                return Forbid();
            }

            // 获取目标用户角色
            IList<string> roles = await _userManager.GetRolesAsync(user);

            // 普通 Admin 不能操作其他 Admin
            if (roles.Contains("Admin") && !currentUser.IsSystemAdmin)
            {
                return Forbid();
            }

            // 切换账号状态：启用变禁用，禁用变启用
            user.IsActive = !user.IsActive;

            // 保存账号状态
            IdentityResult result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }
        #endregion
    }
}