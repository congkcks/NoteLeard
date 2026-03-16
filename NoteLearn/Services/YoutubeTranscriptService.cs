namespace NoteLearn.Services;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
public class YoutubeTranscriptService
{
    private readonly HttpClient _http;
    private readonly string _apiUrl;
    private readonly string _apiKey;

    public YoutubeTranscriptService(IConfiguration config)
    {
        _http = new HttpClient();
        _apiUrl = config["YoutubeTranscript:ApiUrl"]!;
        _apiKey = config["YoutubeTranscript:ApiKey"]!;
    }

    public async Task<string> GetTranscriptAsync(string videoId)
    {
        var body = JsonSerializer.Serialize(new
        {
            ids = new[] { videoId }
        });

        var request = new HttpRequestMessage(HttpMethod.Post, _apiUrl);
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Basic", _apiKey);
        request.Content = new StringContent(body, Encoding.UTF8, "application/json");

        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    public string ExtractVideoId(string youtubeUrl)
    {
        // hỗ trợ nhiều dạng URL
        if (youtubeUrl.Contains("v="))
            return youtubeUrl.Split("v=")[1].Split('&')[0];

        if (youtubeUrl.Contains("youtu.be/"))
            return youtubeUrl.Split("youtu.be/")[1].Split('?')[0];

        throw new ArgumentException("Invalid YouTube URL");
    }
}
