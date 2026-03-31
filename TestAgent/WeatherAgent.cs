using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;

namespace TestAgent;

internal static class WeatherAgent
{
    public const string AgentName = "WeatherAgent";

    public static AIAgent CreateAgent(AzureOpenAIClient azureOpenAIClient, string deploymentName)
    {
        return azureOpenAIClient
            .GetChatClient(deploymentName)
            .AsAIAgent(name: AgentName, instructions: "You are a friendly assistant. Keep your answers brief.",
            tools: [AIFunctionFactory.Create(WeatherTool.GetWeather)]);
    }
}
