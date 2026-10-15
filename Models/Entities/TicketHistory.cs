using EITSTMS.Models.Enums;

namespace EITSTMS.Models.Entities
{
    public class TicketHistory
    {
        public int HistoryId { get; set; }                         // 历史记录主键

        public int TicketId { get; set; }                          // 所属工单 Id

        public string? PerformedById { get; set; }                 // 执行操作的用户 Id；系统自动操作时可为空

        public TicketActionType ActionType { get; set; }           // 操作类型

        public TicketStatus? FromStatus { get; set; }              // 操作前状态；没有前置状态时可为空

        public TicketStatus? ToStatus { get; set; }                // 操作后状态；未改变状态时可为空

        public string? Description { get; set; }                   // 操作说明，例如“优先级由 Normal 调整为 High”

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 操作发生时间

        public Ticket Ticket { get; set; } = null!;                // 导航属性：所属工单完整对象

        public ApplicationUser? PerformedBy { get; set; }          // 导航属性：执行该操作的用户
    }
}