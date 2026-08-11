using System.Globalization;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<DineFlowRestaurantSystem.Services.UserService>();
builder.Services.AddScoped<DineFlowRestaurantSystem.Services.AuthService>();
builder.Services.AddScoped<DineFlowRestaurantSystem.Services.MenuService>();
builder.Services.AddScoped<DineFlowRestaurantSystem.Services.OrderService>();
builder.Services.AddScoped<DineFlowRestaurantSystem.Services.FeedbackService>();
builder.Services.AddScoped<DineFlowRestaurantSystem.Services.IngredientService>();

var app = builder.Build();
var malaysiaCulture = new CultureInfo("en-MY");

CultureInfo.DefaultThreadCurrentCulture = malaysiaCulture;
CultureInfo.DefaultThreadCurrentUICulture = malaysiaCulture;

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(malaysiaCulture),
    SupportedCultures = new[] { malaysiaCulture },
    SupportedUICultures = new[] { malaysiaCulture }
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.UseSession();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}")
    .WithStaticAssets();


app.Run();
