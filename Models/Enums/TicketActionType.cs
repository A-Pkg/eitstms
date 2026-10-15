namespace EITSTMS.Models.Enums
{
    public enum TicketActionType
    {
        Created = 1,              // 创建工单
        PriorityChanged = 2,      // 修改优先级
        Assigned = 3,             // 分配技术人员
        Reassigned = 4,           // 重新分配技术人员
        Started = 5,              // 开始处理
        RequestedInformation = 6, // 请求员工补充信息
        InformationProvided = 7,  // 员工已补充信息
        Completed = 8,            // 本次处理完成
        Closed = 9,               // 手动关闭
        AutoClosed = 10,          // 系统自动关闭
        Cancelled = 11            // 取消工单
    }
}