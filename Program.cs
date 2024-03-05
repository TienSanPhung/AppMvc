using AppMvc.Data;
using AppMvc.Models;
using AppMvc.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// chuyển cổng sang https
builder.Services.AddHttpsRedirection(options=>{
    options.HttpsPort = 443;
});
// add dịch vụ mail
builder.Services.AddOptions (); // Kích hoạt Options
var mailsettings = builder.Configuration.GetSection ("MailSettings"); // đọc config
builder.Services.Configure<MailSettings> (mailsettings); // đăng ký để Inject
 builder.Services.AddTransient<IEmailSender, SendMailService> (); // Đăng ký dịch vụ Mail

// add DbContext
builder.Services.AddDbContext<AppDbContext>(options=>{
    var connectionString = builder.Configuration.GetConnectionString("AppMvc");
    options.UseSqlServer(connectionString);
});
// add dichj vuj identity
builder.Services.AddIdentity<AppUser, IdentityRole> ()
    .AddEntityFrameworkStores<AppDbContext> ()
    .AddDefaultTokenProviders ();

// add dịch vụ thông báo describer
builder.Services.AddSingleton<IdentityErrorDescriber, AppIdentityErrorDescriber>();
// add dịch vụ đăng nhập bằng gg và fb
builder.Services.AddAuthentication()
    .AddGoogle(options=>{
        IConfigurationSection gconfig = builder.Configuration.GetSection("Authentication:google");
        options.ClientId = gconfig["client_id"];
        options.ClientSecret = gconfig["client_secret"];
        options.CallbackPath = "/dang-nhap-tu-google";

    })
    .AddFacebook(options=>{
        IConfigurationSection fconfig = builder.Configuration.GetSection("Authentication:facebook");
        options.ClientId = fconfig["client_id"];
        options.ClientSecret = fconfig["client_secret"];
        options.CallbackPath = "/dang-nhap-tu-facebook";

    });
// thiết lập hiển thị cho menu manager
builder.Services.AddAuthorization(options=>{
    options.AddPolicy("ViewManageMenu", appbuiler =>{
        appbuiler.RequireAuthenticatedUser();
        appbuiler.RequireRole(RoleName.Administrator);
    });
});
// add thiết lập cho identity
builder.Services.Configure<IdentityOptions> (options => {
    // Thiết lập về Password
    options.Password.RequireDigit = false; // Không bắt phải có số
    options.Password.RequireLowercase = false; // Không bắt phải có chữ thường
    options.Password.RequireNonAlphanumeric = false; // Không bắt ký tự đặc biệt
    options.Password.RequireUppercase = false; // Không bắt buộc chữ in
    options.Password.RequiredLength = 3; // Số ký tự tối thiểu của password
    options.Password.RequiredUniqueChars = 1; // Số ký tự riêng biệt

    // Cấu hình Lockout - khóa user
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes (5); // Khóa 5 phút
    options.Lockout.MaxFailedAccessAttempts = 5; // Thất bại 5 lầ thì khóa
    options.Lockout.AllowedForNewUsers = true;

    // Cấu hình về User.
    options.User.AllowedUserNameCharacters = // các ký tự đặt tên user
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true; // Email là duy nhất

    // Cấu hình đăng nhập.
    options.SignIn.RequireConfirmedEmail = true; // Cấu hình xác thực địa chỉ email (email phải tồn tại)
    options.SignIn.RequireConfirmedPhoneNumber = false; // Xác thực số điện thoại

});



// Cấu hình Cookie
builder.Services.ConfigureApplicationCookie (options => {
    // options.Cookie.HttpOnly = true;  
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);  
    options.LoginPath = $"/Areas/Identity/Pages/Account/Login.cshtml";                                 // Url đến trang đăng nhập
    options.LogoutPath = $"/logout/";   
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";   // Trang khi User bị cấm truy cập
});
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    // Trên 5 giây truy cập lại sẽ nạp lại thông tin User (Role)
    // SecurityStamp trong bảng User đổi -> nạp lại thông tinn Security
    options.ValidationInterval = TimeSpan.FromSeconds(5); 
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

app.UseAuthentication();
app.UseAuthorization();



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
