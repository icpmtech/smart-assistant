// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Tutorial.Bot
{
    /// <summary>
    /// Class the Dialog
    /// </summary>
    public class TopLevelDialog : ComponentDialog
    {
        // Define a "done" response for the company selection prompt.
        private const string DoneOption = "done";

        // Define value names for values tracked inside the dialogs.
        private const string UserInfo = "value-userInfo";
        private readonly IConfiguration configuration;

        public TopLevelDialog(IConfiguration configuration, IHttpClientFactory httpClientFactory)
            : base(nameof(TopLevelDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ReviewSelectionDialog( configuration,  httpClientFactory));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                StartQuestionStepAsync,
                EndQuestionStepAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
            this.configuration = configuration;
        }

        /// <summary>
        /// First Step Dialog to the end user.
        /// </summary>
        /// <param name="stepContext">WaterfallStepContext </param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>A new  Review Dialog steps</returns>
        private async Task<DialogTurnResult> StartQuestionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
           
            stepContext.Values[UserInfo] = new UserProfile();
            var userProfile = (UserProfile)stepContext.Values[UserInfo];
            userProfile.Name = stepContext.Context.Activity.From.Name;
            userProfile.AadObjectId = stepContext.Context.Activity.From.AadObjectId;
            
            return await stepContext.BeginDialogAsync(nameof(ReviewSelectionDialog), userProfile, cancellationToken);
            
        }
        /// <summary>
        /// End question step to the end user
        /// </summary>
        /// <param name="stepContext">WaterfallStepContext</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>End Dialog to the end user</returns>
        private async Task<DialogTurnResult> EndQuestionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Set the user's company selection to what they entered in the review-selection dialog.
            var userProfile = (UserProfile)stepContext.Values[UserInfo];
            // Thank them for participating.
            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text("Obrigado."),
                cancellationToken);

            // Exit the dialog, returning the collected user information.
            return await stepContext.EndDialogAsync(stepContext.Values[UserInfo], cancellationToken);
        }
    }
}
