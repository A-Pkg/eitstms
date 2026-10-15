using EITSTMS.Models.Enums;

namespace EITSTMS.Models.ViewModels.Tickets
{
    public class TicketListItemViewModel
    {
        public int TicketId { get; set; } // 工单 Id

        public string TicketNumber { get; set; } = string.Empty; // 工单编号

        public string Title { get; set; } = string.Empty; // 工单标题

        public string CategoryName { get; set; } = string.Empty; // 工单分类名称

        public TicketStatus Status { get; set; } // 工单状态

        public TicketPriority Priority { get; set; } // 工单优先级

        public string? AssignedToName { get; set; } // 当前负责技术人员姓名

        public DateTime CreatedAt { get; set; } // 创建时间
    }
}