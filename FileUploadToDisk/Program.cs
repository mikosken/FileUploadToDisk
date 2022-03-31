using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FileUploadToDisk.Data;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddDbContext<FileUploadToDiskContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("FileUploadToDiskContext")));
builder.Services.AddDbContext<FileUploadToDiskContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("FileUploadToDiskContextSqlite")));


// Add services to the container.
builder.Services.AddControllersWithViews();

// Increase MultipartBodyLengthLimit for handling streaming of large files, default is 128 MB.
// Alternatively, set [RequestFormLimits(MultipartBodyLengthLimit = 268435456)] on affected pages/actions.
//builder.Services.Configure<FormOptions>(options =>
//{
//    // Set the limit to 256 MB
//    options.MultipartBodyLengthLimit = 268435456;
//});

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
    pattern: "{controller=Files}/{action=Index}/{id?}");

app.Run();
