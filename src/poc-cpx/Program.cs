using Microsoft.AspNetCore.Mvc;
using Octokit;
using System.Net.Http.Headers;
using Request = poc_cpx.Models.Request;
using Message = poc_cpx.Models.Message;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello Copilot!");

// make sure you change the App Name below
string yourGitHubAppName = " poc-copilot-extension";
string githubCopilotCompletionsUrl = 
    "https://api.githubcopilot.com/chat/completions";


app.MapPost("/agent", async (
    [FromHeader(Name = "X-GitHub-Token")] string githubToken, 
    [FromBody] Request userRequest) =>
{
    var octokitClient = 
        new GitHubClient(
            new Octokit.ProductHeaderValue(yourGitHubAppName))
    {
        Credentials = new Credentials(githubToken)
    };
    var user = await octokitClient.User.Current();

        userRequest.Messages.Insert(0, new Message
    {
        Role = "system",
        Content = 
            "Start every response with the user's name, " + 
            $"which is @{user.Login}"
    });
    userRequest.Messages.Insert(0, new Message
    {
        Role = "system",
        Content = 
            "You are a helpful assistant that replies to " +
            "user messages as if you were Blackbeard the Pirate."
    });

    var httpClient = new HttpClient();
    httpClient.DefaultRequestHeaders.Authorization = 
        new AuthenticationHeaderValue("Bearer", githubToken);
    userRequest.Stream = true;

    var copilotLLMResponse = await httpClient.PostAsJsonAsync(
        githubCopilotCompletionsUrl, userRequest);

    var responseStream = 
        await copilotLLMResponse.Content.ReadAsStreamAsync();
    return Results.Stream(responseStream, "application/json");
});

//https://reimagined-doodle-rrj7p7vqj5fx9pg-5153.app.github.dev/

app.Run();