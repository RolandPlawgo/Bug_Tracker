using Bug_Tracker.Authorization;
using Bug_Tracker.Authorization.AuthorizationHandlers;
using Bug_Tracker.Data;
using Bug_Tracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAuthorization(options => 
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .Build());

builder.Services.AddControllersWithViews();

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped(typeof(UserManager<IdentityUser>));

builder.Services.AddScoped<IAuthorizationHandler, TicketIsOwnerAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, TicketAdminAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, TicketAccessToReadAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, CommentIsOwnerAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, CommentAdminAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ProjectManagerAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, ProjectAdminAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, ProjectAccessToReadAuthorizationHandler>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

    Roles roles = new Roles(roleManager);
    await roles.EnsureRoleExistsAsync(Constants.AdministratorsRole);
    await roles.EnsureRoleExistsAsync(Constants.ManagersRole);

    InitialAccounts initialAccounts = new InitialAccounts(userManager, roleManager);
    string adminPw = builder.Configuration.GetValue<string>("adminPw");
    await initialAccounts.EnsureAccountExistsAsync(adminPw, "admin@gmail.com", Constants.AdministratorsRole);
}

app.Run();
