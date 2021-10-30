using Bet.AspNetCore.Middleware.Diagnostics;
using Bet.Extensions.Resilience.Http.Options;
using Bet.Extensions.Resilience.WebApp.Sample.Clients;

namespace Bet.Extensions.Resilience.WebApp.Sample
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
            services.AddDeveloperListRegisteredServices(o =>
            {
                o.PathOutputOptions = PathOutputOptions.Json;
            });

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
            });

            services.AddControllers().AddNewtonsoftJson();

            services.AddControllersWithViews().AddNewtonsoftJson();

            // https://docs.microsoft.com/sr-latn-rs/aspnet/core/mvc/views/view-compilation?view=aspnetcore-3.0
            // https://github.com/aspnet/Announcements/issues/343
            services.AddRazorPages().AddRazorRuntimeCompilation();

            // services
            //    .AddResilienceHttpClient<IChavahClient, ChavahClient>()
            //    .ConfigureDefaultPolicies();
            services.AddHttpDefaultResiliencePolicies(sectionName: "DefaultHttpPolicies");

            services
                .AddHttpClient<IChavahClient, ChavahClient>(nameof(ChavahClient))

                // configurations for options becomes IChavahClient
                // .AddHttpClient<IChavahClient, ChavahClient>()
                .ConfigureOptions<ChavahClientOptions>()
                .AddPolicyHandlerFromRegistry(HttpPolicyOptionsKeys.HttpCircuitBreakerPolicy)
                .AddPolicyHandlerFromRegistry(HttpPolicyOptionsKeys.HttpRetryPolicy);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDeveloperListRegisteredServices();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseCookiePolicy();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                   name: "default",
                   pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapRazorPages();
            });
        }
    }
}
