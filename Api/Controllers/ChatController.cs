﻿using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IAiAnalysisService _aiAnalysisService;

        public ChatController(IAiAnalysisService aiAnalysisService)
        {
            _aiAnalysisService = aiAnalysisService;
        }

        [HttpPost("stream")]
        public IAsyncEnumerable<string> ChatStream([FromBody] AnalysisRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.TextToAnalyze))
            {
                return AsyncEnumerable.Empty<string>();
            }

            return _aiAnalysisService.AnalyzeTextAsync(request.TextToAnalyze, cancellationToken);
        }

        [HttpPost("rephrase")]
        public IAsyncEnumerable<string> Rephrase([FromBody] RephraseRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Text) || string.IsNullOrWhiteSpace(request.Language))
            {
                return AsyncEnumerable.Empty<string>();
            }

            return _aiAnalysisService.RephraseTextAsync(request.Text, request.Language, cancellationToken);
        }

        [HttpPost("translate")]
        public IAsyncEnumerable<string> Translate([FromBody] TranslateRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Text) || string.IsNullOrWhiteSpace(request.SourceLanguage))
            {
                return AsyncEnumerable.Empty<string>();
            }

            return _aiAnalysisService.TranslateTextAsync(request.Text, request.SourceLanguage, cancellationToken);
        }
    }
}
