using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using SessionStoreSample.Data;
using SessionStoreSample.Session;
using SessionStoreSample.Session.Memory;
using SessionStoreSample.Session.DistributedCache;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

ConfigureSessionStore(builder);

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

app.Run();



void ConfigureSessionStore(WebApplicationBuilder builder)
{
    builder.Services.AddSingleton<SessionSetting>();

    ConfigureMemorySessionStore(builder);
    //ConfigureRedisSessionStore(builder);
}
void ConfigureMemorySessionStore(WebApplicationBuilder builder)
{
    builder.Services.AddMemoryCache();
    builder.Services.AddSingleton<ITicketStore, MemorySessionStore>();
    builder.Services.AddOptions<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme)
            .Configure<ITicketStore, SessionSetting>((options, store, setting) =>
            {
                options.SessionStore = store;
                options.ExpireTimeSpan = setting.ExpireTime;
            });
    builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
            .AddCookie();

}
void ConfigureRedisSessionStore(WebApplicationBuilder builder)
{
    var config = builder.Configuration;
    builder.Services.AddStackExchangeRedisCache(redisCacheConfig =>
    {
        redisCacheConfig.ConfigurationOptions = ConfigurationOptions.Parse(config.GetConnectionString("RedisConnection"));
    });
    builder.Services.AddSingleton<IDistributedCache, RedisCache>();
    builder.Services.AddSingleton<ITicketStore, DistributedCacheSessionStore>();
    builder.Services.AddOptions<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme)
        .Configure<ITicketStore, SessionSetting> ((options, store, setting) =>
        {
            options.SessionStore = store;
            options.ExpireTimeSpan = setting.ExpireTime;
        });
    builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
        .AddCookie();
}