
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenAI.Chat;
using TestAgent;

var builder = WebApplication.CreateBuilder(args);

// Ensure user secrets are loaded in Development so builder.Configuration can read them
builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true);

var endpoint = new Uri("https://svante-openai.openai.azure.com/");
var model = "gpt-5.4";
var deploymentName = "gpt-5.4";
// Load API key from user secrets (configuration). Set via:
// dotnet user-secrets set "OpenAI:ApiKey" "<your-key>"
var apiKey = builder.Configuration["OpenAI:ApiKey"];
if (string.IsNullOrEmpty(apiKey))
{
    throw new InvalidOperationException("OpenAI API key not configured. Run 'dotnet user-secrets set \"OpenAI:ApiKey\" \"<your-key>\"' to store it for development.");
}

AzureOpenAIClient azureClient = new(endpoint, new AzureKeyCredential(apiKey));

// Register Services needed to run DevUI
//builder.Services.AddChatClient(azureClient.GetChatClient(deploymentName).AsIChatClient()); //You need to register a chat client for the dummy agents to use
builder.Services.AddOpenAIResponses();
builder.Services.AddOpenAIConversations();

//Build a "normal" Agent
string realAgentName = "TestAgent";
AIAgent myAgent = azureClient
    .GetChatClient(deploymentName)
    .AsAIAgent(name: realAgentName, instructions: "You are a friendly assistant. Keep your answers brief.",
    tools: [AIFunctionFactory.Create(WeatherTool.GetWeather)]);

builder.AddAIAgent(realAgentName, (serviceProvider, key) => myAgent); //Get registered as a keyed singleton so name on real agent and key must match

//ChatClient chatClient = azureClient.GetChatClient(deploymentName);

//AIAgent agent = chatClient
//    .AsAIAgent(instructions: "You are a friendly assistant. Keep your answers brief.", name: "TestAgent",
//    tools: [AIFunctionFactory.Create(WeatherTool.GetWeather)]);

//builder.AddAIAgent(instructions: "You are a friendly assistant. Keep your answers brief.", name: "TestAgent");

//Console.WriteLine(await agent.RunAsync("What is the largest city in France and what is the weather there?"));

WebApplication app = builder.Build();

app.MapOpenAIResponses();
app.MapOpenAIConversations();
app.MapDevUI();

app.Run();
