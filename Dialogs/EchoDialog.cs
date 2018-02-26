using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.GDPREchoBot
{
    [Serializable]
    public class EchoDialog : IDialog<object>
    {
        protected int count = 1;

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            var outputMessage = new StringBuilder();
            var continuationToken = "";
            switch (message.Text.ToLower())
            {
                case "reset":
                PromptDialog.Confirm(
                    context,
                    AfterResetAsync,
                    "Are you sure you want to reset the count?",
                    "Didn't get that!",
                    promptStyle: PromptStyle.Auto);
                    break;
                case "getconversations":
                case "get conversations":

                    using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, message))
                    {
                        var client = scope.Resolve<IConnectorClient>();
                        do
                        {
                            var conversationResults = client.Conversations.GetConversations(continuationToken);
                            if (conversationResults == null)
                            {
                                outputMessage.Append("Internal error, conversation results was empty");
                            }
                            else if (conversationResults.Conversations == null)
                            {
                                outputMessage.Append("No conversations found for this bot in this channel");
                            }
                            else
                            {
                                outputMessage.Append($"Here is a batch of {conversationResults.Conversations.Count()} conversations:\n");
                                foreach (var conversationMembers in conversationResults.Conversations)
                                {
                                    string members = string.Join(", ",
                                        conversationMembers.Members.Select(member => member.Name));
                                    outputMessage.Append($"Conversation: {conversationMembers.Id} members: {members}\n");
                                }
                            }
                           
                            continuationToken = conversationResults.Skip;
                        } while (!string.IsNullOrEmpty(continuationToken));
                    }
                    await context.PostAsync(outputMessage.ToString());
                    // LATER: Look at this for how we access the bot state information.  
                    // context.ConversationData.
                    break;
                case "exportstate":
                case "export state":
                    // Get the state data for all conversations that this bot participates in

                    do
                    {
                        var result = await context.ExportStateDataAsync(continuationToken);
                        foreach (var datum in result.BotStateData)
                        {
                            outputMessage.Append($"conversationID: {datum.ConversationId}\tuserId: {datum.UserId}\tdata:{datum.Data}\n");
                        }
                        continuationToken = result.ContinuationToken;
                    }  while (!string.IsNullOrEmpty(continuationToken));
                                     
                    await context.PostAsync(outputMessage.ToString());
                    break; 
                default:

                    await context.PostAsync($"{this.count++}: You said {message.Text}");
                    context.Wait(MessageReceivedAsync);
                    break;
            }          
        }

        public async Task AfterResetAsync(IDialogContext context, IAwaitable<bool> argument)
        {
            var confirm = await argument;
            if (confirm)
            {
                this.count = 1;
                await context.PostAsync("Reset count.");
            }
            else
            {
                await context.PostAsync("Did not reset count.");
            }
            context.Wait(MessageReceivedAsync);
        }

    }
}