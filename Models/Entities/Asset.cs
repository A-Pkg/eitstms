using EITSTMS.Models.Enums;

namespace EITSTMS.Models.Entities
{
    public class Asset
    {
        public int AssetId { get; set; }                           // 资产主键

        public string AssetNumber { get; set; } = string.Empty;    // 资产编号，例如 AST-0001

        public string AssetName { get; set; } = string.Empty;      // 资产名称，例如 Dell Latitude 7450

        public AssetCategory Category { get; set; }                // 资产类别

        public AssetStatus Status { get; set; } = AssetStatus.Available; // 资产状态，默认可用

        public string? Manufacturer { get; set; }                  // 制造商，例如 Dell、Lenovo、HP、Apple

        public string? Model { get; set; }                         // 设备型号，例如 Latitude 7450

        public string? SerialNumber { get; set; }                  // 设备序列号，可为空

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 资产记录创建时间

        public ICollection<Ticket> Tickets { get; set; }           // 导航属性：关联该资产的所有工单
            = new List<Ticket>();
    }
}