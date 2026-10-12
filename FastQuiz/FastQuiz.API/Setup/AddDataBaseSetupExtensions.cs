namespace FastQuiz.API.Setup;

public static class AddDataBaseSetupExtensions
{
    public static IServiceCollection AddDataBaseSetup(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Add database context and other related services here
        // Example:
        // services.AddDbContext<MyDbContext>(options =>
        //     options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        return services;
    }
}
