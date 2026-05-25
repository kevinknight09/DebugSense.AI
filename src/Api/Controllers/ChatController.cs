using DebugSense.Retrieval;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DebugSense.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly RagOrchestratorService ragOrchestratorService;

        public ChatController(RagOrchestratorService orchestratorService)
        {
            ragOrchestratorService = orchestratorService;
        }

        public async Task Post([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Query)) return;

            // Tell the browser to expect a live stream of Server-Sent Events
            Response.Headers.Append("Content-Type", "text/event-stream");

            await foreach (var token in ragOrchestratorService.AnswerQuestionStreamAsync(request.Query))
            {
                var safeToken = token.Replace("\n", "\\n"); // Format newlines for SSE
                await Response.WriteAsync($"data: {safeToken}\n\n");
                await Response.Body.FlushAsync();
            }
        }
    }
}

