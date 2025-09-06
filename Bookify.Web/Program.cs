using Bookify.Web.Core.Mapping;
using Bookify.Web.Seeds;
using Microsoft.AspNetCore.Identity;
using System.Reflection;
using UoN.ExpressiveAnnotations.NetCore.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Bookify.Web.Data;
using Bookify.Web.Helpers;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.DataProtection;
using WhatsAppCloudApi.Extensions;
using Hangfire;
using Hangfire.Dashboard;
using Bookify.Web.Tasks;
using HashidsNet;
using ViewToHTML.Extensions;
using Serilog;
using Serilog.Context;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));


builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders()
    .AddSignInManager<SignInManager<ApplicationUser>>();

builder.Services.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.Zero);

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequiredLength = 8;

    options.User.RequireUniqueEmail = true;
});

builder.Services.AddDataProtection().SetApplicationName(nameof(Bookify));
builder.Services.AddSingleton<IHashids>(_ => new Hashids("f1nd1ngn3m0" , minHashLength: 11));

builder.Services.AddTransient<IImageService, ImageService>();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddTransient<IEmailBodyBuilder, EmailBodyBuilder>();

builder.Services.AddWhatsAppApiClient(builder.Configuration);

builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationUserClaimsPrincipalFactory>();

builder.Services.AddControllersWithViews();

builder.Services.AddAutoMapper(Assembly.GetAssembly(typeof(MappingProfile)));

builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection(nameof(CloudinarySettings)));
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection(nameof(MailSettings)));

builder.Services.AddExpressiveAnnotations();

builder.Services.AddHangfire(x => x.UseSqlServerStorage(connectionString));
builder.Services.AddHangfireServer();

builder.Services.Configure<AuthorizationOptions>(options =>
options.AddPolicy("AdminsOnly", policy =>
{
    policy.RequireAuthenticatedUser();
    policy.RequireRole(AppRoles.Admin);
}));

builder.Services.AddViewToHTML();

builder.Services.AddMvc(options => 
   options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute())
);

// Add Serilog
Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
builder.Host.UseSerilog();

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

app.UseExceptionHandler("/Home/Error");


//app.UseStatusCodePages(async statusCodeContext =>
//{
//	// using static System.Net.Mime.MediaTypeNames;
//	statusCodeContext.HttpContext.Response.ContentType = System.Net.Mime.MediaTypeNames.Text.Plain;

//	await statusCodeContext.HttpContext.Response.WriteAsync(
//		$"Status Code Page: {statusCodeContext.HttpContext.Response.StatusCode}");
//});

//app.UseStatusCodePagesWithRedirects("/Home/Error/{0}");
app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCookiePolicy(new CookiePolicyOptions
{
    Secure = CookieSecurePolicy.Always
});

app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Frame-Options", "Deny");

    await next();
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();

using var scope = scopeFactory.CreateScope();

var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

await DefaultRoles.SeedRolesAsync(roleManager);
await DefaultUsers.SeedAdminUserAsync(userManager);

//hangfire
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    DashboardTitle = "Dream Dashboard",
    //IsReadOnlyFunc = (DashboardContext context) => true,
    Authorization = new IDashboardAuthorizationFilter[]
    {
        new HangfireAuthorizationFilter("AdminsOnly")
    }
});

var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
var webHostEnvironment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
var whatsAppClient = scope.ServiceProvider.GetRequiredService<IWhatsAppClient>();
var emailBodyBuilder = scope.ServiceProvider.GetRequiredService<IEmailBodyBuilder>();
var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

var hangfireTasks = new HangfireTasks(dbContext, webHostEnvironment, whatsAppClient,
    emailBodyBuilder, emailSender);

RecurringJob.AddOrUpdate(() => hangfireTasks.PrepareExpirationAlert(), "0 14 * * *");
RecurringJob.AddOrUpdate(() => hangfireTasks.RentalsExpirationAlert(), "0 14 * * *");

app.Use(async (context, next) =>
{
    LogContext.PushProperty("UserId", context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
    LogContext.PushProperty("UserName", context.User.FindFirst(ClaimTypes.Name)?.Value);

    await next();
});

app.UseSerilogRequestLogging();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
