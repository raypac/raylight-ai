using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

var appSettingsJson = "appsettings.json";

#if DEBUG
    appSettingsJson = "appsettings.Development.json";
#endif

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile(appSettingsJson)
    .AddEnvironmentVariables()
    .Build();

var endpoint = new Uri(config["AzureOpenAI:Endpoint"]!);
var key = config["AzureOpenAI:ApiKey"]!;
var chatDeployment = config["AzureOpenAI:ChatDeployment"]!;
var embedDeployment = config["AzureOpenAI:EmbedDeployment"]!;

var maxOutputTokenCount = int.Parse(config["RequestOptions:MaxOutputTokenCount"] ?? "4096");
var temperature = float.Parse(config["RequestOptions:Temperature"] ?? "1.0");
var topP = float.Parse(config["RequestOptions:TopP"] ?? "1.0");


// Init OpenAI client

var azureClient = new AzureOpenAIClient(endpoint, new AzureKeyCredential(key));
var chatClient = azureClient.GetChatClient(chatDeployment);

// Setup Options

var requestOptions = new ChatCompletionOptions()
{
    MaxOutputTokenCount = maxOutputTokenCount,
    Temperature = temperature,
    TopP = topP,
};

// Chat History

var chatHistory = new List<ChatMessage>();

Console.WriteLine($"{chatDeployment} Chat - Type 'exit' to quit");
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
    chatHistory.Add(new UserChatMessage(userInput));

    // Stream the AI response and display in real time
    Console.Write("Assistant: ");
    var assistantResponse = "";

    await foreach (var update in chatClient.CompleteChatStreamingAsync(chatHistory))
    {
        foreach (var message in update.ContentUpdate)
        {
            Console.Write(message.Text);
            assistantResponse += message.Text;
        }
    }

    Console.WriteLine();

    chatHistory.Add(new SystemChatMessage(assistantResponse));
}