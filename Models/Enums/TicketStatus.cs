namespace EITSTMS.Models.Enums
{
    public enum TicketStatus
    {
        Open = 0,           // 待处理
        Assigned = 1,       // 已分配
        InProgress = 2,     // 处理中
        WaitingForUser = 3, // 等待用户补充信息
        Completed = 4,      // 本次处理完成
        Closed = 5,         // 已关闭
        Cancelled = 6       // 已取消
    }
}
