using DeveloperPath.Application;
using DeveloperPath.Application.Common.Interfaces;
using DeveloperPath.Infrastructure;
using DeveloperPath.Infrastructure.Persistence;
using DeveloperPath.WebApi.Filters;
using DeveloperPath.WebApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSwag;

namespace DeveloperPath.WebApi
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
      services.AddApplication();
      services.AddInfrastructure(Configuration);

      services.AddScoped<ICurrentUserService, CurrentUserService>();

      services.AddHttpContextAccessor();

      services.AddHealthChecks()
          .AddDbContextCheck<ApplicationDbContext>();

      services.AddControllersWithViews(options =>
          options.Filters.Add(new ApiExceptionFilter()));

      services.AddRazorPages();

      // Customise default API behaviour
      services.Configure<ApiBehaviorOptions>(options =>
      {
        options.SuppressModelStateInvalidFilter = true;

        // in case of Model validation error return HTTP 422 instead of 401
        options.InvalidModelStateResponseFactory = actionContext =>
        {
          var actionExecutingContext =
              actionContext as Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext;

          // if there are modelstate errors & all keys were correctly
          // found/parsed we're dealing with validation errors
          if (actionContext.ModelState.ErrorCount > 0
              && actionExecutingContext?.ActionArguments.Count == actionContext.ActionDescriptor.Parameters.Count)
          {
            return new UnprocessableEntityObjectResult(actionContext.ModelState);
          }

          // if one of the keys wasn't correctly found / couldn't be parsed
          // we're dealing with null/unparsable input
          return new BadRequestObjectResult(actionContext.ModelState);
        };
      });


      services.AddOpenApiDocument(configure =>
      {
        configure.PostProcess = (document) =>
        {
          document.Info.Version = "v1";
          document.Info.Title = "Developer Path API";
          document.Info.Description = "Developer Path project Open API";
          document.Info.Contact = new OpenApiContact
          {
            Name = "Sergey Benzenko",
            Email = "sbenzenko@gmail.com",
            Url = "https://t.me/NetDeveloperDiary"
          };
        };

        //configure.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
        //{
        //  Type = OpenApiSecuritySchemeType.ApiKey,
        //  Name = "Authorization",
        //  In = OpenApiSecurityApiKeyLocation.Header,
        //  Description = "Type into the textbox: Bearer {your JWT token}."
        //});

        //configure.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));
      });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
        app.UseMigrationsEndPoint();
      }
      else
      {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
      }

      //app.UseHealthChecks("/health");
      
      app.UseHttpsRedirection();
      
      //app.UseStaticFiles();
      //if (!env.IsDevelopment())
      //{
      //  app.UseSpaStaticFiles();
      //}

      app.UseSwaggerUi3(settings =>
      {
        settings.Path = "/api";
        settings.DocumentPath = "/api/specification.json";
      });

      app.UseRouting();

      app.UseAuthentication();
      //app.UseIdentityServer();
      app.UseAuthorization();
      app.UseEndpoints(endpoints =>
      {
        //endpoints.MapControllerRoute(
        //          name: "default",
        //          pattern: "{controller}/{action=Index}/{id?}");
        endpoints.MapControllers();
      });

      //app.UseSpa(spa =>
      //{
      //  // To learn more about options for serving an Angular SPA from ASP.NET Core,
      //  // see https://go.microsoft.com/fwlink/?linkid=864501

      //  spa.Options.SourcePath = "ClientApp";

      //  if (env.IsDevelopment())
      //  {
      //    spa.UseAngularCliServer(npmScript: "start");
      //  }
      //});

    }
  }
}
