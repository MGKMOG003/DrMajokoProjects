using DrMajokoProjects.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Register API Service
builder.Services.AddHttpClient<IApiService, ApiService>();

// Register Performance Manager for Contractor Dashboard
builder.Services.AddScoped<IPerformanceManager, PerformanceManager>();

// Add distributed memory cache for session (required for authentication)
builder.Services.AddDistributedMemoryCache();

// Add session support for authentication
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);  // Extended to 2 hours
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".DrMajokoProjects.Session";
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;  // Enforce HTTPS
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// IMPORTANT: Session must be enabled BEFORE authorization
app.UseSession();

app.UseAuthorization();

// Changed default route to login page
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();