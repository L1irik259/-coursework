using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using FranchiseAgregator.Models;
using FranchiseAgregator.Services;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        
        options.Cookie.Name = ".AspNetCore.FranchiseAuth";

        
        options.Cookie.Path = "/";

        
        options.Cookie.SecurePolicy = CookieSecurePolicy.None;

        
        options.Cookie.SameSite = SameSiteMode.Lax;

        
        options.ExpireTimeSpan = TimeSpan.FromHours(24);

        
        options.SlidingExpiration = true;

        
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = false;
    });


builder.Services.AddAuthorization();


builder.Services.AddControllersWithViews();
builder.Services.AddScoped<PdfOrderService>();

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
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

app.Run(); 