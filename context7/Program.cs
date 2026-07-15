using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;

using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.ClearProviders();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();

[McpServerToolType]
public static class Context7Tool
{
    [McpServerTool]
    [Description("Fetches documentation, guides and code examples for a framework or package.")]
    public static string FetchDocumentation(string query)
    {
        return $"Retrieving documentation for query '{query}': [Mocked documentation content and context API specifications]";
    }
}
