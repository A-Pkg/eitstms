using EITSTMS.Models.Enums;

namespace EITSTMS.Extensions
{
    /// <summary>
    /// 工单操作类型显示转换
    /// 用于将枚举转换为用户看到的中文名称
    /// </summary>
    public static class TicketActionTypeExtensions
    {
        /// <summary>
        /// 获取操作类型中文名称
        /// </summary>
        public static string ToDisplayName(this TicketActionType actionType)
        {
            return actionType switch
            {
                TicketActionType.Created => "创建工单",

                TicketActionType.Assigned => "分配技术人员",

                TicketActionType.Started => "开始处理",

                TicketActionType.RequestedInformation => "请求补充信息",

                TicketActionType.InformationProvided => "员工补充信息",

                TicketActionType.Completed => "完成处理",

                TicketActionType.Closed => "关闭工单",

                TicketActionType.AutoClosed => "系统自动关闭",

                _ => actionType.ToString()
            };
        }
    }
}