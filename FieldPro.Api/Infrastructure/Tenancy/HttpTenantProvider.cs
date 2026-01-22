using FieldPro.Application.Tenancy;
using Microsoft.AspNetCore.Http;

namespace FieldPro.Api.Tenancy; // cambia se usi un namespace diverso

public class HttpTenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpTenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetCurrentTenantId()
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("No HttpContext");

        if (!httpContext.Request.Headers.TryGetValue("X-Tenant", out var tenant) ||
            string.IsNullOrWhiteSpace(tenant))
        {
            throw new InvalidOperationException("Tenant non specificato");
        }

        return tenant.ToString();
    }
}
