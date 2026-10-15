using EITSTMS.Data;
using EITSTMS.Models.Entities;
using EITSTMS.Models.ViewModels.Tickets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EITSTMS.Controllers
{
    /// <summary>
    /// 工单历史管理
    /// 只有初始系统管理员可以访问
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class TicketHistoriesController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public TicketHistoriesController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        /// <summary>
        /// 查看全部工单历史记录
        /// 支持根据工单编号、操作类型、时间范围查询
        /// </summary>
        /// <param name="queryModel">查询条件</param>
        /// <returns></returns>
        public async Task<IActionResult> Index(TicketHistoryQueryViewModel queryModel)
        {
            // 获取当前登录用户
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            // 只有初始系统管理员可以查看全部历史
            if (!currentUser.IsSystemAdmin)
            {
                return Forbid();
            }

            // 查询全部工单历史
            // 同时加载：
            // 1. 所属工单
            // 2. 执行操作的用户
            IQueryable<TicketHistory> query =
                _context.TicketHistories
                    .Include(h => h.Ticket)
                    .Include(h => h.PerformedBy);


            // 根据工单编号查询
            if (!string.IsNullOrWhiteSpace(queryModel.TicketNumber))
            {
                query = query.Where(h =>h.Ticket.TicketNumber.Contains(queryModel.TicketNumber));
            }

            // 根据操作类型查询
            if (queryModel.ActionType.HasValue)
            {
                query = query.Where(h =>h.ActionType == queryModel.ActionType.Value);
            }

            // 根据开始时间查询
            if (queryModel.StartDate.HasValue)
            {
                query = query.Where(h =>h.CreatedAt >= queryModel.StartDate.Value);
            }

            // 根据结束时间查询
            if (queryModel.EndDate.HasValue)
            {
                query = query.Where(h =>h.CreatedAt <= queryModel.EndDate.Value);
            }

            // 最新操作显示在最前面
            List<TicketHistoryItemViewModel> histories =
                await query
                    .OrderByDescending(h => h.CreatedAt)

                    // 转换为页面显示模型
                    .Select(h => new TicketHistoryItemViewModel
                    {
                        // 工单编号
                        TicketNumber = h.Ticket.TicketNumber,

                        // 操作类型
                        ActionType = h.ActionType,

                        // 状态变化
                        FromStatus = h.FromStatus,
                        ToStatus = h.ToStatus,

                        // 操作说明
                        Description = h.Description,

                        // 用户操作显示用户名
                        // 系统自动操作没有用户Id
                        PerformedByName = h.PerformedBy == null ? "系统" : h.PerformedBy.Name + "(" + h.PerformedBy.UserName + ")",

                        // 操作时间
                        CreatedAt = h.CreatedAt
                    })
                    .ToListAsync();

            // 保存查询条件
            // 用于页面回显查询条件
            ViewBag.Query = queryModel;

            return View(histories);
        }
    }
}