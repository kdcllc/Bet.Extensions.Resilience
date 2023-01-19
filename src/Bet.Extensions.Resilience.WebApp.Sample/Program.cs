using Bet.AspNetCore.Middleware.Diagnostics;
using Bet.Extensions.Resilience.Http.Options;
using Bet.Extensions.Resilience.WebApp.Sample.Clients;
using Bet.Extensions.Resilience.WebApp.Sample.Models;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseResilienceOnStartup();

builder.Services.AddDeveloperListRegisteredServices(o =>
{
    o.PathOutputOptions = PathOutputOptions.Json;
});

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
    options.CheckConsentNeeded = context => true;
});

builder.Services.AddControllers();

builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages();

// https://docs.microsoft.com/sr-latn-rs/aspnet/core/mvc/views/view-compilation?view=aspnetcore-3.0
// https://github.com/aspnet/Announcements/issues/343
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

// services
//    .AddResilienceHttpClient<BibleClient, BibleClient>()
//    .ConfigureDefaultPolicies();
builder.Services.AddHttpDefaultResiliencePolicies(sectionName: "DefaultHttpPolicies");

builder.Services
    .AddHttpClient<IBibleClient, BibleClient>(nameof(BibleClient))

    // configurations for options becomes IChavahClient
    .ConfigureOptions<BibleClientOptions>()
    .AddPolicyHandlerFromRegistry(HttpPolicyOptionsKeys.HttpCircuitBreakerPolicy)
    .AddPolicyHandlerFromRegistry(HttpPolicyOptionsKeys.HttpRetryPolicy);

builder.Services.AddSingleton<ThrowModel>();

builder.Services
    .AddHttpClient<IThrowClient, ThrowClient>(nameof(ThrowClient))
    .ConfigureOptions<ThrowClientOptions>()
    .AddPolicyHandlerFromRegistry(HttpPolicyOptionsKeys.HttpCircuitBreakerPolicy)
    .AddPolicyHandlerFromRegistry(HttpPolicyOptionsKeys.HttpRetryPolicy);

var app = builder.Build();

if (app.Environment.IsDevelopment())
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

await app.RunAsync();
