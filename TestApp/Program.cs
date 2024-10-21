using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using TestApp.Data;
using TestApp.Services;
using BGProcess.Services;
using BGProcess.Interface;
using SendGrid;
using SendGrid.Helpers.Mail;




var builder = WebApplication.CreateBuilder(args);

    // Configuration for SendGrid
    var sendGridApiKey = builder.Configuration["SendGrid:ApiKey"];

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    builder.Services.AddSingleton<SendMail>();
    builder.Services.AddSingleton<IEmailQueue, EmailQueueService>();
    builder.Services.AddSingleton<ISendGridClient>(new SendGridClient(sendGridApiKey));
    builder.Services.AddControllersWithViews();
    builder.Services.AddScoped<UserService>();

    builder.Services.AddSingleton<IEmailQueue, EmailQueueService>();
    builder.Services.AddSingleton<EmailQueueService>();

   // builder.Services.AddSingleton<EmailService>();

   // builder.Services.AddHostedService<EmailService>();


    var app = builder.Build();

    // Middleware
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
    app.UseAuthorization();

    // Map default route
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=User}/{action=Index}/{id?}");

    app.Run();
