using ServerAPI.Data;
using Microsoft.EntityFrameworkCore;
using IRepository;
using Repository.AdminDashboard;
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
using DataAccess.Repository.AdminDashboard;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient("DrinkDbClient", client =>
{
    client.BaseAddress = new Uri("http://localhost:5280/");
});

DependencyInjection(builder);

WebApplication app = builder.Build();

using (IServiceScope scope2 = app.Services.CreateScope())
{
    GitHubLocalOAuthServer gitHubServer = scope2.ServiceProvider.GetRequiredService<GitHubLocalOAuthServer>();
    _ = gitHubServer.StartAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

void DependencyInjection(WebApplicationBuilder builder)
{
    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty;

    builder.Services.AddDbContextFactory<DatabaseContext>(options =>
        options.UseSqlServer(connectionString));

    builder.Services.AddScoped<DatabaseContext>(sp =>
        sp.GetRequiredService<IDbContextFactory<DatabaseContext>>().CreateDbContext());
    builder.Services.AddScoped<ISessionRepository, SessionRepository>();

    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IReviewsRepository, ReviewsRepository>();
    builder.Services.AddScoped<IOffensiveWordsRepository, OffensiveWordsRepository>();
    builder.Services.AddScoped<IUpgradeRequestsRepository, UpgradeRequestsRepository>();
    builder.Services.AddScoped<IRolesRepository, RolesRepository>();
    builder.Services.AddScoped<IOffensiveWordsRepository, OffensiveWordsRepository>();

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

    builder.Services.AddScoped<LinkedInLocalOAuthServer>(sp =>
        new LinkedInLocalOAuthServer("http://localhost:8891/"));
    builder.Services.AddScoped<GitHubLocalOAuthServer>(sp =>
        new GitHubLocalOAuthServer("http://localhost:8890/"));
    builder.Services.AddScoped<FacebookLocalOAuthServer>(sp =>
        new FacebookLocalOAuthServer("http://localhost:8888/"));

    builder.Services.AddScoped<IGitHubHttpHelper, GitHubHttpHelper>();
    builder.Services.AddScoped<GitHubOAuth2Provider>(sp =>
        new GitHubOAuth2Provider(
            sp.GetRequiredService<IUserService>(),
            sp.GetRequiredService<ISessionService>()));
    builder.Services.AddScoped<IGitHubOAuthHelper>(sp =>
        new GitHubOAuthHelper(
            sp.GetRequiredService<GitHubOAuth2Provider>(),
            sp.GetRequiredService<GitHubLocalOAuthServer>()));
    builder.Services.AddScoped<IGoogleOAuth2Provider, GoogleOAuth2Provider>();
    builder.Services.AddScoped<FacebookOAuth2Provider>(sp =>
                        new FacebookOAuth2Provider(
                            sp.GetRequiredService<ISessionService>(),
                            sp.GetRequiredService<IUserService>()));
    builder.Services.AddScoped<IFacebookOAuthHelper, FacebookOAuthHelper>();
    builder.Services.AddScoped<LinkedInOAuth2Provider>(sp =>
        new LinkedInOAuth2Provider(
            sp.GetRequiredService<IUserService>(),
            sp.GetRequiredService<ISessionService>()));
    builder.Services.AddScoped<ILinkedInOAuthHelper>(sp => new LinkedInOAuthHelper(
        "86j0ikb93jm78x",
        "WPL_AP1.pg2Bd1XhCi821VTG.+hatTA==",
        "http://localhost:8891/auth",
        "openid profile email",
        sp.GetRequiredService<LinkedInOAuth2Provider>()));

    builder.Services.AddScoped<IAutoCheck, AutoCheck>(sp => new AutoCheck(sp.GetRequiredService<IOffensiveWordsRepository>()));
    builder.Services.AddScoped<ICheckersService, CheckersService>();
    builder.Services.AddScoped<IBasicAuthenticationProvider>(sp =>
        new BasicAuthenticationProvider(sp.GetRequiredService<IUserService>()));
    builder.Services.AddScoped<ITwoFactorAuthenticationService, TwoFactorAuthenticationService>();

    builder.Services.AddScoped<ILinkedInLocalOAuthServer>(sp =>
    new LinkedInLocalOAuthServer("http://localhost:8891/"));

    builder.Services.AddScoped<IGitHubLocalOAuthServer>(sp =>
        new GitHubLocalOAuthServer("http://localhost:8890/"));

    builder.Services.AddScoped<IFacebookLocalOAuthServer>(sp =>
        new FacebookLocalOAuthServer("http://localhost:8888/"));
}