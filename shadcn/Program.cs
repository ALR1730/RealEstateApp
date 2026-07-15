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
public static class ShadcnTool
{
    [McpServerTool]
    [Description("Lists all available Shadcn components.")]
    public static string ListShadcnComponents()
    {
        return "button, dialog, card, table, form, input, select, checkbox, switch, tabs, accordion, avatar, badge";
    }

    [McpServerTool]
    [Description("Gets source code and details for a specific component.")]
    public static string GetComponentDetails(string componentName)
    {
        return $"Source code and details for shadcn component '{componentName}': [Mocked component source code]";
    }

    [McpServerTool]
    [Description("Searches for a component by a keyword query.")]
    public static string SearchComponents(string query)
    {
        return $"Found components matching '{query}': button, card, input (mock search results)";
    }
}
