namespace QnABot.Models
{
    internal class TicketFlow
    {
       
        public string EmailAdress { get; set; }
        public string EmailSubject { get; set; }
        public string EmailBody { get; set; }

        
           
        public TicketFlow()
        {
        }

        public TicketFlow(string emailadress, string emailSubject, string emailBody)
        {
            this.EmailAdress = emailadress;
            this.EmailSubject = emailSubject;
            this.EmailBody = emailBody;
        }
    }
}