using Application.Interfaces;
using Domain.Entities;
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
                GpuLayerCount = 0, // Set to a higher number if you have a compatible GPU
                Threads = Environment.ProcessorCount,
                BatchSize = 512
            };

            _model = LLamaWeights.LoadFromFile(_parameters);
            _context = _model.CreateContext(_parameters);
        }

        private InferenceParams GetInferenceParams() => new InferenceParams()
        {
            MaxTokens = 2048,
            // **FIX:** This is a much more robust list of stop words to catch all variations.
            AntiPrompts = new List<string>
            {
                "### Instruction:",
                "\n### Instruction:",
                "### Input:",
                "\n### Input:",
                " ### Input:",
                "### Response:",
                "User:",
                "\nUser:"
            },
            SamplingPipeline = new DefaultSamplingPipeline()
            {
                Temperature = 0.4f, // Lower temperature for more focused, less random output
                TopP = 0.9f,
            }
        };

        public async IAsyncEnumerable<string> AnalyzeTextAsync(string text, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var executor = new InstructExecutor(_context);
            var prompt = BuildGenericConversationPrompt(text);
            var inferenceParams = GetInferenceParams();

            await foreach (var token in executor.InferAsync(prompt, inferenceParams, cancellationToken))
            {
                yield return token;
            }
        }

        public async IAsyncEnumerable<string> RephraseTextAsync(string text, string language, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var executor = new InstructExecutor(_context);
            var prompt = BuildGenericRephrasePrompt(text, language);
            var inferenceParams = GetInferenceParams();

            await foreach (var token in executor.InferAsync(prompt, inferenceParams, cancellationToken))
            {
                yield return token;
            }
        }

        public async IAsyncEnumerable<string> TranslateTextAsync(string text, string sourceLanguage, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var executor = new InstructExecutor(_context);
            var prompt = BuildGenericTranslatePrompt(text, sourceLanguage);
            var inferenceParams = GetInferenceParams();

            await foreach (var token in executor.InferAsync(prompt, inferenceParams, cancellationToken))
            {
                yield return token;
            }
        }

        // --- Generic, Model-Agnostic Prompt Formats ---

        private string BuildGenericConversationPrompt(string userMessage)
        {
            // Reverting to the more stable instruction format with clear instructions.
            return $@"### Instruction:
You are a helpful AI assistant. Provide a direct, plain text response to the user's input. Do not use any special formatting, JSON, or markdown.

### Input:
{userMessage}

### Response:
";
        }

        private string BuildGenericRephrasePrompt(string text, string language)
        {
            return $@"### Instruction:
You are a language tool. Your task is to rephrase the user's text. Your response must be a valid JSON array of strings containing exactly two rephrased options, and nothing else.

### Input:
Rephrase the following {language} text: ""{text}""

### Response:
";
        }

        private string BuildGenericTranslatePrompt(string text, string sourceLanguage)
        {
            var targetLanguage = sourceLanguage.ToLower() == "english" ? "arabic" : "english";
            return $@"### Instruction:
You are a language translation tool. Your task is to translate the user's text. Your response must be a valid JSON array of strings containing exactly two translation options, and nothing else.

### Input:
Translate the following {sourceLanguage} text to {targetLanguage}: ""{text}""

### Response:
";
        }

        public void Dispose()
        {
            _context.Dispose();
            _model.Dispose();
        }
    }
}
