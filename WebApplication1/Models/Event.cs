namespace WebApplication1.Models
{
    public class Event
    {
        public int Id { get; set; }  
        public string TypeOfEvent { get; set; }  
        public string NameOfEvent { get; set; }
        public string Url { get; set; }
        public string PhotoUrl { get; set; }
        public DateTime SalesStartDate { get; set; }
        public DateTime SalesEndDate { get; set; }  
        public DateTime StartOfEvent { get; set; }  
        public DateTime EndOfEvent { get; set; }  
        public string Adress { get; set; }  
        public string NameOfClub { get; set; }  
        public string Classifications { get; set; }   

        // Klucz obcy do User (jedno wydarzenie jest przypisane do jednego użytkownika)
        public int UserId { get; set; }

        // Nawigacja do użytkownika
        public User User { get; set; }
    }

}
