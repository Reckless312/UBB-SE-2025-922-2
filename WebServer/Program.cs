using DataAccess.AuthProviders.Facebook;
using DataAccess.AuthProviders.Github;
using DataAccess.AuthProviders.LinkedIn;
using DataAccess.AuthProviders;
using DataAccess.AutoChecker;
using DataAccess.Service;
using DataAccess.Service.AdminDashboard;
using DataAccess.Service.AdminDashboard.Interfaces;
using DataAccess.Service.Authentication.Interfaces;
using DataAccess.Service.Authentication;
using IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Repository.AdminDashboard;
using Repository.Authentication;
using ServerAPI.Data;
using DrinkDb_Auth.AuthProviders.Google;
using DrinkDb_Auth.Service.AdminDashboard.Interfaces;
using DrinkDb_Auth.Service.AdminDashboard;
using DrinkDb_Auth.Service.Authentication;
using DataAccess.Repository.AdminDashboard;
using DrinkDb_Auth.Service.Authentication.Components;
using DataAccess.AuthProviders.Twitter;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<DatabaseContext>();
builder.Services.AddControllersWithViews();

DependencyInjection(builder);

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseDeveloperExceptionPage();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=MainWindow}/{id?}");
app.MapRazorPages();

app.Run();

static void DependencyInjection(WebApplicationBuilder builder)
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    builder.Services.AddDbContextFactory<DatabaseContext>(options => options.UseSqlServer(connectionString));
    builder.Services.AddScoped<DatabaseContext>(sp => sp.GetRequiredService<IDbContextFactory<DatabaseContext>>().CreateDbContext());

    builder.Services.AddScoped<ISessionRepository, SessionRepository>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IReviewsRepository, ReviewsRepository>();
    builder.Services.AddScoped<IOffensiveWordsRepository, OffensiveWordsRepository>();
    builder.Services.AddScoped<IUpgradeRequestsRepository, UpgradeRequestsRepository>();
    builder.Services.AddScoped<IRolesRepository, RolesRepository>();

    builder.Services.AddScoped<ISessionService, SessionService>();
    builder.Services.AddScoped<IUserService, UserService>();

    builder.Services.AddScoped<IBasicAuthenticationProvider>(sp => new BasicAuthenticationProvider(sp.GetRequiredService<IUserService>()));

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

    builder.Services.AddScoped<ITwoFactorAuthenticationService, TwoFactorAuthenticationService>(sp => new TwoFactorAuthenticationService(
        sp.GetRequiredService<IUserRepository>()));

    builder.Services.AddSingleton<LinkedInLocalOAuthServer>(sp =>
        new LinkedInLocalOAuthServer("http://localhost:8891/"));
    builder.Services.AddSingleton<GitHubLocalOAuthServer>(sp =>
        new GitHubLocalOAuthServer("http://localhost:8890/"));
    builder.Services.AddSingleton<FacebookLocalOAuthServer>(sp =>
        new FacebookLocalOAuthServer("http://localhost:8888/"));

    builder.Services.AddSingleton<ILinkedInLocalOAuthServer>(sp =>
        sp.GetRequiredService<LinkedInLocalOAuthServer>());
    builder.Services.AddSingleton<IGitHubLocalOAuthServer>(sp =>
        sp.GetRequiredService<GitHubLocalOAuthServer>());
    builder.Services.AddScoped<FacebookOAuth2Provider>(sp =>
                        new FacebookOAuth2Provider(
                            sp.GetRequiredService<ISessionService>(),
                            sp.GetRequiredService<IUserService>()
                            ));
    builder.Services.AddSingleton<IFacebookLocalOAuthServer>(sp =>
        sp.GetRequiredService<FacebookLocalOAuthServer>());

    builder.Services.AddScoped<IGitHubHttpHelper, GitHubHttpHelper>();
    builder.Services.AddScoped<GitHubOAuth2Provider>(sp =>
        new GitHubOAuth2Provider(
            sp.GetRequiredService<IUserService>(),
            sp.GetRequiredService<ISessionService>()
        ));
    builder.Services.AddScoped<IGitHubOAuthHelper>(sp =>
        new GitHubOAuthHelper(
            sp.GetRequiredService<GitHubOAuth2Provider>(),
            sp.GetRequiredService<GitHubLocalOAuthServer>()
        ));
    builder.Services.AddScoped<IGoogleOAuth2Provider, GoogleOAuth2Provider>();
    builder.Services.AddScoped<IFacebookOAuthHelper, FacebookOAuthHelper>();

    builder.Services.AddScoped<LinkedInOAuth2Provider>();
    builder.Services.AddScoped<ILinkedInOAuthHelper>(sp =>
        new LinkedInOAuthHelper(
            "86j0ikb93jm78x",
            "WPL_AP1.pg2Bd1XhCi821VTG.+hatTA==",
            "http://localhost:8891/auth",
            "openid profile email",
            sp.GetRequiredService<LinkedInOAuth2Provider>()
        ));

    builder.Services.AddScoped<IAutoCheck, AutoCheck>();
    builder.Services.AddScoped<ICheckersService, CheckersService>();

    builder.Services.AddScoped<IVerify, Verify2FactorAuthenticationSecret>();
    builder.Services.AddScoped<ITwitterOAuth2Provider, TwitterOAuth2Provider>(sp =>
        new TwitterOAuth2Provider(
            sp.GetRequiredService<IUserService>(),
            sp.GetRequiredService<ISessionService>()));
}
