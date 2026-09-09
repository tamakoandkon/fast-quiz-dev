using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Auth.Domain.Repositories;
using FastQuiz.Auth.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FastQuiz.Auth.Infrastructure.Initializer;

public static class AuthDbSeedData
{
    public static async Task InitializeAsync(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default
    )
    {
        using var scope = serviceProvider.CreateScope();
        var tenantRepository = scope.ServiceProvider.GetRequiredService<ITenantRepository>();
        var organizationRepository =
            scope.ServiceProvider.GetRequiredService<IOrganizationRepository>();
        var roleRepository = scope.ServiceProvider.GetRequiredService<IRoleRepository>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        await CreateDefaultTenantAsync(tenantRepository).ConfigureAwait(false);

        await CreateDefaultRoleAsync(roleRepository).ConfigureAwait(false);

        await CreateDefaultAdminAsync(
                userRepository,
                passwordHasher,
                roleRepository,
                tenantRepository
            )
            .ConfigureAwait(false);
    }

    public static async Task CreateDefaultTenantAsync(ITenantRepository tenantRepository)
    {
        var defaultTenant = await tenantRepository
            .FindByIdAsync(Tenant.DefaultTenantId)
            .ConfigureAwait(false);
        if (defaultTenant == null)
        {
            defaultTenant = new Tenant(Tenant.DefaultTenantId, Tenant.DefaultTenantName, "");

            await tenantRepository.CreateAsync(defaultTenant).ConfigureAwait(false);
        }
    }

    public static async Task CreateDefaultRoleAsync(IRoleRepository roleRepository)
    {
        var defaultRole = await roleRepository
            .FindByIdAsync(Role.DefaultAdminRoleId)
            .ConfigureAwait(false);
        if (defaultRole == null)
        {
            defaultRole = new Role(
                Role.DefaultAdminRoleId,
                Tenant.DefaultTenantId,
                Role.DefaultAdminRoleName,
                "",
                true,
                true
            );

            await roleRepository.CreateAsync(defaultRole).ConfigureAwait(false);
        }

        await roleRepository.ClearPermissionsAsync(Role.DefaultAdminRoleId).ConfigureAwait(false);
        await roleRepository.AddPermissionsAsync(
            Role.DefaultAdminRoleId,
            Permissions.AllPermissions()
        );
    }

    public static async Task CreateDefaultAdminAsync(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IRoleRepository roleRepository,
        ITenantRepository tenantRepository
    )
    {
        var defaultAdmin = await userRepository
            .FindByIdAsync(User.DefaultAdminId)
            .ConfigureAwait(false);
        if (defaultAdmin == null)
        {
            defaultAdmin = new User(
                User.DefaultAdminId,
                User.DefaultAdminUsername,
                null,
                passwordHasher.Hash(User.DefaultAdminPassword)
            );

            await userRepository.CreateAsync(defaultAdmin).ConfigureAwait(false);
        }

        var defaultUserRole = await roleRepository
            .ExistsUserRoleAsync(User.DefaultAdminId, Role.DefaultAdminRoleId)
            .ConfigureAwait(false);
        if (!defaultUserRole)
        {
            await roleRepository
                .AddRoleToUserAsync(
                    User.DefaultAdminId,
                    Role.DefaultAdminRoleId,
                    Tenant.DefaultTenantId
                )
                .ConfigureAwait(false);
        }

        var tenantUserMapExisted = await tenantRepository
            .GetUserInTenantAsync(Tenant.DefaultTenantId, User.DefaultAdminId)
            .ConfigureAwait(false);

        if (!tenantUserMapExisted)
        {
            await tenantRepository
                .AddUserToTenantAsync(Tenant.DefaultTenantId, User.DefaultAdminId)
                .ConfigureAwait(false);
        }
    }
}
