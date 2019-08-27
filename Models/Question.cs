namespace QnABot.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public int UserTeamsId { get; set; }
        public string UserTeamsMail { get; set; }
        public string Question { get; set; }
    }
}