// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Tutorial.Bot
{
    // Defines a state property used to track information about the user.
    public class UserProfile
    {
        public string Name { get; set; }
      

        // The list of companies the user wants to review.
        public List<string> Questions { get; set; } = new List<string>();
        public int TeamsId { get; internal set; }
        public string Mail { get; internal set; }
        public string AadObjectId { get; internal set; }
    }
}