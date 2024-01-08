using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;

namespace ChatMAUI;
public interface IOpenAI {
	ChatHistory? chatHistory { get; set; }
	Task startChat();
	Task<OpenAIChatMessageContent> getResponse(string input);
	void OnChatHistoryCleared();
	event EventHandler? ChatHistoryCleared;
}
