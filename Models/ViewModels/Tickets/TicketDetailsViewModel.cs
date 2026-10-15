using EITSTMS.Models.Enums;

namespace EITSTMS.Models.ViewModels.Tickets
{
    public class TicketDetailsViewModel
    {
        public int TicketId { get; set; } // 工单 Id

        public string TicketNumber { get; set; } = string.Empty; // 工单编号

        public string Title { get; set; } = string.Empty; // 工单标题

        public string Description { get; set; } = string.Empty; // 问题描述

        public string CategoryName { get; set; } = string.Empty; // 工单分类

        public TicketStatus Status { get; set; } // 当前状态

        public TicketPriority Priority { get; set; } // 当前优先级

        public string CreatedByName { get; set; } = string.Empty; // 创建人姓名

        public string? AssignedToName { get; set; } // 当前技术人员

        public string? RelatedAssetName { get; set; } // 关联资产

        public string? InformationRequest { get; set; } // 技术人员要求补充的信息

        public string? Resolution { get; set; } // 最终处理结果

        public DateTime CreatedAt { get; set; } // 创建时间

        public DateTime? AssignedAt { get; set; } // 分配时间

        public DateTime? StartedAt { get; set; } // 开始处理时间

        public DateTime? CompletedAt { get; set; } // 完成时间

        public DateTime? ClosedAt { get; set; } // 关闭时间

        // 当前工单的全部处理历史记录
        public List<TicketHistoryItemViewModel> Histories { get; set; } = new List<TicketHistoryItemViewModel>();

        // 当前工单的全部补充信息
        public List<TicketSupplementItemViewModel> Supplements { get; set; } = new List<TicketSupplementItemViewModel>(); 
    }
}