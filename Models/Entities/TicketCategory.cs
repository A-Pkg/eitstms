namespace EITSTMS.Models.Entities
{
    public class TicketCategory
    {
        public int CategoryId { get; set; }                          // 工单分类主键

        public string CategoryName { get; set; } = string.Empty;     // 分类名称，例如 Hardware、Software

        public string? Description { get; set; }                     // 分类说明，可为空

        public bool IsActive { get; set; } = true;                   // 分类是否启用

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;   // 分类创建时间，统一保存 UTC
    }
}
