using Azure;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using TestAgent;

var builder = WebApplication.CreateBuilder(args);

// Ensure user secrets are loaded in Development so builder.Configuration can read them
builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true);

// Register Services needed to run DevUI
builder.Services.AddOpenAIResponses();
builder.Services.AddOpenAIConversations();

var endpoint = new Uri("https://svante-openai.openai.azure.com/");
var deploymentName = "gpt-5.4";
// Load API key from user secrets (configuration). Set via:
// dotnet user-secrets set "OpenAI:ApiKey" "<your-key>"
var apiKey = builder.Configuration["OpenAI:ApiKey"];
if (string.IsNullOrEmpty(apiKey))
{
    throw new InvalidOperationException("OpenAI API key not configured. Run 'dotnet user-secrets set \"OpenAI:ApiKey\" \"<your-key>\"' to store it for development.");
}

AzureOpenAIClient azureClient = new(endpoint, new AzureKeyCredential(apiKey));

StartWeatherAgent();

WebApplication app = builder.Build();
app.MapOpenAIResponses();
app.MapOpenAIConversations();
app.MapDevUI();
app.Run();


void StartWeatherAgent()
{
    var agent = WeatherAgent.CreateAgent(azureClient, deploymentName);
    builder.AddAIAgent(WeatherAgent.AgentName, (serviceProvider, key) => agent); //Get registered as a keyed singleton so name on real agent and key must match
}
