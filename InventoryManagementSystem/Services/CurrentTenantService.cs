using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace InventoryManagementSystem.Services
{
    public interface ICurrentTenantService
    {
        int? TenantId { get; }
    }

    public class CurrentTenantService : ICurrentTenantService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentTenantService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? TenantId
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;

                if (user == null || !user.Identity!.IsAuthenticated)
                    return null;

                var tenantClaim = user.FindFirst("TenantId")?.Value;

                return int.TryParse(tenantClaim, out var id) ? id : null;
            }
        }
    }
}