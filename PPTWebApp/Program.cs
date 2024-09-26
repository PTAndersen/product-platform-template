using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using PPTWebApp.Components;
using PPTWebApp.Components.Account;
using PPTWebApp.Data;
using PPTWebApp.Data.Repositories;
using PPTWebApp.Data.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddScoped<IUserStore<ApplicationUser>>(provider => new CustomUserStore(connectionString));
builder.Services.AddScoped<IRoleStore<IdentityRole>>(provider => new CustomRoleStore(connectionString));

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
builder.Services.AddScoped<BasketService>();


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
