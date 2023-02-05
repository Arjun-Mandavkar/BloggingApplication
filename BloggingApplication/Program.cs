using BloggingApplication.DbConnection;
using BloggingApplication.DbConnection.Implementations;
using BloggingApplication.Models;
using BloggingApplication.Repositories;
using BloggingApplication.Repositories.Implementations;
using BloggingApplication.Services;
using BloggingApplication.Services.Implementations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

namespace BloggingApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //Add logging service to container
            Log.Logger = new LoggerConfiguration()
                //.WriteTo.File(@"C:\Users\ArjunMandavkar\Desktop\DotNet_Practice\WebApps\WebApps\BloggingApplication\BloggingApplication\BloggingApplication\logs.json")
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            Log.Information("Hello world");

            builder.Logging.AddSerilog();

            // Add services to the container.
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(
                                builder.Configuration.GetSection("Jwt:Secret").Value
                                )
                            ),
                        //https://stackoverflow.com/questions/70597009/what-is-the-meaning-of-validateissuer-and-validateaudience-in-jwt
                        //https://stackoverflow.com/questions/61976960/asp-net-core-jwt-authentication-always-throwing-401-unauthorized
                        ValidateIssuer = true,
                        ValidIssuer = builder.Configuration.GetSection("AuthSettings:Issuer").Value,
                        ValidateAudience = false,
                        ValidAudience = builder.Configuration.GetSection("AuthSettings:Audience").Value
                        
                    };
                });

            builder.Services.AddControllers();
            
            builder.Services.AddSingleton<IDbConnectionFactory,DbConnectionFactoryImpl>();
            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            builder.Services.AddScoped<IUserService, UserServiceImpl>();
            builder.Services.AddScoped<IBlogService, BlogServiceImpl>();

            builder.Services.AddTransient<IUserStore<ApplicationUser>, UserStoreImpl>();
            builder.Services.AddTransient<IRoleStore<IdentityRole>, RoleStoreImpl>();
            builder.Services.AddTransient<IUserRolesStore<IdentityRole,ApplicationUser>, UserRolesStoreImpl>();
            builder.Services.AddTransient<IBlogStore<Blog>, BlogStoreImpl>();
            builder.Services.AddTransient<IBlogLikesStore<Blog,ApplicationUser>, BlogLikesStoreImpl>();
            builder.Services.AddTransient<IBlogCommentsStore<BlogComment>, BlogCommentsStoreImpl>();
            builder.Services.AddTransient<IBlogOwnersStore<BlogOwner>, BlogOwnersStoreImpl>();
            builder.Services.AddTransient<IBlogEditorsStore<Blog, ApplicationUser>,BlogEditorsStoreImpl>();
            
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddDefaultTokenProviders();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMyExceptionHandler("/ErrorDevEnv");
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {
                app.UseMyExceptionHandler("/Error");
            }
            

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}