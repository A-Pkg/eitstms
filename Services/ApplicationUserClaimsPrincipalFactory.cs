using EITSTMS.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace EITSTMS.Services
{
    /// <summary>
    /// 自定义用户 Claims 工厂
    /// 每次 Identity 创建用户身份时自动加入业务 Claim
    /// </summary>
    public class ApplicationUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
    {
        public ApplicationUserClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> options)
            : base(userManager, roleManager, options)
        {

        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            ClaimsIdentity identity = await base.GenerateClaimsAsync(user);


            // 如果用户是系统管理员
            // 加入自定义 Claim
            if (user.IsSystemAdmin)
            {
                identity.AddClaim(new Claim("IsSystemAdmin","true"));
            }

            return identity;
        }
    }
}