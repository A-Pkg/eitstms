using System.Data;
using EITSTMS.Data;
using EITSTMS.Models.Entities;
using EITSTMS.Models.Enums;
using EITSTMS.Models.ViewModels.Tickets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EITSTMS.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TicketsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// 显示工单列表，根据用户角色显示不同的内容
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Employee,Technician,Admin")]
        public async Task<IActionResult> Index()
        {
            // 获取当前登录用户
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }


            // Admin 优先判断
            // Admin 可以查看全部工单
            if (User.IsInRole("Admin"))
            {
                ViewData["PageTitle"] = "工单管理";

                List<Ticket> tickets =
                    await _context.Tickets
                        .OrderByDescending(t => t.CreatedAt)
                        .ToListAsync();

                List<TicketCategory> categories =
                    await _context.TicketCategories
                        .OrderBy(c => c.CategoryName)
                        .ToListAsync();

                ViewBag.Categories = categories;

                return View(tickets);
            }


            // Technician 查看分配给自己的工单
            else if (User.IsInRole("Technician"))
            {
                ViewData["PageTitle"] = "我的任务";

                List<Ticket> tickets =
                    await _context.Tickets
                        .Where(t =>
                            t.AssignedToId == currentUser.Id)
                        .OrderByDescending(t => t.CreatedAt)
                        .ToListAsync();

                return View(tickets);
            }


            // Employee 查看自己创建的工单
            else if (User.IsInRole("Employee"))
            {
                ViewData["PageTitle"] = "我的工单";

                List<Ticket> tickets =
                    await _context.Tickets
                        .Where(t =>
                            t.CreatedById == currentUser.Id)
                        .OrderByDescending(t => t.CreatedAt)
                        .ToListAsync();

                return View(tickets);
            }


            return Forbid();
        }

        /// <summary>
        /// 添加新的工单分类
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                TempData["CategoryError"] = "分类名称不能为空。";

                return RedirectToAction("Index");
            }

            bool categoryExists =
                await _context.TicketCategories
                    .AnyAsync(c => c.CategoryName == categoryName);

            if (categoryExists)
            {
                TempData["CategoryError"] = "该工单分类已存在。";

                return RedirectToAction("Index");
            }

            TicketCategory category = new TicketCategory
            {
                CategoryName = categoryName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.TicketCategories.Add(category);

            await _context.SaveChangesAsync();

            TempData["CategorySuccess"] = "工单分类添加成功。";

            return RedirectToAction("Index");
        }

        /// <summary>
        /// 删除工单分类
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            TicketCategory? category =
                await _context.TicketCategories
                    .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                TempData["CategoryError"] = "工单分类不存在。";

                return RedirectToAction("Index");
            }

            bool isUsed =
                await _context.Tickets
                    .AnyAsync(t => t.CategoryId == id);

            if (isUsed)
            {
                TempData["CategoryError"] =
                    "该工单分类已经被工单使用，不能删除。";

                return RedirectToAction("Index");
            }

            _context.TicketCategories.Remove(category);

            await _context.SaveChangesAsync();

            TempData["CategorySuccess"] = "工单分类删除成功。";

            return RedirectToAction("Index");
        }

        /// <summary>
        /// 显示创建工单的表单
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> Create()
        {
            CreateTicketViewModel model = new CreateTicketViewModel
            {
                Categories =
                    await _context.TicketCategories
                        .Where(c => c.IsActive)
                        .OrderBy(c => c.CategoryName)
                        .Select(c => new SelectListItem
                        {
                            Value = c.CategoryId.ToString(),
                            Text = c.CategoryName
                        })
                        .ToListAsync(),

                Assets =
                    await _context.Assets
                        .Where(a => a.Status != AssetStatus.Retired)
                        .OrderBy(a => a.AssetNumber)
                        .Select(a => new SelectListItem
                        {
                            Value = a.AssetId.ToString(),
                            Text = a.AssetNumber + " - " + a.AssetName
                        })
                        .ToListAsync()
            };

            return View(model);
        }

        /// <summary>
        /// 处理创建工单的表单提交
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Employee")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTicketViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCreateLists(model);

                return View(model);
            }

            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            bool categoryExists =
                await _context.TicketCategories
                    .AnyAsync(c =>
                        c.CategoryId == model.CategoryId &&
                        c.IsActive);

            if (!categoryExists)
            {
                ModelState.AddModelError(
                    "CategoryId",
                    "请选择有效的工单分类。");

                await LoadCreateLists(model);

                return View(model);
            }

            if (model.RelatedAssetId.HasValue)
            {
                bool assetExists =
                    await _context.Assets
                        .AnyAsync(a =>
                            a.AssetId == model.RelatedAssetId.Value &&
                            a.Status != AssetStatus.Retired);

                if (!assetExists)
                {
                    ModelState.AddModelError("RelatedAssetId","请选择有效的资产。");

                    await LoadCreateLists(model);

                    return View(model);
                }
            }

            await using IDbContextTransaction transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            try
            {
                DateTime now = DateTime.UtcNow;
                DateTime today = now.Date;

                TicketNumberCounter? counter =
                    await _context.TicketNumberCounters
                        .FirstOrDefaultAsync(c => c.CounterDate == today);

                if (counter == null)
                {
                    counter = new TicketNumberCounter
                    {
                        CounterDate = today,
                        CurrentNumber = 1
                    };

                    _context.TicketNumberCounters.Add(counter);
                }
                else
                {
                    counter.CurrentNumber++;
                }

                string ticketNumber = $"TK-{today:yyyyMMdd}-{counter.CurrentNumber:D4}";

                // 创建新的工单
                Ticket ticket = new Ticket
                {
                    TicketNumber = ticketNumber,
                    Title = model.Title,
                    Description = model.Description,
                    CategoryId = model.CategoryId,
                    RelatedAssetId = model.RelatedAssetId,
                    Status = TicketStatus.Open,
                    Priority = TicketPriority.Normal,
                    CreatedById = currentUser.Id,
                    CreatedAt = now
                };

                _context.Tickets.Add(ticket);

                // 创建工单历史记录
                TicketHistory history = new TicketHistory
                {
                    Ticket = ticket,
                    PerformedById = currentUser.Id,
                    ActionType = TicketActionType.Created,
                    FromStatus = null,
                    ToStatus = TicketStatus.Open,
                    Description = "创建工单",
                    CreatedAt = now
                };

                _context.TicketHistories.Add(history);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["TicketSuccess"] = $"工单 {ticket.TicketNumber} 创建成功。";

                return RedirectToAction("Index");
            }
            catch
            {
                await transaction.RollbackAsync();

                throw;
            }
        }

        #region 查看工单
        /// <summary>
        /// 查看工单详情
        /// </summary>
        /// <param name="id">工单Id</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            // 获取当前登录用户
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            // 创建工单查询对象
            // 初始条件：根据工单Id查询指定工单
            // 后续根据用户角色追加访问限制
            IQueryable<Ticket> query =
                _context.Tickets
                    .Where(t => t.TicketId == id);

            // Employee只能查看自己创建的工单
            if (User.IsInRole("Employee"))
            {
                query = query.Where(t =>
                    t.CreatedById == currentUser.Id);
            }

            // Technician只能查看分配给自己的工单
            else if (User.IsInRole("Technician"))
            {
                query = query.Where(t =>
                    t.AssignedToId == currentUser.Id);
            }

            // Admin:
            // 不增加限制，可以查看全部工单

            // 查询工单实体，同时加载
            Ticket? ticket =
                await query
                    // 加载工单分类
                    .Include(t => t.Category)
                    // 加载创建人
                    .Include(t => t.CreatedBy)
                    // 加载负责人
                    .Include(t => t.AssignedTo)
                    // 加载关联资产
                    .Include(t => t.RelatedAsset)
                    // 加载员工补充信息以及提交用户
                    .Include(t => t.Supplements)
                        .ThenInclude(s => s.User)
                    // 加载历史记录以及执行用户
                    .Include(t => t.Histories)
                        .ThenInclude(h => h.PerformedBy)
                    .FirstOrDefaultAsync();

            if (ticket == null)
            {
                return NotFound();
            }


            // 将实体转换为详情页面需要的 ViewModel
            TicketDetailsViewModel model =
                new TicketDetailsViewModel
                {
                    TicketId = ticket.TicketId,
                    TicketNumber = ticket.TicketNumber,
                    Title = ticket.Title,
                    Description = ticket.Description,
                    CategoryName = ticket.Category.CategoryName,
                    Priority = ticket.Priority,
                    Status = ticket.Status,

                    // 创建人信息
                    CreatedByName = ticket.CreatedBy.Name + "(" +ticket.CreatedBy.UserName +")",
                    // 当前负责人信息
                    AssignedToName = ticket.AssignedTo == null? null: ticket.AssignedTo.Name +"(" +ticket.AssignedTo.UserName + ")",
                    // 关联资产信息
                    RelatedAssetName =ticket.RelatedAsset == null ? null: ticket.RelatedAsset.AssetNumber +" - " +ticket.RelatedAsset.AssetName,

                    InformationRequest = ticket.InformationRequest,
                    Resolution = ticket.Resolution,
                    CreatedAt = ticket.CreatedAt,
                    AssignedAt = ticket.AssignedAt,
                    StartedAt = ticket.StartedAt,
                    CompletedAt = ticket.CompletedAt,
                    ClosedAt = ticket.ClosedAt
                };

            // 生成补充信息列表
            // 数据来源：
            // 1. TicketHistory:
            //    技术人员请求员工补充信息
            //
            // 2. TicketSupplement:
            //    员工提交补充信息
            model.Supplements =
                ticket.Histories
                    .Where(h => h.ActionType == TicketActionType.RequestedInformation)
                    .Select(h => new TicketSupplementItemViewModel
                    {
                        UserName = h.PerformedBy == null ? "系统"  : h.PerformedBy.Name + "(" + h.PerformedBy.UserName +")",
                        Type = "请求补充",
                        Content = h.Description ?? string.Empty,
                        CreatedAt = h.CreatedAt
                    })
                    .Concat(
                        ticket.Supplements
                            .Select(s => new TicketSupplementItemViewModel
                            {
                                UserName = s.User.Name +"(" +s.User.UserName + ")",
                                Type = "员工补充",
                                Content = s.Content,
                                CreatedAt = s.CreatedAt
                            })
                    )
                    .OrderBy(s => s.CreatedAt)
                    .ToList();

            return View(model);
        }
        #endregion

        #region 工单分配
        /// <summary>
        /// 显示工单分配页面
        /// </summary>
        /// <param name="id">工单Id</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Assign(int id)
        {
            Ticket? ticket =
                await _context.Tickets
                    .FirstOrDefaultAsync(t => t.TicketId == id);

            if (ticket == null)
            {
                return NotFound();
            }

            if (ticket.Status != TicketStatus.Open)
            {
                TempData["TicketError"] = "只有 Open 状态的工单可以分配。";

                return RedirectToAction("Index");
            }

            IList<ApplicationUser> technicians = await _userManager.GetUsersInRoleAsync("Technician");

            AssignTicketViewModel model =
                new AssignTicketViewModel
                {
                    TicketId = ticket.TicketId,
                    TicketNumber = ticket.TicketNumber,
                    Title = ticket.Title,

                    Technicians =
                        technicians
                            .Where(u => u.IsActive)
                            .OrderBy(u => u.Name)
                            .Select(u => new SelectListItem
                            {
                                Value = u.Id,
                                Text = u.Name + " (" + u.UserName + ")"
                            })
                            .ToList()
                };

            return View(model);
        }

        /// <summary>
        /// 处理工单分配
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(AssignTicketViewModel model)
        {
            ApplicationUser? currentUser =
                await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Challenge();
            }

            Ticket? ticket =
                await _context.Tickets
                    .FirstOrDefaultAsync(t => t.TicketId == model.TicketId);

            if (ticket == null)
            {
                return NotFound();
            }

            if (ticket.Status != TicketStatus.Open)
            {
                TempData["TicketError"] = "只有 Open 状态的工单可以进行首次分配。";

                return RedirectToAction("Index");
            }

            ApplicationUser? technician =
                await _userManager.FindByIdAsync(model.TechnicianId);

            if (technician == null ||
                !technician.IsActive ||
                !await _userManager.IsInRoleAsync(technician,"Technician"))
            {
                ModelState.AddModelError(
                    "TechnicianId",
                    "请选择有效的技术人员。");
            }

            if (!ModelState.IsValid)
            {
                IList<ApplicationUser> technicians =
                    await _userManager.GetUsersInRoleAsync("Technician");

                model.Technicians =
                    technicians
                        .Where(u => u.IsActive)
                        .OrderBy(u => u.Name)
                        .Select(u => new SelectListItem
                        {
                            Value = u.Id,
                            Text = u.Name + " (" + u.UserName + ")"
                        })
                        .ToList();

                model.TicketNumber = ticket.TicketNumber;
                model.Title = ticket.Title;

                return View(model);
            }

            DateTime now = DateTime.UtcNow;

            ticket.AssignedToId = technician!.Id;
            ticket.AssignedAt = now;
            ticket.Status = TicketStatus.Assigned;

            TicketHistory history = new TicketHistory
            {
                TicketId = ticket.TicketId,
                PerformedById = currentUser.Id,
                ActionType = TicketActionType.Assigned,
                FromStatus = TicketStatus.Open,
                ToStatus = TicketStatus.Assigned,
                Description =
                    "工单已分配给 " +
                    technician.Name +
                    " (" +
                    technician.UserName +
                    ")",
                CreatedAt = now
            };

            _context.TicketHistories.Add(history);

            await _context.SaveChangesAsync();

            TempData["TicketSuccess"] =
                $"工单 {ticket.TicketNumber} 分配成功。";

            return RedirectToAction("Index");
        }

        #endregion

        #region 处理工单
        /// <summary>
        /// 技术人员开始处理工单
        /// </summary>
        /// <param name="id">工单Id</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Technician")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start(int id)
        {
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            Ticket? ticket =
                await _context.Tickets
                    .FirstOrDefaultAsync(t =>
                        t.TicketId == id);

            if (ticket == null)
            {
                return NotFound();
            }

            // 判断是否分配给当前技术员
            if (ticket.AssignedToId != currentUser.Id)
            {
                return Forbid();
            }

            // 只能处理已分配状态的工单
            if (ticket.Status != TicketStatus.Assigned)
            {
                TempData["TicketError"] = "当前工单状态无法开始处理。";

                return RedirectToAction("Index");
            }

            // 更新工单状态为 InProgress，并记录开始处理时间
            DateTime now = DateTime.UtcNow;
            ticket.Status = TicketStatus.InProgress;
            ticket.StartedAt = now;

            TicketHistory history =
                new TicketHistory
                {
                    TicketId = ticket.TicketId,
                    PerformedById = currentUser.Id,
                    ActionType = TicketActionType.Started,
                    FromStatus = TicketStatus.Assigned,
                    ToStatus = TicketStatus.InProgress,
                    Description = "技术人员开始处理工单",
                    CreatedAt = now
                };

            _context.TicketHistories.Add(history);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        #endregion

        #region 完成工单
        /// <summary>
        /// 技术人员完成工单
        /// </summary>
        /// <param name="id">工单Id</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Technician")]
        public async Task<IActionResult> Complete(int id)
        {
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            Ticket? ticket =
                await _context.Tickets
                    .FirstOrDefaultAsync(t =>
                        t.TicketId == id);

            if (ticket == null)
            {
                return NotFound();
            }

            if (ticket.AssignedToId != currentUser.Id)
            {
                return Forbid();
            }

            if (ticket.Status != TicketStatus.InProgress)
            {
                TempData["TicketError"] = "当前工单状态无法完成处理。";

                return RedirectToAction("Index");
            }

            return View(ticket);
        }

        /// <summary>
        /// 提交完成工单
        /// </summary>
        /// <param name="id">工单Id</param>
        /// <param name="resolution">处理结果</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Technician")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id,string resolution)
        {
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            Ticket? ticket =
                await _context.Tickets
                    .FirstOrDefaultAsync(t =>
                        t.TicketId == id);

            if (ticket == null)
            {
                return NotFound();
            }

            if (ticket.AssignedToId != currentUser.Id)
            {
                return Forbid();
            }

            if (ticket.Status != TicketStatus.InProgress)
            {
                TempData["TicketError"] = "当前工单状态无法完成处理。";

                return RedirectToAction("Index");
            }

            // 更新工单状态为 Completed，并记录完成时间和处理结果
            DateTime now = DateTime.UtcNow;
            ticket.Status = TicketStatus.Completed;
            ticket.CompletedAt = now;
            ticket.Resolution = resolution;
            //ticket.AutoCloseAt = now.AddDays(3); // 设置自动关闭时间为3天后
            ticket.AutoCloseAt = now.AddSeconds(5); // 设置自动关闭时间为5秒后，方便测试

            TicketHistory history =
                new TicketHistory
                {
                    TicketId = ticket.TicketId,
                    PerformedById = currentUser.Id,
                    ActionType = TicketActionType.Completed,
                    FromStatus = TicketStatus.InProgress,
                    ToStatus = TicketStatus.Completed,
                    Description = "技术人员完成工单",
                    CreatedAt = now
                };

            _context.TicketHistories.Add(history);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        #endregion

        #region 补充信息
        /// <summary>
        /// 技术人员请求员工补充信息
        /// </summary>
        /// <param name="id">工单Id</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Technician")]
        public async Task<IActionResult> RequestInformation(int id)
        {
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            Ticket? ticket =
                await _context.Tickets
                    .FirstOrDefaultAsync(t =>
                        t.TicketId == id);

            if (ticket == null)
            {
                return NotFound();
            }

            if (ticket.AssignedToId != currentUser.Id)
            {
                return Forbid();
            }

            if (ticket.Status != TicketStatus.InProgress)
            {
                TempData["TicketError"] = "当前工单状态无法请求补充信息。";

                return RedirectToAction("Index");
            }

            return View(ticket);
        }

        /// <summary>
        /// 提交请求补充信息
        /// </summary>
        /// <param name="id">工单Id</param>
        /// <param name="informationRequest">需要补充的信息</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Technician")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestInformation(int id,string informationRequest)
        {
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            Ticket? ticket =
                await _context.Tickets
                    .FirstOrDefaultAsync(t =>
                        t.TicketId == id);

            if (ticket == null)
            {
                return NotFound();
            }

            if (ticket.AssignedToId != currentUser.Id)
            {
                return Forbid();
            }

            if (ticket.Status != TicketStatus.InProgress)
            {
                TempData["TicketError"] = "当前工单状态无法请求补充信息。";

                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(informationRequest))
            {
                ModelState.AddModelError("informationRequest","请输入需要员工补充的信息。");

                return View(ticket);
            }

            // 更新工单状态为 WaitingForUser，并记录请求的信息
            DateTime now = DateTime.UtcNow;
            ticket.Status = TicketStatus.WaitingForUser;
            ticket.InformationRequest = informationRequest;

            TicketHistory history =
                new TicketHistory
                {
                    TicketId = ticket.TicketId,
                    PerformedById = currentUser.Id,
                    ActionType = TicketActionType.RequestedInformation,
                    FromStatus = TicketStatus.InProgress,
                    ToStatus = TicketStatus.WaitingForUser,
                    Description = informationRequest,
                    CreatedAt = now
                };

            _context.TicketHistories.Add(history);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        /// <summary>
        /// 员工补充工单信息页面
        /// </summary>
        /// <param name="id">工单Id</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> AddSupplement(int id)
        {
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            Ticket? ticket =
                await _context.Tickets
                    .FirstOrDefaultAsync(t =>
                        t.TicketId == id &&
                        t.CreatedById == currentUser.Id);

            if (ticket == null)
            {
                return NotFound();
            }

            if (ticket.Status != TicketStatus.WaitingForUser)
            {
                TempData["TicketError"] = "当前工单不需要补充信息。";

                return RedirectToAction("Index");
            }

            return View(ticket);
        }

        /// <summary>
        /// 保存员工补充信息
        /// </summary>
        /// <param name="id">工单Id</param>
        /// <param name="content">补充内容</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Employee")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSupplement(int id,string content)
        {
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            Ticket? ticket =
                await _context.Tickets
                    .FirstOrDefaultAsync(t =>
                        t.TicketId == id &&
                        t.CreatedById == currentUser.Id);

            if (ticket == null)
            {
                return NotFound();
            }

            if (ticket.Status != TicketStatus.WaitingForUser)
            {
                TempData["TicketError"] = "当前工单状态无法补充信息。";

                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                ModelState.AddModelError("content","请输入补充内容。");

                return View(ticket);
            }


            DateTime now = DateTime.UtcNow;


            TicketSupplement supplement =
                new TicketSupplement
                {
                    TicketId = ticket.TicketId,
                    UserId = currentUser.Id,
                    Content = content,
                    CreatedAt = now
                };

            _context.TicketSupplements.Add(supplement);

            ticket.Status = TicketStatus.InProgress;

            ticket.InformationRequest = null;


            TicketHistory history =
                new TicketHistory
                {
                    TicketId = ticket.TicketId,
                    PerformedById = currentUser.Id,
                    ActionType = TicketActionType.InformationProvided,
                    FromStatus = TicketStatus.WaitingForUser,
                    ToStatus = TicketStatus.InProgress,
                    Description = "员工补充工单信息",
                    CreatedAt = now
                };


            _context.TicketHistories.Add(history);


            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        #endregion

        #region 关闭工单
        /// <summary>
        /// 员工确认问题解决，关闭工单
        /// </summary>
        /// <param name="id">工单Id</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Employee")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Close(int id)
        {
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            Ticket? ticket =
                await _context.Tickets
                    .FirstOrDefaultAsync(t =>
                        t.TicketId == id);

            if (ticket == null)
            {
                return NotFound();
            }

            // 只能关闭自己创建的工单
            if (ticket.CreatedById != currentUser.Id)
            {
                return Forbid();
            }

            // 只有 Completed 状态可以关闭
            if (ticket.Status != TicketStatus.Completed)
            {
                TempData["TicketError"] = "当前工单状态无法关闭。";

                return RedirectToAction("Index");
            }

            DateTime now = DateTime.UtcNow;
            // 更新工单状态为 Closed
            ticket.Status = TicketStatus.Closed;
            // 记录关闭时间
            ticket.ClosedAt = now;
            // 清除自动关闭时间，因为工单已经被员工手动关闭
            ticket.AutoCloseAt = null; 

            TicketHistory history =
                new TicketHistory
                {
                    TicketId = ticket.TicketId,
                    PerformedById = currentUser.Id,
                    ActionType = TicketActionType.Closed,
                    FromStatus = TicketStatus.Completed,
                    ToStatus = TicketStatus.Closed,
                    Description = "员工确认问题解决，关闭工单",
                    CreatedAt = now
                };

            _context.TicketHistories.Add(history);

            await _context.SaveChangesAsync();

            TempData["TicketSuccess"] = $"工单 {ticket.TicketNumber} 已关闭。";

            return RedirectToAction("Index");
        }
        #endregion

        /// <summary>
        /// 加载创建工单所需的下拉列表数据
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private async Task LoadCreateLists(CreateTicketViewModel model)
        {
            model.Categories =
                await _context.TicketCategories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.CategoryName)
                    .Select(c => new SelectListItem
                    {
                        Value = c.CategoryId.ToString(),
                        Text = c.CategoryName
                    })
                    .ToListAsync();

            model.Assets =
                await _context.Assets
                    .Where(a => a.Status != AssetStatus.Retired)
                    .OrderBy(a => a.AssetNumber)
                    .Select(a => new SelectListItem
                    {
                        Value = a.AssetId.ToString(),
                        Text = a.AssetNumber + " - " + a.AssetName
                    })
                    .ToListAsync();
        }

        /// <summary>
        /// 查询工单当前状态
        /// 用于 AJAX 轮询获取最新状态
        /// </summary>
        /// <param name="id">工单Id</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetStatus(int id)
        {
            Ticket? ticket =
                await _context.Tickets
                    .FirstOrDefaultAsync(t => t.TicketId == id);

            if (ticket == null)
            {
                return NotFound();
            }

            return Json(new
            {
                status = ticket.Status.ToString()
            });
        }
    }
}