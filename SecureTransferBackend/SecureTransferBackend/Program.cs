using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using SecureTransferBackend.Data;
using SecureTransferBackend.Services.Auth.Models;
using SecureTransferBackend.Services.Keys;
using SecureTransferBackend.Services.Transfers;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 524_288_000;
});

// Add services to the container.
var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION") ??
                       builder.Configuration.GetConnectionString("PostgresConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "myOriginsPolicy",
        policy  =>
        {
            policy.WithOrigins("http://localhost:3000");
            policy.AllowCredentials();
            policy.WithHeaders(HeaderNames.ContentType, "Cookie", "X-CSRF-TOKEN");
        });
});
builder.Services.AddTransient<ITransferService, TransferService>();
builder.Services.AddTransient<IKeysService, KeysService>();
builder.Services.Configure<FormOptions>(options =>
{
    // 500 MB
    options.MultipartBodyLengthLimit = 524288000;
});
// builder.Services.AddControllersWithViews(options =>
// {
//     options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
// });
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.HttpOnly = false;
    options.HeaderName = "X-CSRF-TOKEN";
});
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401;    
        return Task.CompletedTask;
    };
});

var app = builder.Build();

app.UseCors("myOriginsPolicy");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpLogging();
app.UseStaticFiles(new StaticFileOptions()
{
    DefaultContentType = "text/plain",
    ServeUnknownFileTypes = true
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();
