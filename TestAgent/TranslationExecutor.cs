using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;

namespace TestAgent;

internal sealed partial class TranslationExecutor(AIAgent translationAgent) : Executor("TranslationExecutor")
{
    protected override ProtocolBuilder ConfigureProtocol(ProtocolBuilder protocolBuilder)
    {
        return protocolBuilder;
    }

    [MessageHandler]
    private async ValueTask<string> HandleAsync(string message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        var response = await translationAgent.RunAsync(message, cancellationToken: cancellationToken);

        return response.Text;
    }
}
