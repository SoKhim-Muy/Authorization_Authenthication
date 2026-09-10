using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyCodeFirstProject.Data;
using MyCodeFirstProject.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Register DbContext & Identity Services
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

// 2. Configure Dual Authentication (Cookie for MVC + JWT Bearer for REST Web API)
builder.Services.ConfigureApplicationCookie(opt => opt.LoginPath = "/Account/Login");
builder.Services.AddAuthentication()
    .AddJwtBearer(opt => {
        opt.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"], ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddScoped<IAppUserRepository, AppUserRepository>();
builder.Services.AddControllersWithViews(); 
builder.Services.AddEndpointsApiExplorer(); 
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseHttpsRedirection(); app.UseStaticFiles(); app.UseRouting();
app.UseAuthentication(); app.UseAuthorization();

// Data Seeding (Automatic Admin Account Initialization)
using (var scope = app.Services.CreateScope()) {
    var um = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var rm = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    if (!await rm.RoleExistsAsync("Admin")) await rm.CreateAsync(new IdentityRole("Admin"));
    if (await um.FindByEmailAsync("admin@example.com") == null) {
        var admin = new IdentityUser { UserName = "admin@example.com", Email = "admin@example.com", EmailConfirmed = true };
        if ((await um.CreateAsync(admin, "Admin123!")).Succeeded) await um.AddToRoleAsync(admin, "Admin");
    }
}
app.MapControllerRoute(name: "default", pattern: "{controller=AppUser}/{action=Index}/{id?}");
app.Run();