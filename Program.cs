using AppMvc.Models;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// chuyển cổng sang https
builder.Services.AddHttpsRedirection(options=>{
    options.HttpsPort = 443;
});

// add DbContext
builder.Services.AddDbContext<AppDbContext>(options=>{
    var connectionString = builder.Configuration.GetConnectionString("AppMvc");
    options.UseSqlServer(connectionString);
});

// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
// builder.Services.AddRazorPages();
// cấu hình asp tìm kiếm view
// builder.Services.Configure<RazorViewEngineOptions>(options=>{

    
//     options.AreaViewLocationFormats.Add("/Areas/{2}/Views/{1}/{0}.cshtml");
//     options.AreaViewLocationFormats.Add("/Areas/{2}/Views/Shared/{0}.cshtml");
//     options.AreaViewLocationFormats.Add("/Views/Shared/{0}.cshtml");
// });

// thêm cài đặt cho route
builder.Services.Configure<RouteOptions> (options => {
    options.AppendTrailingSlash = false;        // Thêm dấu / vào cuối URL
    options.LowercaseUrls = true;               // url chữ thường
    options.LowercaseQueryStrings = false;      // không bắt query trong url phải in thường
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
