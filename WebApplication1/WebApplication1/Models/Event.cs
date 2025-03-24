namespace WebApplication1.Models
{
    public class Event
    {
        public int Id { get; set; }  
        public string Title { get; set; }  
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }  
        public string Adress { get; set; }  
        public string TypeOfMusic { get; set; }  
        public string Url { get; set; }  
        public string PhotoUrl { get; set; }  

        // Klucz obcy do User (jedno wydarzenie jest przypisane do jednego użytkownika)
        public int UserId { get; set; }

        // Nawigacja do użytkownika
        public User User { get; set; }
    }

}
