using Microsoft.AspNet.SignalR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Net;
using System.Threading.Tasks;
using WordGame.API.Application.Configuration;
using WordGame.API.Application.Services;
using WordGame.API.Data.Repositories;
using WordGame.API.Domain.Repositories;
using WordGame.API.Extensions;
using WordGame.API.Hubs;
using WordGame.API.Middleware;

namespace WordGame.API
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllersWithViews()
				.AddNewtonsoftJson(options =>
				{
					options.SerializerSettings.Converters.Add(
						new StringEnumConverter(new CamelCaseNamingStrategy(false, true)));

					JsonConvert.DefaultSettings = () => options.SerializerSettings;
				});

			services.AddOpenApiDocument();

			// In production, the React files will be served from this directory
			services.AddSpaStaticFiles(configuration =>
			{
				configuration.RootPath = "ClientApp/build";
			});

			services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
				.AddCookie(options =>
				{
					options.Events = new CookieAuthenticationEvents
					{
						OnRedirectToLogin = context =>
						{
							context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
							return Task.CompletedTask;
						},
						OnRedirectToAccessDenied = context =>
						{
							context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
							return Task.CompletedTask;
						}
					};
				});

			services.Configure<BotSettings>(Configuration.GetSection(nameof(BotSettings)));
			services.AddMongo(Configuration);
			services.AddScoped<IRandomAccessor, RandomAccessor>();
			services.AddScoped<IGameRepository, GameRepository>();
			services.AddScoped<INameGenerator, NameGenerator>();
			services.AddScoped<IGameBoardGenerator, GameBoardGenerator>();
			services.AddScoped<IGameUpdater, GameUpdater>();

			GlobalHost.DependencyResolver.Register(typeof(IUserIdProvider), () => new PlayerIdProvider());

			services.AddSignalR()
				.AddNewtonsoftJsonProtocol(options =>
				{
					options.PayloadSerializerSettings.Converters.Add(
						new StringEnumConverter(new CamelCaseNamingStrategy(false, true)));
				});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseSpaStaticFiles();

			app.UseRouting();

			app.UseOpenApi();
			app.UseSwaggerUi3();
			app.UseReDoc();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseMiddleware<HttpExceptionMiddleware>();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapHub<GameHub>("/hubs/game");
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller}/{action=Index}/{id?}");
			});

			app.UseSpa(spa =>
			{
				spa.Options.SourcePath = "ClientApp";

				if (env.IsDevelopment())
				{
					spa.UseReactDevelopmentServer(npmScript: "start");
				}
			});
		}
	}
}
