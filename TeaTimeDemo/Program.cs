using Microsoft.EntityFrameworkCore;
using TeaTimeDemo.DataAccess.Data;
using TeaTimeDemo.DataAccess.Repository.IRepository;
using TeaTimeDemo.DataAccess.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using TeaTimeDemo.Utility;
using TeaTimeDemo.DataAccess.DbInitializer;

// 建立 WebApplicationBuilder 實例，用於配置和構建應用程式
var builder = WebApplication.CreateBuilder(args);

// 加入 MVC 控制器和視圖到依賴注入容器
builder.Services.AddControllersWithViews();

// 設定資料庫上下文，使用 SQL Server 作為資料庫提供者
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 設定 ASP.NET Core Identity 使用 IdentityUser 和 IdentityRole
// 並要求使用者在註冊時需要確認帳號
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// 配置應用程式的 Cookie 認證路徑
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});

// 將 IDbInitializer 接口與 DbInitializer 實現綁定，設為 Scoped 範圍
builder.Services.AddScoped<IDbInitializer, DbInitializer>();

// 加入 Razor Pages 支援
builder.Services.AddRazorPages();

// 將 IUnitOfWork 接口與 UnitOfWork 實現綁定，設為 Scoped 範圍
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// 將 IEmailSender 接口綁定到 EmailSender 實現
builder.Services.AddScoped<IEmailSender, EmailSender>();

// 建構 Web 應用程式
var app = builder.Build();

// 配置 HTTP 請求管道
// 如果不是開發環境，則啟用錯誤處理頁面和 HSTS
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// 啟用 HTTPS 強制重導
app.UseHttpsRedirection();

// 啟用靜態文件支援
app.UseStaticFiles();

// 啟用路由功能
app.UseRouting();

// 執行資料庫種子方法來初始化資料庫
SeedDatabase();

// 啟用身份驗證
app.UseAuthentication();

// 啟用授權
app.UseAuthorization();

// 映射 Razor Pages
app.MapRazorPages();

// 配置預設的控制器路由
app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

// 啟動應用程式並開始接收 HTTP 請求
app.Run();

// 資料庫種子方法，用於在應用啟動時初始化資料庫
void SeedDatabase()
{
    using (var scope = app.Services.CreateScope())
    {
        var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        dbInitializer.Initialize();
    }
}
