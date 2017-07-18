using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SpentBook.Web.Data;
using SpentBook.Web.Models;
using SpentBook.Web.Services;
using SpentBook.Domain;
using SpentBook.Web.Binders;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Http;
using SpentBook.Web.Filters;
using Microsoft.AspNetCore.Http.Features;
using SpentBook.Domain.Services;

namespace SpentBook.Web
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
            services.AddScoped<IUnitOfWork, PocDatabaseUoW>();
            services.AddScoped((service) => new TransactionTableService(service.GetService<IUnitOfWork>()));
            services.AddScoped((service) => {
                return new TransactionService(service.GetService<IUnitOfWork>());
            }
            );


            // Razor render dependences
            services.AddTransient<ViewRenderService>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            
            // assim pega o body real
            services.Configure<FormOptions>(options => options.BufferBody = true);

            services.AddDbContext<ApplicationContext>(options =>
                options.UseSqlServer(ConfigurationManager.GetConnectionString()));

            services.AddDbContext<IdentityDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<IdentityDbContext>()
                .AddDefaultTokenProviders();

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();

            // Other configuration code
            services.AddMvc(options =>
            {
                // Using the followint code to add the new binder
                options.AddFlagsEnumModelBinderProvider();
                options.Filters.Add(new JsonOutputWhenGenericExceptionAttribute());
            }).AddJsonOptions(opt =>
            {
                var resolver = opt.SerializerSettings.ContractResolver;
                if (resolver != null)
                {
                    var res = resolver as DefaultContractResolver;
                    res.NamingStrategy = null;

                    opt.SerializerSettings.DateFormatString = "dd/MM/yyyy HH:mm:ss";
                }
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}