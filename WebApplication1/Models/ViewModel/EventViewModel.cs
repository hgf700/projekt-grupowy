using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models.ViewModel
{
    public class EventViewModel
    {
        public int? Id { get; set; }  // Nullable dla tworzenia i edycji

        [Required(ErrorMessage = "Nazwa wydarzenia jest wymagana.")]
        public string NameOfEvent { get; set; }

        [Required(ErrorMessage = "Typ wydarzenia jest wymagany.")]
        public string TypeOfEvent { get; set; }

        [Url(ErrorMessage = "Nieprawidłowy adres URL.")]
        public string UrlOfEvent { get; set; }

        [Url(ErrorMessage = "Nieprawidłowy URL zdjęcia.")]
        public string PhotoUrl { get; set; }

        [Display(Name = "Data rozpoczęcia sprzedaży")]
        [DataType(DataType.DateTime)]
        public DateTime? SalesStartDate { get; set; }

        [Display(Name = "Data zakończenia sprzedaży")]
        [DataType(DataType.DateTime)]
        public DateTime? SalesEndDate { get; set; }

        [Display(Name = "Początek wydarzenia")]
        [DataType(DataType.DateTime)]
        public DateTime? StartOfEvent { get; set; }

        [Display(Name = "Koniec wydarzenia")]
        [DataType(DataType.DateTime)]
        public DateTime? EndOfEvent { get; set; }

        [Display(Name = "Adres")]
        public string Address { get; set; }

        [Display(Name = "Nazwa klubu")]
        public string NameOfClub { get; set; }

        public string Classifications { get; set; }

        public string ExternalEventId { get; set; }  // Opcjonalne

        // Możesz dodać pola na potrzeby selekcji użytkownika, jeśli jest potrzebna np. w formularzu tworzenia:
        public int? SelectedUserId { get; set; }
    }
}
