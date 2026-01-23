using FieldPro.Application.Tenancy;
using Microsoft.AspNetCore.Http;

namespace FieldPro.Api.Infrastructure.Tenancy;

public class HttpTenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpTenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string TenantId
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("No HttpContext");

            // 1) Header X-Tenant (frontend React)
            if (httpContext.Request.Headers.TryGetValue("X-Tenant", out var tenant) &&
                !string.IsNullOrWhiteSpace(tenant))
            {
                return tenant.ToString();
            }

            // 2) Fallback definitivo: tenant di default
            return "main";
        }
    }
}
