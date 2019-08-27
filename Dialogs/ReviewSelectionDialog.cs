// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QnABot.Models;

namespace Tutorial.Bot
{
    /// <summary>
    /// Menu child the review questions make by the user
    /// </summary>
    public class ReviewSelectionDialog : ComponentDialog
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private const string DoneOption = "Terminar";
        private const string NoSelected = "value-noSelected";
        private const string PromptFirstQuestionBot = "Podes colocar a tua questão?";
        private const string PromptSecondStepQuestionBot = "Desculpa! Ainda não te pude ajudar, podes reformular a tua questão novamente?";
        private const string PromptThirdStepQuestionBot = "Desculpa!Ainda não te pude ajudar, mais uma tentativa podes reformular a tua questão novamente?";
        private const string UserInfo = "value-userInfo";
        // Define the company choices for the company selection prompt.
        private readonly string[] _satisfatoryOptions = new string[]
        {
           "Não"
        };
        /// <summary>
        /// Method to make a review with the choice of the end user
        /// </summary>
        /// <param name="configuration">The configuration get from appsettings</param>
        /// <param name="httpClientFactory">The Http client to make a request to QaNMaker</param>
        public ReviewSelectionDialog(IConfiguration configuration, IHttpClientFactory httpClientFactory )
            : base(nameof(ReviewSelectionDialog))
        {
            _configuration = configuration;
           
            _httpClientFactory = httpClientFactory;
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
                {
                    QuestionStepAsync,
                    SelectionStepAsync,
                    LoopStepAsync,
                }));

            InitialDialogId = nameof(WaterfallDialog);
        }
        /// <summary>
        /// This is the first step to the bot
        /// </summary>
        /// <param name="stepContext">WaterfallStepContext context</param>
        /// <param name="cancellationToken">Token</param>
        /// <returns>A message to the user with a TextPrompt</returns>
        private  async Task<DialogTurnResult> QuestionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Continue using the same selection list, if any, from the previous iteration of this dialog.
            var list = stepContext.Options as List<string> ?? new List<string>();
            stepContext.Values[NoSelected] = list;
            string userInput = stepContext.Context.Activity.Text;
            string splitedInput = stepContext.Context.Activity.Text;
            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text(PromptFirstQuestionBot) };
            if (stepContext.Context.Activity?.ChannelId=="msteams")
            {
                splitedInput = RemoveMentionInTeams(userInput, splitedInput);
            }


            if (list.Count is 0 && !string.IsNullOrEmpty(splitedInput) && !string.IsNullOrWhiteSpace(splitedInput))
            {
               
                return await stepContext.NextAsync(list, cancellationToken);
            }
           else if (list.Count is 1)
            {
                promptOptions.Prompt = MessageFactory.Text(PromptSecondStepQuestionBot);
                // Ask the user to enter their question.
                return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
            }
            else if (list.Count is 2)
            {
                promptOptions.Prompt = MessageFactory.Text(PromptThirdStepQuestionBot);
                // Ask the user to enter their question.
                return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
            }
            else
            {
                promptOptions.Prompt = MessageFactory.Text(PromptFirstQuestionBot);
                // Ask the user to enter their question.
                return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
            }

        }

        private static string RemoveMentionInTeams(string userInput, string splitedInput)
        {
            if (userInput.Contains("</at>"))
            {
                string[] splitInput = userInput.Split("</at>");
                splitedInput = splitInput.Length > 0 ? splitInput[1] : string.Empty;
            }

            return splitedInput;
        }

        /// <summary>
        /// This method step validates the choice from the end user
        /// </summary>
        /// <param name="stepContext">WaterfallStepContext from choice</param>
        /// <param name="cancellationToken">The cancellation token to the task</param>
        /// <returns></returns>
        private async Task<DialogTurnResult> SelectionStepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            // Continue using the same selection list, if any, from the previous iteration of this dialog.
            var list = stepContext.Options as List<string> ?? new List<string>();
            stepContext.Values[NoSelected] = list;
            await MakeRequestToGetAnswerAsync(stepContext, cancellationToken);
            // Create a prompt message.
            string message;
            if (list.Count is 0)
            {
                message = $"{stepContext.Context.Activity.From.Name } a resposta foi satisfatória, ou escolhe `{DoneOption}` para sair.";
            }
            else
            {
                message = $"{stepContext.Context.Activity.From.Name } seleccionaste **{list[0]}**. Queres colocar outra questão, " +
                     $"ou escolhe `{DoneOption}` para sair.";
            }

            // Create the list of options to choose from.
            var options = _satisfatoryOptions.ToList();
            options.Add(DoneOption);
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text(message),
                RetryPrompt = MessageFactory.Text("Por favor, valida a tua resposta e selecciona uma das opções!"),
                Choices = ChoiceFactory.ToChoices(options),
            };

            // Prompt the user for a choice.
            return await stepContext.PromptAsync(nameof(ChoicePrompt), promptOptions, cancellationToken);
        }

        /// <summary>
        /// To Get the Host Name
        /// </summary>
        /// <returns>The host name</returns>
        private string GetHostname()
        {
            var hostname = _configuration["QnAEndpointHostName"];
            if (!hostname.StartsWith("https://"))
            {
                hostname = string.Concat("https://", hostname);
            }

            if (!hostname.EndsWith("/qnamaker"))
            {
                hostname = string.Concat(hostname, "/qnamaker");
            }

            return hostname;
        }

        /// <summary>
        /// Make a request based in a question maked by the user
        /// </summary>
        /// <param name="stepContext">The context</param>
        /// <param name="cancellationToken">the cancelation Token</param>
        /// <returns>The answer to the question asked</returns>
        private async Task MakeRequestToGetAnswerAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values[UserInfo] = new UserProfile();
            string question = stepContext.Context.Activity.Text;
            var userProfile = (UserProfile)stepContext.Values[UserInfo];
            userProfile.Name = stepContext.Context.Activity.From.Name;
            if (stepContext.Context.Activity?.ChannelId == "msteams")
            {
                question = RemoveMentionInTeams(question, question);
                userProfile.Questions.Add(question);
            }
            else
            {
                userProfile.Questions.Add(question);
            }
            
            var httpClient = _httpClientFactory.CreateClient();
            var qnaMaker = new QnAMaker(new QnAMakerEndpoint
            {
                KnowledgeBaseId = _configuration["QnAKnowledgebaseId"],
                EndpointKey = _configuration["QnAAuthKey"],
                Host = GetHostname()
            },
          null,
          httpClient);

            // The actual call to the QnA Maker service.
            var response = await qnaMaker.GetAnswersAsync(stepContext.Context);
            if (response != null && response.Length > 0)
            {
                Activity reply = MessageFactory.Text($"{userProfile.Name} a resposta que encontrei foi:{Environment.NewLine} {response[0].Answer}");


                await stepContext.Context.SendActivityAsync(reply, cancellationToken);
            }
            else
            {
                
               
                Activity reply = MessageFactory.Text($"{userProfile.Name} não encontrei nenhuma resposta para a pergunta que fizeste.");
                await stepContext.Context.SendActivityAsync(reply, cancellationToken);
            }
        }
        /// <summary>
        /// The Loop Step to the limit of 3 No Questions give by the end user or a Done with the value true selected by the end user.
        /// </summary>
        /// <param name="stepContext">WaterfallStepContext </param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns></returns>
        private async Task<DialogTurnResult> LoopStepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            // Retrieve their selection list, the choice they made, and whether they chose to finish.
            var list = stepContext.Values[NoSelected] as List<string>;
           
            
            var choice = (FoundChoice)stepContext.Result;
            var done = choice.Value == DoneOption;
            if (!done)
            {
                // If they chose a company, add it to the list.
                list.Add(choice.Value);
                
                
            }

            if (done || list.Count >= 3)
            {
                if (!done)
                {
                    CreateTicketToSupport(stepContext, cancellationToken);
                }
               

                // If they're done, exit and return their list.
                return await stepContext.EndDialogAsync(list, cancellationToken);

            }
            else
            {
                // Otherwise, repeat this dialog, passing in the list from this iteration.
                return await stepContext.ReplaceDialogAsync(nameof(ReviewSelectionDialog), list, cancellationToken);
            }
        }
        /// <summary>
        /// Method to create the ticket if the end user give 3 no answers.
        /// And try get the mail from the end user to send the mail.
        /// </summary>
        /// <param name="stepContext">WaterfallStepContext</param>
        /// <param name="cancellationToken"></param>
        private void CreateTicketToSupport(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Create Ticket
            try
            {


                var userProfile = (UserProfile)stepContext.Values[UserInfo];
                ConnectorClient connector = new ConnectorClient(new Uri(stepContext.Context.Activity.ServiceUrl), _configuration["MicrosoftAppId"], _configuration["MicrosoftAppPassword"]);
                var members = connector.Conversations.GetConversationMembersWithHttpMessagesAsync(stepContext.Context.Activity.Conversation.Id).Result.Body;
                var member = members?.Where(s => s.Name == stepContext.Context.Activity.From.Name).FirstOrDefault();
                if (member!=null&&member.Properties.ContainsKey("email"))
                {
                    userProfile.Mail = member.Properties["email"].ToString();
                }



                var question = userProfile.Questions.Last();
                var ticket = new Ticket();
                ticket.Question = question;
                ticket.UserName = userProfile.Name;
                ticket.Id = userProfile.TeamsId;
                ticket.UserTeamsId = userProfile.TeamsId;
                ticket.UserTeamsMail = userProfile.Mail;
                // call sync
                OpenTicketAsync(ticket, stepContext, cancellationToken);


            }
            catch (System.Exception error)
            {
                throw error;
            }
        }
        /// <summary>
        /// Open the ticket and send it to the flow, also fill the values in the TicketFlow model.
        /// If fails send a message to the user with the response of the error.
        /// </summary>
        /// <param name="ticket"></param>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        private async void  OpenTicketAsync(Ticket ticket, WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            try
            {
                if (!string.IsNullOrEmpty(ticket.Question) && !string.IsNullOrEmpty(ticket.UserTeamsMail))
                {
                    await SendMail(ticket, stepContext, cancellationToken);

                }
                else if (!string.IsNullOrEmpty(ticket.Question))
                {
                    //Use this mail adress mourao.martins@gmail.com if the user not have one
                    ticket.UserTeamsMail = "mourao.martins@gmail.com";
                    await SendMail(ticket, stepContext, cancellationToken);
                }

            }
            catch (Exception ex)
            {
                //to log this error
               // throw ex;
            }
          
           


        }
        /// <summary>
        /// Send mail to support get configuration from appsettings
        /// </summary>
        /// <param name="ticket">The model</param>
        /// <param name="stepContext">The  context to give feedback to the end user.</param>
        /// <param name="cancellationToken">The cancelation toke</param>
        /// <returns>Error exception or succes task</returns>
        private async Task SendMail(Ticket ticket, WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            try
            {

                var flowUrl = _configuration["FlowUrl"];
                var supportMail = _configuration["SupportMail"];
                string bodyMail = "<div>";
                bodyMail += "<p>Lamentamos não ter conseguido responder à sua dúvida. Solicito que contacte um dos meus colegas do apoio Service Desk através dos canais abaixo indicados:</p>";
                bodyMail += "<p>Portal Tutorial: Serviços/ Helpdesk – Portal de Serviços</p>";
                bodyMail += "<ul>";
                bodyMail += "<li>Email <a href='mailto:%20ServiceDesk@tutorial.com/'>ServiceDesk@tutorial.com</a></li>";
                bodyMail += "<li>Ext. xxxx</li>";
                bodyMail += "<li>Tel. (+xxx xxx xxx xxx) – Portugal</li>";
                bodyMail += "</ul>";
                bodyMail += "</div>";

                string subjectMail = "Smart Assistant - contactos de suporte.";
                var ticketFlow = new TicketFlow(ticket.UserTeamsMail, subjectMail, bodyMail);
                var httpClient = _httpClientFactory.CreateClient();
                var client = new HttpClient { BaseAddress = new Uri(flowUrl) };
                string output = Newtonsoft.Json.JsonConvert.SerializeObject(ticketFlow);
                var response = client.PostAsync(flowUrl, new StringContent(output, Encoding.UTF8, "application/json")).Result;

                if (response.IsSuccessStatusCode)
                {
                    Activity reply = MessageFactory.Text("Contacta o suporte envie te um mail com os contactos.");
                    await stepContext.Context.SendActivityAsync(reply, cancellationToken);
                }
                else
                {
                    throw new Exception(response.ToString());
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }

   
}
