using EITSTMS.Models.Enums;

namespace EITSTMS.Models.ViewModels.Tickets
{
    /// <summary>
    /// 工单历史查询条件
    /// </summary>
    public class TicketHistoryQueryViewModel
    {
        public string? TicketNumber { get; set; } // 工单编号

        public TicketActionType? ActionType { get; set; } // 操作类型

        public DateTime? StartDate { get; set; } // 开始时间

        public DateTime? EndDate { get; set; } // 结束时间
    }
}