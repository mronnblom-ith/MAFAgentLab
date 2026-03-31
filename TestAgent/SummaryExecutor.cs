using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;

namespace TestAgent;

internal sealed partial class SummaryExecutor(AIAgent summaryAgent) : Executor("SummaryExecutor")
{
    protected override ProtocolBuilder ConfigureProtocol(ProtocolBuilder protocolBuilder)
    {
        return protocolBuilder;
    }

    [MessageHandler]
    private async ValueTask<string> HandleAsync(string message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        var response = await summaryAgent.RunAsync(message, cancellationToken: cancellationToken);

        return response.Text;
    }
}
