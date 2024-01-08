using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using MAUIBC;

namespace ChatMAUI;
public class OpenAI: IOpenAI {
	private Kernel kernel { get; set; }
	private OpenAIPromptExecutionSettings settings { get; set; }
	public ChatHistory? chatHistory { get; set; }
	private IChatCompletionService chat { get; set; }
	public event EventHandler? ChatHistoryCleared;
	public virtual void OnChatHistoryCleared() {
		ChatHistoryCleared?.Invoke(this, EventArgs.Empty);
	}
	private const string Persona = $"""
You are a booking officer in the customer service counter. Please answer questions about bookings by following the instructions below.
- You should use the functions provided to answer questions.
- Only answer questions related to make booking, enquire, release, Reclaim and list bookings.
- To make, release or Reclaim a booking
  1, Check the given date's booking status and booking Id.
  2, Request the customer to double confirm.
  3, Call the corresponding function.
- If you're unsure of an answer, you can say "Sorry, I don't have the information right now.
- All bookings are on a daily bases in the format of dd/MM/yyyy.
- When customer says Today, Tomorrow, next Thursday, etc, translate them to date format before calling the functions.
- If the booking owner is 'You', inform the customer that he or she has already booked on the date.
- Output of the functions may suggest follow-up actions to the customer. Please follow up with the customer.
- If the date range to list available dates is too large, inform the customer that only the first 10 available dates will be shown.
- The available date list contains some (*) remarks to customer. Please show them to the customer.
- Use the Enquire Availablility function to check a single date. Use the List Available Dates function to check a range of dates.
- if error is 'wallet not connected', please ask the customer to connect the wallet first.
""";
	//any available dates for booking next month?
	public OpenAI(INFTService nftservice) {
		IKernelBuilder builder = Kernel.CreateBuilder();
		builder.AddOpenAIChatCompletion("gpt-3.5-turbo-1106", Env.Var("OpenAIKey"), serviceId: "chat");
		builder.Services.AddLogging(services => services.AddDebug().SetMinimumLevel(LogLevel.Trace));
		kernel = builder.Build();
		INFTPlugin nftplugin = new NFTPlugin(nftservice);
		kernel.Plugins.AddFromObject(nftplugin);
		kernel.Plugins.AddFromType<TimePlugin>();

		chat = kernel.GetRequiredService<IChatCompletionService>();
		settings = new() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions, Temperature = 0.2, TopP = 0.2 };
	}

	public async Task startChat() {
		chatHistory = new ChatHistory();
		chatHistory.AddSystemMessage(Persona);
		var result = (OpenAIChatMessageContent)await chat.GetChatMessageContentAsync(chatHistory, settings, kernel).ConfigureAwait(false);
		chatHistory.Add(result);
	}

	public async Task<OpenAIChatMessageContent> getResponse(string input) {
		var result = (OpenAIChatMessageContent)await chat.GetChatMessageContentAsync(chatHistory, settings, kernel).ConfigureAwait(false);
		chatHistory.Add(result);
		return result;
	}
}
