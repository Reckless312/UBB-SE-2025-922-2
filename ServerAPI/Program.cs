using ServerAPI.Controllers;
using ServerAPI.Data;
using Microsoft.EntityFrameworkCore;
using IRepository;
using Repository.AdminDashboard;
using ServerAPI.Repository.AutoChecker;
using Repository.Authentication;
using DataAccess.Service.AdminDashboard.Interfaces;
using DataAccess.Service.AdminDashboard;
using DataAccess.Service.Authentication;
using DataAccess.Service.Authentication.Interfaces;
using DataAccess.AuthProviders;
using DrinkDb_Auth.Service.Authentication;
using DataAccess.Service;
using DataAccess.AuthProviders.Facebook;
using DataAccess.AuthProviders.LinkedIn;
using DataAccess.AuthProviders.Github;
using DrinkDb_Auth.Service.AdminDashboard.Interfaces;
using DrinkDb_Auth.Service.AdminDashboard;
using DrinkDb_Auth.AuthProviders.Google;
using DataAccess.AutoChecker;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure HttpClient
builder.Services.AddHttpClient("DrinkDbClient", client =>
{
    client.BaseAddress = new Uri("http://localhost:5280/");
});

DependencyInjection(builder);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

//app.MapRoleEndpoints();

app.Run();

static void DependencyInjection(WebApplicationBuilder builder)
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    // Add database context to server
    builder.Services.AddDbContext<DatabaseContext>(options =>
        options.UseSqlServer(connectionString));

    // Register DbContextFactory for use in singleton services
    //builder.Services.AddPooledDbContextFactory<DatabaseContext>(options =>
    //    options.UseSqlServer(connectionString));

    // Register Repositories (these are the "real" repositories that will be used by services)
    // Use scoped instead of singleton for repositories that use DbContext
    builder.Services.AddScoped<ISessionRepository, SessionRepository>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IReviewsRepository, ReviewsRepository>();
    builder.Services.AddScoped<IOffensiveWordsRepository, OffensiveWordsRepository>();
    builder.Services.AddScoped<IUpgradeRequestsRepository, UpgradeRequestsRepository>();
    builder.Services.AddScoped<IRolesRepository, RolesRepository>();

    // Register Services (these are the "real" services that will be called by controllers)
    // Use scoped instead of singleton for services that use repositories
    builder.Services.AddScoped<ISessionService, SessionService>();
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IReviewService, ReviewsService>();
    builder.Services.AddScoped<IUpgradeRequestsService, UpgradeRequestsService>();
    builder.Services.AddScoped<IRolesService, RolesService>();
    builder.Services.AddScoped<IAuthenticationService>(sp => new AuthenticationService(
        sp.GetRequiredService<ISessionRepository>(),
        sp.GetRequiredService<IUserRepository>(),
        sp.GetRequiredService<LinkedInLocalOAuthServer>(),
        sp.GetRequiredService<GitHubLocalOAuthServer>(),
        sp.GetRequiredService<FacebookLocalOAuthServer>(),
        sp.GetRequiredService<IBasicAuthenticationProvider>()));

    // Register OAuth Servers
    builder.Services.AddScoped<LinkedInLocalOAuthServer>(sp =>
        new LinkedInLocalOAuthServer("http://localhost:8891/"));
    builder.Services.AddScoped<GitHubLocalOAuthServer>(sp =>
        new GitHubLocalOAuthServer("http://localhost:8890/"));
    builder.Services.AddScoped<FacebookLocalOAuthServer>(sp =>
        new FacebookLocalOAuthServer("http://localhost:8888/"));

    // OAuth helpers and providers - these should be configured based on your server needs
    builder.Services.AddScoped<IGitHubHttpHelper, GitHubHttpHelper>();
    builder.Services.AddScoped<GitHubOAuth2Provider>(sp =>
        new GitHubOAuth2Provider(
            sp.GetRequiredService<IUserRepository>(),
            sp.GetRequiredService<ISessionRepository>(),
            sp.GetRequiredService<IGitHubHttpHelper>()
        ));
    builder.Services.AddScoped<IGitHubOAuthHelper>(sp =>
        new GitHubOAuthHelper(
            sp.GetRequiredService<GitHubOAuth2Provider>(),
            sp.GetRequiredService<GitHubLocalOAuthServer>()
        ));
    builder.Services.AddScoped<IGoogleOAuth2Provider, GoogleOAuth2Provider>();
    builder.Services.AddScoped<IFacebookOAuthHelper, FacebookOAuthHelper>();
    builder.Services.AddScoped<ILinkedInOAuthHelper>(sp => new LinkedInOAuthHelper(
        "86j0ikb93jm78x",
        "WPL_AP1.pg2Bd1XhCi821VTG.+hatTA==",
        "http://localhost:8891/auth",
        "openid profile email"
    ));

    // Other supporting services
    builder.Services.AddScoped<IAutoCheck, AutoCheck>();
    builder.Services.AddScoped<ICheckersService, CheckersService>();
    builder.Services.AddScoped<IBasicAuthenticationProvider>(sp =>
        new BasicAuthenticationProvider(sp.GetRequiredService<IUserRepository>()));
    builder.Services.AddScoped<ITwoFactorAuthenticationService, TwoFactorAuthenticationService>();


    builder.Services.AddScoped<ILinkedInLocalOAuthServer>(sp =>
    new LinkedInLocalOAuthServer("http://localhost:8891/"));

    builder.Services.AddScoped<IGitHubLocalOAuthServer>(sp =>
        new GitHubLocalOAuthServer("http://localhost:8890/"));

    builder.Services.AddScoped<IFacebookLocalOAuthServer>(sp =>
        new FacebookLocalOAuthServer("http://localhost:8888/"));

}