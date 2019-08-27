// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector.Teams;
using Microsoft.Bot.Builder.Dialogs;
using System;
using Microsoft.Bot.Schema.Teams;
using System.Text;

namespace Tutorial.Bot
{
    public class QnABot<T> : ActivityHandler where T : Dialog
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        protected readonly BotState ConversationState;
        protected readonly Dialog Dialog;
        protected readonly ILogger Logger;
        protected readonly BotState UserState;

        public QnABot(IConfiguration configuration, ILogger<T> logger, IHttpClientFactory httpClientFactory, ConversationState conversationState, UserState userState, T dialog)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            ConversationState = conversationState;
            UserState = userState;
            Dialog = dialog;
            Logger = logger;
        }
        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {

           

          


            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn.
            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await UserState.SaveChangesAsync(turnContext, false, cancellationToken);
        }
       
       
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Running dialog with Message Activity.");

            // Run the Dialog with the new message Activity.
            await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
           
        }

       
    }

    
}
