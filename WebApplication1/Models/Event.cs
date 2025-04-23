using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class Event
    {
        [Key]
        public int Id { get; set; }

        public string ExternalEventId { get; set; }

        public string TypeOfEvent { get; set; }
        public string NameOfEvent { get; set; }

        [Url]
        public string UrlOfEvent { get; set; }

        [Url]
        public string PhotoUrl { get; set; }

        public DateTime SalesStartDate { get; set; }
        public DateTime SalesEndDate { get; set; }
        public DateTime StartOfEvent { get; set; }
        public DateTime EndOfEvent { get; set; }

        public string Address { get; set; }
        public string NameOfClub { get; set; }

        public string Classifications { get; set; }

        public ICollection<UserEvent> UserEvents { get; set; }
    }

}
