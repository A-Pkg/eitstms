using EITSTMS.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EITSTMS.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options)
        {
        }

        public DbSet<Department> Departments { get; set; } = null!;                 // 部门表

        public DbSet<TicketCategory> TicketCategories { get; set; } = null!;       // 工单分类表

        public DbSet<Ticket> Tickets { get; set; } = null!;                       // 工单表

        public DbSet<TicketSupplement> TicketSupplements { get; set; } = null!;   // 工单补充信息表

        public DbSet<TicketHistory> TicketHistories { get; set; } = null!;         // 工单历史记录表

        public DbSet<Asset> Assets { get; set; } = null!;                         // IT 资产表

        public DbSet<TicketNumberCounter> TicketNumberCounters { get; set; }      // 工单流水号计数器表

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 获取 ApplicationDbContext 所在的程序集，也就是 EITSTMS 项目编译出来的程序集。
            //自动扫描当前程序集里所有实现了 IEntityTypeConfiguration<T> 的配置类，然后把这些 Fluent API 配置全部应用到 EF Core
            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}