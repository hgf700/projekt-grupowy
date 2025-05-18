﻿using System;
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
        public DateTime StartOfEvent { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string NameOfClub { get; set; }
        public ICollection<UserEvent> UserEvents { get; set; }
    }

}
