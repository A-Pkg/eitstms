using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace EITSTMS.Models.ViewModels.Tickets
{
    public class AssignTicketViewModel
    {
        public int TicketId { get; set; } // 工单 Id

        public string TicketNumber { get; set; } = string.Empty; // 工单编号

        public string Title { get; set; } = string.Empty; // 工单标题

        [Required(ErrorMessage = "请选择技术人员")]
        public string TechnicianId { get; set; } = string.Empty; // 选择的技术人员 Id

        public List<SelectListItem> Technicians { get; set; } = new List<SelectListItem>(); // 技术人员下拉列表
    }
}