# 4. Build a SK ChatGPT Bot to Manage the NFT Contract
* Install the SemanticKernel (SK) Nuget package
* Introduction to writing ChatGPT 'Prompts'
* Instruct ChatGPT to response to our request to do NFT booking operations
* Inform ChatGPT what functions are available for calling and when and how to call them
* The ChatGPT Bot structure
* Add the Chatbot Blazor web app to the existing MAUI App
* Examples of dialogue that we can create with ChatGPT

## Install the SemanticKernel (SK) Nuget package
* An open source developed by Microsoft to encapsulate OpenAI, ChatGPT, hugging face, etc into a C# library
* [GitHub source](https://github.com/microsoft/semantic-kernel)
```
<PackageReference Include="Microsoft.SemanticKernel" Version="1.0.1" />
```
## Introduction to writing ChatGPT 'Prompts' 
* Building functions using ChatGPT created an obvious paradigm shift in the way we give instructions to computers
* In the past, we give exact logical instructions in form of a syntactic language to computers, such is set, if/then/else, while, call, etc.
* To give instructions to ChatGPT, we write semantic 'prompts' in human languages
* [OpenAI's guide of writing better Prompts](https://platform.openai.com/docs/guides/prompt-engineering/six-strategies-for-getting-better-results)
* All program codings are already in the 'mauibc' folder
* You need an OpenAI trial account or even better, a paid account that pay as you go
## Instruct ChatGPT to response to our request to do NFT booking operations
* Below is the prompt we give to ChatGPT
```
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
```
## Inform ChatGPT what functions are available for calling and when and how to call them
* It is the 'function calling' feature of OpenAI
* We create 'Plugins' to tell OpenAI what functions are availabe and their meaning and usage
* E.g. in the Prompt above, we instruct ChatGPT to check the date's booking status and booking Id
* Below is the CheckBookingStatus Plugin
* Note the remarks and annotations to give meaning to parameters and function
* Note the return strings are also 'Prompts' for ChatGPT to answer the customer
```
/// <param name="date"> Date to check booking status</param>
/// <returns> Check Booking status of the given date </returns>
[KernelFunction, Description("Given a date, check its booking status")]
public string CheckBookingStatus(
    [Description("The date to check its booking status]")] string Date) {

    DateTime date = DateTime.ParseExact(Date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
    if (date < DateTime.UtcNow.Date) {
        return $"Failed: Date must be a future date!";
    }

    if (!nftService.IsConnected) { return "Please connect your Wallet first!"; }

    string message = $"There is no booking on {date.ToString("dd/MM/yyyy")}. Please reconfirm if you want to make booking.";
    NFTItem query = nftService.Lookup(date).Result;
    if (query.Id != 0) {
        if (query.Owner == nftService.MyAddress) {
            message = $"You have already booked {date.ToString("dd/MM/yyyy")}. The booking Id is {query.Id}";
            if (query.Vacant) {
                message += $" Please reconfirm if you want to Reclaim booking Id {query.Id}.";
            } else {
                message += $" Please reconfirm if you want to release booking Id {query.Id}.";
            }	
        } else {
            message = $"Booking on {date.ToString("dd/MM/yyyy")} is currently owned by {query.Owner}.";
            if (query.Vacant) {
                message += " You can request the owner to tranfer it to you.";
            } else {
                message += " You cannot book, release or reclaim on this date.";
            }
        }
    }
    return message;
}
```
## The ChatGPT Bot structure
### Plugins
* SK\NFTPlugin.cs: The major NFT functions, which in turn call the NFTService.cs in the MAUI App
* SK\TimePlugin.cs: Today, Tomorrow, Next Tuesday, Next Month, etc. ChatGPT is weak in date arithmetics
### SK Chat Library - IOpenAI.cs & OpenAI.cs
* OpenAI Constructor:
    * Login to OpenAI requesting the use of gpt-3.5-turbo-1106 LLM
    * Obtain your OpenAI key by 'Env.Var("OpenAIKey")'
    * You need to put your key in Windows Enviornment variable "OpenAIKey"
    * Add the Plugins - Tell ChatGPT the available functions to use
* chatHistory: the history of dialogues between you and ChatGPT
* startChat: Start a new chat by giving ChatGPT the instructions mentioned above
* getResponse: Post your question to ChatGPT and obtain the answer
    * ChatGPT will request execution of the Plugins
    * SK handles these requests behind the scene and calls these plugins
    * SK post the Plugins' returns (also prompts) back to ChatGPT
    * ChatGPT consolidates all plugin returns and return with an answer
* ChatHistoryCleared event: When the MAUI App logout, this event will be triggered. The ChatGPT Blazor web app will subscribe and handle this event to clear the chat history
### ChatGPT Blazor web app
* wwwroot
    * CSS and index.html for Blazor web app
    * scrollToBottom JavaScript
```
window.scrollToBottom = (elementId) => {
    var element = document.getElementById(elementId);
    console.log("scrollToBottom", elementId, element);
    if (element) {
        element.scrollTop = element.scrollHeight;
    }
}
```
* Pages\Main.razor
    * <div class="main">: display the chat history
    * spinner: when calling OpenAI and NFT services
    * <div class="footer">: for user to enter instruction to ChatGPT
    * <div role="alert">: to display alerts
    * OnInitializedAsync: start new chat and subscribe to ChatHistoryCleared event
    * await Task.Yield() & StateHasChanged(): force refresh of binding data
    * await JSRuntime.InvokeVoidAsync("scrollToBottom", "conversation-panel")
        * call a JavaScript in index.html
        * force the chat history to scroll to the latest conversion
    * SendChat()
        * Add user's instruction to chat history and refresh screen
        * Send the chat history to ChatGPT
        * Refresh the chat history to show the reply from ChatGPT
### Supporting files
* Models\NFT.cs & Service\NFTService.cs
    * Reuse files in MAUI App
* SK\ConsoleLogger.cs & Env.cs: SemanticKernel utilities
## Add the Chatbot Blazor web app to the existing MAUI App
* To make it more interesting, ChatGPT will be hosted as a Blazor web app inside the MAUI App
* [How to Add a Blazor web app in an existing .NET MAUI app](https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/blazorwebview?view=net-maui-8.0)
* Views\BlazorWebView.xaml
    * MAUI page to host the Blazor Main.razor
* MauiProgram.cs
    * builder.Services.AddMauiBlazorWebView();
    * builder.Services.AddBlazorWebViewDeveloperTools();
    * Register dependency injection for OpenAI, Plugins and BlazorWebView
* AppShell.xaml: add route to BlazorWebView
## Examples of dialogue that we can create with ChatGPT
* Please make booking on 15/02/2024 for me.
* Please show all my bookings.
* Is next Thursday available for booking?
    * you can also answer 'yes' to proceed
* Please show me available dates next week.
* Please release 15/02/2024 for me.
* I would like to reclaim 15/02/2024.
