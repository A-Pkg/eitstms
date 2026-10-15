using EITSTMS.Data;
using EITSTMS.Models.Entities;
using EITSTMS.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace EITSTMS.Services
{
    /// <summary>
    /// 工单自动关闭后台服务
    /// 定时检查已完成但超过自动关闭时间的工单，并自动将其关闭
    /// </summary>
    public class TicketAutoCloseService : BackgroundService
    {
        // 用于创建依赖注入作用域
        // 因为 BackgroundService 生命周期是单例，不能直接注入 DbContext（DbContext 默认是 Scoped）
        private readonly IServiceProvider _serviceProvider;

        public TicketAutoCloseService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 后台服务启动后执行的方法，程序运行期间会一直执行
        /// </summary>
        /// <param name="stoppingToken">应用程序停止通知</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // 检查是否存在需要自动关闭的工单
                await CloseExpiredTickets();

                // 每天检查一次
                // 实际项目中可以根据业务调整，例如每小时检查一次
                //await Task.Delay( TimeSpan.FromDays(1),stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
            }
        }

        /// <summary>
        /// 查询并关闭超过自动关闭时间的工单
        /// </summary>
        private async Task CloseExpiredTickets()
        {
            // 创建依赖注入作用域
            // 用于获取当前作用域内的 DbContext
            using IServiceScope scope = _serviceProvider.CreateScope();

            // 获取数据库上下文
            ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // 获取当前 UTC 时间
            DateTime now = DateTime.UtcNow;

            // 查询：
            // 1. 当前状态必须是 Completed
            // 2. 设置了自动关闭时间
            // 3. 自动关闭时间已经到达
            List<Ticket> tickets =
                await context.Tickets
                    .Where(t =>
                        t.Status == TicketStatus.Completed &&
                        t.AutoCloseAt != null &&
                        t.AutoCloseAt <= now)
                    .ToListAsync();

            foreach (Ticket ticket in tickets)
            {
                // 更新工单状态为已关闭
                ticket.Status = TicketStatus.Closed;

                // 记录最终关闭时间
                ticket.ClosedAt = now;

                // 清除自动关闭计划时间
                // 因为工单已经关闭，不需要再次进入自动关闭检查
                ticket.AutoCloseAt = null;

                // 创建历史记录
                // 说明该关闭操作不是用户执行，而是系统执行
                TicketHistory history =
                    new TicketHistory
                    {
                        TicketId = ticket.TicketId,
                        ActionType = TicketActionType.AutoClosed,
                        FromStatus = TicketStatus.Completed,
                        ToStatus = TicketStatus.Closed,
                        Description = "系统自动关闭工单",
                        CreatedAt = now
                    };

                // 添加历史记录
                context.TicketHistories.Add(history);
            }


            // 一次性保存：
            // 1. 工单状态修改
            // 2. 关闭时间修改
            // 3. 历史记录新增
            await context.SaveChangesAsync();
        }
    }
}