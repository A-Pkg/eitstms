using EITSTMS.Models.Enums;

namespace EITSTMS.Models.Entities
{
    public class Ticket
    {
        public int TicketId { get; set; }                            // 工单主键

        public string TicketNumber { get; set; } = string.Empty;     // 工单编号，例如 TK-20261004-0001

        public string Title { get; set; } = string.Empty;            // 工单标题

        public string Description { get; set; } = string.Empty;      // 问题详细描述

        public int CategoryId { get; set; }                          // 工单分类外键

        public TicketStatus Status { get; set; } = TicketStatus.Open; // 当前工单状态

        public TicketPriority Priority { get; set; }
            = TicketPriority.Normal;                                 // 工单优先级，默认 Normal

        public string CreatedById { get; set; } = string.Empty;      // 工单创建人 Id

        public string? AssignedToId { get; set; }                    // 当前负责技术人员 Id，未分配时为空

        public int? RelatedAssetId { get; set; }                     // 相关资产 Id，可为空

        public string? InformationRequest { get; set; }              // 技术人员当前要求员工补充的信息

        public string? Resolution { get; set; }                      // 最终处理结论

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;   // 工单创建时间

        public DateTime? AssignedAt { get; set; }                    // 分配技术人员时间

        public DateTime? StartedAt { get; set; }                     // 技术人员开始处理时间

        public DateTime? CompletedAt { get; set; }                   // 本次处理完成时间

        public DateTime? AutoCloseAt { get; set; }                   // 计划自动关闭时间

        public DateTime? ClosedAt { get; set; }                      // 最终关闭时间


        public TicketCategory Category { get; set; } = null!;        // 导航属性：工单分类完整对象
        public ApplicationUser CreatedBy { get; set; } = null!;      // 导航属性：创建该工单的用户
        public ApplicationUser? AssignedTo { get; set; }             // 导航属性：当前负责的技术人员
        public Asset? RelatedAsset { get; set; }                     // 导航属性：关联资产完整对象
        public ICollection<TicketSupplement> Supplements { get; set; }
            = new List<TicketSupplement>();                          // 导航属性：该工单的全部补充信息
        public ICollection<TicketHistory> Histories { get; set; }
            = new List<TicketHistory>();                             // 导航属性：该工单的全部历史记录
    }
}