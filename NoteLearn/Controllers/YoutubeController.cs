using Microsoft.AspNetCore.Mvc;
using NoteLearn.Dtos;
using NoteLearn.Services;
using System.Text.Json;
namespace NoteLearn.Controllers;
[ApiController]
[Route("api/youtube")]
public class YoutubeController : ControllerBase
{
    private readonly YoutubeTranscriptService _service;

    public YoutubeController(YoutubeTranscriptService service)
    {
        _service = service;
    }

    [HttpPost("transcript")]
    public async Task<IActionResult> GetTranscript(
        [FromBody] YoutubeTranscriptRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.YoutubeUrl))
            return BadRequest("YoutubeUrl is required");

        var videoId = _service.ExtractVideoId(request.YoutubeUrl);

        var transcriptJson = await _service.GetTranscriptAsync(videoId);

        return Ok(new YoutubeTranscriptResponse
        {
            VideoId = videoId,
            TranscriptRaw = JsonSerializer.Deserialize<object>(transcriptJson)!
        });
    }
}
