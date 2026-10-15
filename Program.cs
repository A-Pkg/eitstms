using EITSTMS.Data;
using EITSTMS.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EITSTMS.Services;

var builder = WebApplication.CreateBuilder(args);

// 读取 Compose secrets 挂载的文件。
// 文件名作为配置键，文件内容作为配置值。
// 本机通过 Visual Studio 运行时，该目录可以不存在。
builder.Configuration.AddKeyPerFile(
    directoryPath: "/run/secrets",
    optional: true);

// Add services to the container.
builder.Services.AddControllersWithViews();

// 告诉项目“数据库怎么连接、用哪个 DbContext”
builder.Services.AddDbContext<ApplicationDbContext>(options =>              // 注册 ApplicationDbContext 到依赖注入容器
    options.UseSqlServer(                                                   // 指定 EF Core 使用 SQL Server 数据库
        builder.Configuration.GetConnectionString("EITSTMSConnection")));   // 从配置文件读取 EITSTMSConnection 连接字符串

// 告诉项目“用户和角色系统怎么工作、数据存到哪里”
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>()       // 注册 ASP.NET Core Identity；用户使用 ApplicationUser，角色使用默认 IdentityRole
    .AddEntityFrameworkStores<ApplicationDbContext>()   // 指定 Identity 的用户、角色、Claim 等数据通过 ApplicationDbContext + EF Core 存取
    .AddDefaultTokenProviders();                        // 注册默认 Token Provider，供密码重置、邮箱确认等功能使用

// 注册自定义的 Claims 工厂
builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>,ApplicationUserClaimsPrincipalFactory>();

builder.Services.Configure<IdentityOptions>(options =>
{
    //                                  密码规则
    // 密码最少长度
    options.Password.RequiredLength = 3;
    // 密码是否必须包含数字
    options.Password.RequireDigit = false;
    // 密码是否必须包含小写字母
    options.Password.RequireLowercase = false;
    // 密码是否必须包含大写字母
    options.Password.RequireUppercase = false;
    // 密码是否必须包含特殊字符，例如 ! @ # $
    options.Password.RequireNonAlphanumeric = false;
    // 密码中至少需要包含多少个不同字符
    // 例如设置为 1，表示不额外限制字符重复
    options.Password.RequiredUniqueChars = 1;


    //                              登录失败锁定规则
    // 连续登录失败达到 5 次后锁定账号
    options.Lockout.MaxFailedAccessAttempts = 5;
    // 账号被锁定后，默认锁定 5 分钟
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    // 新创建的用户默认允许使用账号锁定机制
    options.Lockout.AllowedForNewUsers = true;


    //                                用户规则
    // 用户名允许出现的字符,null 表示不限制用户名字符
    options.User.AllowedUserNameCharacters = null;
    // 是否要求用户名唯一
    options.User.RequireUniqueEmail = false;


    //                              登录规则
    // 是否要求邮箱确认后才能登录,当前项目没有使用邮箱确认，所以设置为 false
    options.SignIn.RequireConfirmedEmail = false;
    // 是否要求手机号确认后才能登录
    options.SignIn.RequireConfirmedPhoneNumber = false;
    // 是否要求账号确认后才能登录
    options.SignIn.RequireConfirmedAccount = false;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    // 未登录用户访问需要认证的页面时，跳转到这里
    options.LoginPath = "/Account/Login";

    // 用户已登录，但没有权限访问某页面时，跳转到这里
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// 注册后台服务：定时检查工单是否过期并自动关闭
builder.Services.AddHostedService<TicketAutoCloseService>();

var app = builder.Build();

//CreateScope() 是因为启动阶段没有 HTTP 请求作用域，
//而 UserManager / RoleManager / DbContext 需要 Scoped 生命周期；
using (var scope = app.Services.CreateScope())
{
    await DbInitializer.SeedRolesAsync(scope.ServiceProvider);
    await DbInitializer.SeedAdminAsync(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication(); // 先认证：恢复当前用户
app.UseAuthorization();  // 再授权：判断用户是否有权限

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
