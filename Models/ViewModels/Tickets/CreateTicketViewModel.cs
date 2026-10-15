using System.ComponentModel.DataAnnotations;
using EITSTMS.Models.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EITSTMS.Models.ViewModels.Tickets
{
    public class CreateTicketViewModel
    {
        [Required]
        public string Title { get; set; } = string.Empty; // 工单标题

        [Required]
        public string Description { get; set; } = string.Empty; // 问题详细描述

        [Required]
        public int CategoryId { get; set; } // 选择的工单分类 Id

        public int? RelatedAssetId { get; set; } // 关联资产 Id，可为空

        public List<SelectListItem> Categories { get; set; } = new List<SelectListItem>(); // 分类下拉列表

        public List<SelectListItem> Assets { get; set; } = new List<SelectListItem>(); // 资产下拉列表
    }
}