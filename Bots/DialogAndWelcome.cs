// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QnABot.Models;

namespace Tutorial.Bot
{
    public class DialogAndWelcomeBot<T> : DialogBot<T> where T : Dialog
    {
        public DialogAndWelcomeBot( ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger)
            : base(conversationState, userState, dialog, logger)
        {
        }

        // Messages sent to the user.
        private const string WelcomeMessage = @" bem vindo! Faz me tuas perguntas?";

      

        protected override async Task OnMembersAddedAsync(
            IList<ChannelAccount> membersAdded,
            ITurnContext<IConversationUpdateActivity> turnContext,
            CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
               
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                   
                    var welcomeCard = CreateAdaptiveCardAttachment();
                    var response = CreateResponse(turnContext.Activity, welcomeCard);
                    await turnContext.SendActivityAsync(response, cancellationToken);
                    await turnContext.SendActivityAsync($"Olá - {member.Name}{WelcomeMessage}", cancellationToken: cancellationToken);
                 
                    
                }
            }
        }
        private Activity CreateResponse(IActivity activity, Attachment attachment)
        {
            var response = ((Activity)activity).CreateReply();
            response.Attachments = new List<Attachment>() { attachment };
            return response;
        }
        private Activity CreateResponse(IActivity activity)
        {
            var response = ((Activity)activity).CreateReply();

            return response;
        }

        // Load attachment from file.
        private Attachment CreateAdaptiveCardAttachment()
        {
            // combine path for cross platform support
            string[] paths = { ".", "Cards", "welcomeCard.json" };
            string fullPath = Path.Combine(paths);
            var adaptiveCard = File.ReadAllText(fullPath);
            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCard),
            };
        }
    }
}