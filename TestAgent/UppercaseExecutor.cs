using Microsoft.Agents.AI.Workflows;

namespace TestAgent;

internal sealed partial class UppercaseExecutor() : Executor("UppercaseExecutor")
{
    protected override ProtocolBuilder ConfigureProtocol(ProtocolBuilder protocolBuilder)
    {
        return protocolBuilder;
    }

    [MessageHandler]
    private ValueTask<string> HandleAsync(string message, IWorkflowContext context)
    {
        string result = message.ToUpperInvariant();
        return ValueTask.FromResult(result); // Return value is automatically sent to connected executors
    }
}
