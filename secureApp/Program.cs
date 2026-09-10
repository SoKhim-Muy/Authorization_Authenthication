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
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// 2. Enable CORS Policy (Crucial for JS Single Page App)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 3. Configure Dual Authentication Scheme (Cookie + JWT Bearer)
builder.Services.ConfigureApplicationCookie(opt => opt.LoginPath = "/Account/Login");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opt => {
    opt.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

// 4. Register Custom Repositories & Controller Services
builder.Services.AddScoped<IAppUserRepository, AppUserRepository>();
builder.Services.AddControllersWithViews(); 
builder.Services.AddEndpointsApiExplorer(); 
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 5. Configure HTTP Request Pipeline
if (app.Environment.IsDevelopment()) 
{ 
    app.UseSwagger(); 
    app.UseSwaggerUI(); 
}

app.UseHttpsRedirection(); 
app.UseStaticFiles(); 
app.UseRouting();

// CORS MUST be placed after UseRouting and before UseAuthentication/UseAuthorization
app.UseCors("AllowAll");

app.UseAuthentication(); 
app.UseAuthorization();

// 6. Automatic Identity Data Seeding
using (var scope = app.Services.CreateScope()) 
{
    var um = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var rm = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    
    if (!await rm.RoleExistsAsync("Admin")) await rm.CreateAsync(new IdentityRole("Admin"));
    if (!await rm.RoleExistsAsync("User")) await rm.CreateAsync(new IdentityRole("User"));

    if (await um.FindByEmailAsync("admin@example.com") == null) 
    {
        var admin = new IdentityUser { UserName = "admin@example.com", Email = "admin@example.com", EmailConfirmed = true };
        if ((await um.CreateAsync(admin, "Admin123!")).Succeeded) await um.AddToRoleAsync(admin, "Admin");
    }

    if (await um.FindByEmailAsync("khim@example.com") == null) 
    {
        var khim = new IdentityUser { UserName = "khim@example.com", Email = "khim@example.com", EmailConfirmed = true };
        if ((await um.CreateAsync(khim, "Khim123!")).Succeeded) await um.AddToRoleAsync(khim, "User");
    }
}

app.MapControllerRoute(name: "default", pattern: "{controller=AppUser}/{action=Index}/{id?}");

app.Run();