using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Thrift.Data;
using System;
using Thrift.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Thrift.Repositories;
using Thrift.Models;
using Thrift.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add connection string to configuration
// Make sure your connection string is in appsettings.json
// "ConnectionStrings": { "MongoDbConnection": "mongodb://localhost:27017" }

// Add MongoDB services
builder.Services.AddSingleton<MongoDbContext>();

// Add error handling for MongoDB connection
builder.Services.AddHealthChecks().AddCheck<MongoDbHealthCheck>("mongodb_health_check");

// Add repositories
builder.Services.AddScoped<SavingsGoalRepository>();
builder.Services.AddScoped<SavingsTransactionRepository>();
builder.Services.AddScoped<BudgetRepository>();
builder.Services.AddScoped<ExpenseRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<LoanRepository>();
builder.Services.AddScoped<LoanPaymentRepository>();
builder.Services.AddScoped<AccountBalanceRepository>();
builder.Services.AddScoped<ThriftGroupRepository>();

// Add loan services
builder.Services.AddScoped<LoanCalculationService>();

// Configure Identity with MongoDB
builder.Services.AddScoped<IUserStore<User>, MongoUserStore>();
builder.Services.AddScoped<IRoleStore<IdentityRole>, MongoRoleStore>();
builder.Services.AddIdentity<User, IdentityRole>()
    .AddDefaultTokenProviders();

// Configure Identity options
builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
    
    // Don't require email confirmation for development
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
});

// Configure cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    // Cookie settings
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(5);

    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;
});

// Add services for controllers and views
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Email service
builder.Services.AddTransient<IEmailSender, EmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Enable health checks endpoint
app.MapHealthChecks("/health");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
