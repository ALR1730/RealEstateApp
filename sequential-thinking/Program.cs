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
public static class SequentialThinkingTool
{
    [McpServerTool]
    [Description("A detailed, step-by-step thinking tool for complex reasoning tasks.")]
    public static string SequentialThinking(
        string thought,
        int thoughtNumber,
        int totalThoughts,
        bool nextThoughtNeeded,
        bool? isRevision = null,
        int? revisesThought = null,
        int? branchFromThought = null,
        string? branchId = null,
        bool? needsMoreThoughts = null)
    {
        var revisionInfo = isRevision == true ? $" (Revision of thought #{revisesThought})" : "";
        var branchInfo = branchFromThought != null ? $" (Branching from thought #{branchFromThought}, Branch ID: {branchId})" : "";
        return $"[Thought #{thoughtNumber}/{totalThoughts}]{revisionInfo}{branchInfo}: {thought}";
    }
}
