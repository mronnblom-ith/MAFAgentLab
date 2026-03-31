using Azure;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Workflows;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenAI.Chat;
using System.Reflection;
using TestAgent;

var builder = WebApplication.CreateBuilder(args);

// Ensure user secrets are loaded in Development so builder.Configuration can read them
builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true);

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

//StartWeatherAgent();
//StartWorkflow();
StartWorkflow2();

// Register Services needed to run DevUI
builder.Services.AddOpenAIResponses();
builder.Services.AddOpenAIConversations();

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

void StartWorkflow() // Currently doesn't work.. HandleAsync not called? Bug?
{
    var chatClient = azureClient.GetChatClient(deploymentName);

    // Create agents
    ChatClientAgent summaryAgent = chatClient.AsAIAgent(name: "SummaryAgent", instructions: "Summarize the text you are given to max 20 words");
    ChatClientAgent translationAgent = chatClient.AsAIAgent(name: "TranslationAgent", instructions: "Given a text Translate it to French (you need to translate the summary and not the original text)");

    // Create executors
    var summaryExecutor = new SummaryExecutor(summaryAgent);
    var translationExecutor = new TranslationExecutor(translationAgent);
    var uppercaseExecutor = new UppercaseExecutor();

    // Build the workflow
    //Workflow workflow = AgentWorkflowBuilder.BuildSequential(summaryAgent, translationAgent);
    var workflow = new WorkflowBuilder(summaryExecutor)
        .AddEdge(summaryExecutor, translationExecutor) // Connect summary to translation so translation gets the output of summary as input
        .AddEdge(translationExecutor, uppercaseExecutor)
        .WithName("MyWorkflow")
        .Build();

    builder.AddWorkflow(workflow.Name, (sp, key) => workflow);
}

void StartWorkflow2()
{
    var chatClient = azureClient.GetChatClient(deploymentName);

    // Create agents
    ChatClientAgent summaryAgent = chatClient.AsAIAgent(name: "SummaryAgent", instructions: "Summarize the text you are given to max 20 words");
    ChatClientAgent translationAgent = chatClient.AsAIAgent(name: "TranslationAgent", instructions: "Given a text Translate it to French (you need to translate the summary and not the original text)");

    // Create executors
    var uppercaseExecutor = new UppercaseExecutor();

    // Build the workflow
    //Workflow workflow = AgentWorkflowBuilder.BuildSequential(summaryAgent, translationAgent);
    var workflow = new WorkflowBuilder(summaryAgent)
        .AddEdge(summaryAgent, translationAgent) // Connect summary to translation so translation gets the output of summary as input
        .AddEdge(translationAgent, uppercaseExecutor)
        .WithName("MyWorkflow")
        .Build();

    builder.AddWorkflow(workflow.Name, (sp, key) => workflow);
}