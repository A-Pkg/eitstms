namespace EITSTMS.Models.ViewModels.Tickets
{
    public class TicketSupplementItemViewModel
    {
        public string UserName { get; set; } = string.Empty; // 操作人

        public string Type { get; set; } = string.Empty; // 请求补充/员工补充

        public string Content { get; set; } = string.Empty; // 内容

        public DateTime CreatedAt { get; set; } // 时间
    }
}