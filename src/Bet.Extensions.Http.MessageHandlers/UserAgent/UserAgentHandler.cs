using System.Net.Http.Headers;
using System.Reflection;

namespace Bet.Extensions.Http.MessageHandlers.UserAgent;

public class UserAgentHandler : DelegatingHandler
{
    public UserAgentHandler()
        : this(Assembly.GetEntryAssembly())
    {
    }

    public UserAgentHandler(Assembly assembly)
        : this(GetProduct(assembly), GetVersion(assembly))
    {
    }

    public UserAgentHandler(string applicationName, string applicationVersion)
    {
        if (applicationName == null)
        {
            throw new ArgumentNullException(nameof(applicationName));
        }

        if (applicationVersion == null)
        {
            throw new ArgumentNullException(nameof(applicationVersion));
        }

        UserAgentValues = new List<ProductInfoHeaderValue>()
        {
            new ProductInfoHeaderValue(applicationName.Replace(' ', '-'), applicationVersion),
            new ProductInfoHeaderValue($"({Environment.OSVersion})"),
        };
    }

    public UserAgentHandler(List<ProductInfoHeaderValue> userAgentValues)
    {
        UserAgentValues = userAgentValues ?? throw new ArgumentNullException(nameof(userAgentValues));
    }

    public List<ProductInfoHeaderValue> UserAgentValues { get; set; }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.Headers.UserAgent.Any())
        {
            foreach (var userAgentValue in UserAgentValues)
            {
                request.Headers.UserAgent.Add(userAgentValue);
            }
        }

        // Else the header has already been added due to a retry.
        return base.SendAsync(request, cancellationToken);
    }

    private static string GetProduct(Assembly assembly)
    {
        return GetAttributeValue<AssemblyProductAttribute>(assembly);
    }

    private static string GetVersion(Assembly assembly)
    {
        var infoVersion = GetAttributeValue<AssemblyInformationalVersionAttribute>(assembly);
        return infoVersion ?? GetAttributeValue<AssemblyFileVersionAttribute>(assembly);
    }

    private static string GetAttributeValue<T>(Assembly assembly)
        where T : Attribute
    {
        var type = typeof(T);
        var attribute = assembly
            .CustomAttributes
            .Where(x => x.AttributeType == type)
            .Select(x => x.ConstructorArguments.FirstOrDefault())
            .FirstOrDefault();

        return attribute == null ? string.Empty : attribute.Value.ToString();
    }
}
