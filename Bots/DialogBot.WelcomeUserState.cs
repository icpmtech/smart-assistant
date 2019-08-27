// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Dialogs;

namespace Tutorial.Bot
{
    public partial class DialogBot<T> where T : Dialog
    {
        // Stores User Welcome state for the conversation.
        // Stored in "Microsoft.Bot.Builder.ConversationState" and
        // backed by "Microsoft.Bot.Builder.MemoryStorage".

        public class WelcomeUserState
        {
            // Gets or sets whether the user has been welcomed in the conversation.
            public bool DidBotWelcomeUser { get; set; } = false;
        }
    }
}
