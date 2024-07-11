using Lamar.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RhDev.Common.Web.Core.Configuration;
using RhDev.Common.Web.Core.Utils;
using RhDev.Customer.Component.Core.Impl.Data;
using RhDev.Customer.Component.Core.Impl;
using RhDev.Common.Web.Core.Impl.DependencyInjection;
using RhDev.Common.Web.Core.Impl.Host;
using RhDev.Common.Web.Core.Security;
using RhDev.Customer.Component.App;
using RhDev.Customer.Component.App.Data;
using RhDev.Customer.Component.App.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureAppConfiguration(b =>
{
    b.ConfigureConfiguration(new[] { Constants.SOLUTION_PREF });
});

builder.Host.UseLamar((ctx, registry) =>
{
    registry.AddHttpContextAccessor();

    registry
    .AddCommon<ApplicationDbContext>(
        ctx,
        new[] { TestCompositionDefinition.GetDefinition() },
        useLazyLoadingProxies: false,
        useDbContextFactory: true,
        b: b => b.UseQueueHostedService(),
        databaseSaveChangeInterceptorsTypes: typeof(AuditableEntityInterceptor));

    registry
        .AddOptions<ApplicationConfiguration>()
        .BindConfiguration("App");

    registry.AddDatabaseDeveloperPageExceptionFilter();

    registry.AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.Password = IdentitySettings.PasswordOptions;
        options.Lockout.AllowedForNewUsers = true;
        options.SignIn.RequireConfirmedAccount = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;
        options.SignIn.RequireConfirmedEmail = false;

    })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

    registry.AddRazorPages()
        .AddViewLocalization();

    registry.AddServerSideBlazor();
    registry.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<ApplicationUser>>();

    registry.ConfigureApplicationCookie(options =>
    {
        var config = ctx.Configuration.GetSection(ConfigurationUtils.GetPathConfigurationProperty<CommonConfiguration>(a => a.Identity.CookieExpirationInMinutes!))?.Value;

        if (!string.IsNullOrWhiteSpace(config) && int.TryParse(config, out int expiration) && expiration > 0)
        {
            options.ExpireTimeSpan = TimeSpan.FromMinutes(expiration);
        }
    });

    registry.AddLocalization(options =>
    {
        options.ResourcesPath = "Resources";
    });
});

builder.Host.AddSerilog(fileNameTemplate: "c:\\logs\\test\\log-.txt", useSql: true, sqlBatchLimit: 1, appIdentifier: "AppX");

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();

    var db = app.Services.GetRequiredService<ApplicationDbContext>();

    //await db.Database.EnsureCreatedAsync();

    db.Database.Migrate();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();

app.MapRazorPages();

app.MapFallbackToPage("/_Host");

app.Run();
