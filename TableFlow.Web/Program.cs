using FluentValidation;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using MudBlazor.Services;
using TableFlow.Api.Data;
using TableFlow.Api.Data.Entities;
using TableFlow.Web.Auth;
using TableFlow.Web.Components;
using TableFlow.Web.Models;
using TableFlow.Web.Services;
using TableFlow.Web.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass        = Defaults.Classes.Position.TopLeft;
    config.SnackbarConfiguration.VisibleStateDuration = 3000;
    config.SnackbarConfiguration.HideTransitionDuration = 200;
    config.SnackbarConfiguration.ShowTransitionDuration = 200;
    config.SnackbarConfiguration.SnackbarVariant      = Variant.Filled;
});

//builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
//builder.Services.AddDbContextFactory<AppDbContext>(options =>
//    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")),
    ServiceLifetime.Scoped);

//builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
//{
//    options.Password.RequireDigit = false;
//    options.Password.RequiredLength = 6;
//    options.Password.RequireNonAlphanumeric = false;
//    options.Password.RequireUppercase = false;

//})
//    .AddEntityFrameworkStores<AppDbContext>()
//    .AddDefaultTokenProviders();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
}).AddRoles<IdentityRole>().AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultScheme = "NoOp";
//    options.DefaultChallengeScheme = "NoOp";
//})
//.AddCookie("NoOp", options =>
//{
//    options.LoginPath = "/login";
//    options.AccessDeniedPath = "/login";
//});

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddDataProtection();

builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

builder.Services.AddHttpClient("TableFlowApi", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5009");
});

builder.Services.AddSingleton<IValidator<LoginModel>, LoginValidator>();

builder.Services.AddSingleton<AppUpdateService>();

builder.Services.AddScoped<UnauthorizedNotifier>();

builder.Services.AddScoped<UserApiService>();

builder.Services.AddScoped<MenuApiService>();

builder.Services.AddScoped<TableApiService>();

builder.Services.AddScoped<CartService>();

builder.Services.AddScoped<OrderApiService>();

builder.Services.AddScoped<SessionApiService>();

builder.Services.AddScoped<UserFormValidator>();

var app = builder.Build();

// Trust reverse proxy headers (Railway, nginx, etc.) so HTTPS redirect works correctly
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
// app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

//app.UseAuthentication();
//app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode().AllowAnonymous();

app.Run();
