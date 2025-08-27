using Microsoft.Extensions.AI;
using OllamaSharp;
using Microsoft.Extensions.Configuration;

var appSettingsJson = "appsettings.json";

#if DEBUG
    appSettingsJson = "appsettings.Development.json";
#endif

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile(appSettingsJson)
    .AddEnvironmentVariables()
    .Build();

var endpoint = new Uri(config["OllamaConfig:Endpoint"]!);
var model = config["OllamaConfig:Model"]!;

// Init Ollama client targetting the "gpt-oss:20b" model

IChatClient chatClient = new OllamaApiClient(endpoint, model);

// Chat History

List<ChatMessage> chatHistory = [];

Console.WriteLine($"{model} Chat - Type 'exit' to quit");
Console.WriteLine();

while (true)
{
    Console.Write("You: ");
    var userInput = Console.ReadLine();

    if (userInput?.ToLower() == "exit")
    {
        break;
    }

    if (string.IsNullOrWhiteSpace(userInput))
    {
        continue;
    }

    // Add user message to chat history
    chatHistory.Add(new ChatMessage(ChatRole.User, userInput));

    // Stream the AI response and display in real time
    Console.Write("Assistant: ");
    var assistantResponse = "";

    await foreach (var update in chatClient.GetStreamingResponseAsync(chatHistory))
    {
        Console.Write(update.Text);
        assistantResponse += update.Text;
    }

    Console.WriteLine();

    chatHistory.Add(new ChatMessage(ChatRole.Assistant, assistantResponse));
}