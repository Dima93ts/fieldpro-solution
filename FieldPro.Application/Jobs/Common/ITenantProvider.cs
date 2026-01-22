namespace FieldPro.Application.Tenancy; // o Common, se usi quel nome

public interface ITenantProvider
{
    string GetCurrentTenantId();
}
