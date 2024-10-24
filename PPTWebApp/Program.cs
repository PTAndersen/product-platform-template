using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using PPTWebApp.Components;
using PPTWebApp.Components.Account;
using PPTWebApp.Data;
using PPTWebApp.Data.Models;
using PPTWebApp.Data.Repositories;
using PPTWebApp.Data.Repositories.Interfaces;
using PPTWebApp.Data.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' is either missing or empty.");
}

builder.Services.AddScoped<IUserProfileRepository>(provider => new UserProfileRepository(connectionString));
//builder.Services.AddScoped<UserProfileService>();

builder.Services.AddScoped<IApplicationUserRepository>(provider => new ApplicationUserRepository(connectionString, provider.GetRequiredService<IUserProfileRepository>()));
builder.Services.AddScoped<IUserStore<ApplicationUser>>(provider =>
{
    var userRepository = provider.GetRequiredService<IApplicationUserRepository>();
    return new ApplicationUserService(userRepository);
});
builder.Services.AddScoped<ApplicationUserService>(provider =>
{
    var userRepository = provider.GetRequiredService<IApplicationUserRepository>();
    return new ApplicationUserService(userRepository);
});


builder.Services.AddScoped<IApplicationRoleRepository>(provider => new ApplicationRoleRepository(connectionString));
builder.Services.AddScoped<IRoleStore<IdentityRole>>(provider =>
{
    var roleRepository = provider.GetRequiredService<IApplicationRoleRepository>();
    return new ApplicationRoleService(roleRepository);
});
builder.Services.AddScoped<ApplicationRoleService>(provider =>
{
    var roleRepository = provider.GetRequiredService<IApplicationRoleRepository>();
    return new ApplicationRoleService(roleRepository);
});


builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/account/login";
        options.LogoutPath = "/account/logout";
    });


builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<IPostRepository>(provider => new PostRepository(connectionString));
builder.Services.AddScoped<PostService>();
builder.Services.AddScoped<IProductRepository>(provider => new ProductRepository(connectionString));
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<IProductRepository>(provider => new ProductRepository(connectionString));
builder.Services.AddScoped<IHighlightRepository>(provider => new HighlightRepository(connectionString, provider.GetRequiredService<IProductRepository>()));
builder.Services.AddScoped<HighlightService>();
builder.Services.AddScoped<BasketService>();
builder.Services.AddScoped<IProductCategoryRepository>(provider => new ProductCategoryRepository(connectionString));
builder.Services.AddScoped<ProductCategoryService>();
builder.Services.AddScoped<IProductInventoryRepository>(provider => new ProductInventoryRepository(connectionString));
builder.Services.AddScoped<ProductInventoryService>();
builder.Services.AddScoped<IDiscountRepository>(provider => new DiscountRepository(connectionString));
builder.Services.AddScoped<DiscountService>();
builder.Services.AddScoped<IVisitorSessionRepository>(provider => new VisitorSessionRepository(connectionString));
builder.Services.AddScoped<VisitorSessionService>();
builder.Services.AddScoped<IVisitorPageViewRepository>(provider => new VisitorPageViewRepository(connectionString));
builder.Services.AddScoped<VisitorPageViewService>();
builder.Services.AddScoped<IUserActivityRepository>(provider => new UserActivityRepository(connectionString));
builder.Services.AddScoped<UserActivityService>();
builder.Services.AddScoped<IOrderRepository>(provider => new OrderRepository(connectionString));
builder.Services.AddScoped<OrderService>();

builder.WebHost.UseStaticWebAssets();

builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddRoles<IdentityRole>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    var dataInitializer = new DataInitializer(connectionString, userManager, roleManager);
    await dataInitializer.InitializeDataAsync();
}

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints();

app.Run();
