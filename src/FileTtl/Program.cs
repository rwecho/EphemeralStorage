using FileTtl.BackgroundJobs;
using FileTtl.Controllers;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using Quartz;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.WebHost.UseKestrel(options =>
{
    options.Limits.MaxConcurrentConnections = 100;
    options.Limits.MaxConcurrentUpgradedConnections = 100;
    options.Limits.MaxRequestBodySize = 1024 * 1024 * 1024; // 1GB
});

// Add services to the container.
builder.Services.AddRazorPages();

// configure options.
var configuration = builder.Configuration;
builder.Services.Configure<FormOptions>(configuration.GetSection("Form"));

// configure FileStorage options
builder.Services.Configure<FileStorageOptions>(configuration.GetSection("FileStorage"));

// configure controllers
builder.Services.AddControllers();
#if DEBUG
    builder.Services.AddSwaggerGen();
#endif

// confgure quartz
builder.Services.AddQuartz(quartz =>
{
    quartz.UseMicrosoftDependencyInjectionJobFactory();

    var jobKey = new JobKey(nameof(CleanupExpiredFilesJob));
    quartz.AddJob<CleanupExpiredFilesJob>(jobKey);

    quartz.AddTrigger(options =>
    {
        options.ForJob(jobKey)
        .WithIdentity($"{nameof(CleanupExpiredFilesJob)}-trigger")
        .WithCronSchedule("0 * * ? * *");
    });
});
builder.Services.AddQuartzServer(options =>
{
    // when shutting down we want jobs to complete gracefully
    options.WaitForJobsToComplete = true;
});

// inject services.
builder.Services.AddSingleton<FileItemStorage>();


var app = builder.Build();

var form = app.Services.GetRequiredService<IOptions<FormOptions>>();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.MapControllerRoute("default", "api/{controller=Home}/{action=Index}/{id?}");

app.Run();
