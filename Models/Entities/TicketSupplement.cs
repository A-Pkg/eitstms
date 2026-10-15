namespace EITSTMS.Models.Entities
{
    /// <summary>
    /// 工单补充信息实体类，表示员工对工单的补充内容
    /// </summary>
    public class TicketSupplement
    {
        public int SupplementId { get; set; }                    // 补充信息主键

        public int TicketId { get; set; }                        // 所属工单 Id

        public string UserId { get; set; } = string.Empty;       // 提交补充信息的用户 Id

        public string Content { get; set; } = string.Empty;      // 员工提交的补充内容

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 补充信息提交时间

        public Ticket Ticket { get; set; } = null!;              // 导航属性：所属工单完整对象

        public ApplicationUser User { get; set; } = null!;       // 导航属性：提交该补充信息的用户
    }
}