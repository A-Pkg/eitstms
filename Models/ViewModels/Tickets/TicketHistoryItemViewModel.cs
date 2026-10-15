using EITSTMS.Models.Enums;

namespace EITSTMS.Models.ViewModels.Tickets
{
    /// <summary>
    /// 工单历史记录显示模型
    /// 用于展示工单的操作记录
    /// </summary>
    public class TicketHistoryItemViewModel
    {
        // 工单编号
        // 例如：TK-20261004-0001
        public string TicketNumber { get; set; } = string.Empty;

        // 操作类型
        // 例如：Created、Assigned、Completed
        public TicketActionType ActionType { get; set; }

        // 操作前状态
        // 例如：Open
        // 如果没有前置状态，可以为空
        public TicketStatus? FromStatus { get; set; }

        // 操作后状态
        // 例如：Assigned
        // 如果操作没有改变状态，可以为空
        public TicketStatus? ToStatus { get; set; }

        // 操作说明
        // 例如：分配给技术人员 Tom
        public string? Description { get; set; }


        // 执行操作的用户
        // 如果是系统自动操作，则显示系统
        public string PerformedByName { get; set; } = string.Empty;

        // 操作发生时间
        public DateTime CreatedAt { get; set; }
    }
}