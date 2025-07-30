using Application.Interfaces;
using LLama;
using LLama.Common;
using LLama.Sampling;
using System.Runtime.CompilerServices;

namespace Infrastructure.Ai
{
    public class LocalAiAnalysisService : IAiAnalysisService, IDisposable
    {
        private readonly LLamaWeights _model;
        private readonly LLamaContext _context;
        private readonly ModelParams _parameters;

        public LocalAiAnalysisService(string modelPath)
        {
            if (string.IsNullOrEmpty(modelPath) || !File.Exists(modelPath))
            {
                throw new ArgumentException($"Local AI model path '{modelPath}' is not configured or the file does not exist.");
            }

            _parameters = new ModelParams(modelPath)
            {
                GpuLayerCount = 0,
                Threads = Environment.ProcessorCount,
                BatchSize = 512
            };

            _model = LLamaWeights.LoadFromFile(_parameters);
            _context = _model.CreateContext(_parameters);
        }

        public async IAsyncEnumerable<string> AnalyzeTextAsync(string text, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var executor = new InstructExecutor(_context);

            var prompt = BuildPrompt(text);

            var inferenceParams = new InferenceParams()
            {
                MaxTokens = 1024,
                AntiPrompts = new List<string> { "<end_of_turn>" },
                SamplingPipeline = new DefaultSamplingPipeline()
                {
                    Temperature = 0.7f,
                    TopK = 40,
                    TopP = 0.9f,
                }
            };

            await foreach (var token in executor.InferAsync(prompt, inferenceParams, cancellationToken))
            {
                yield return token;
            }
        }

        private string BuildPrompt(string userMessage)
        {
            return $@"<start_of_turn>user
Respond to the following prompt. Your response should be well-formatted. Use Markdown for lists, bold text, and especially for creating tables if the content is tabular in nature. Do not output any extra characters or conversational text after your response is complete.

Prompt:
---
{userMessage}<end_of_turn>
<start_of_turn>model
";
        }

        public void Dispose()
        {
            _context.Dispose();
            _model.Dispose();
        }
    }
}
