using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.IO;

namespace WebApplication1.ProjectSERVICES
{
    public class InvoiceDocument : IDocument
    {
        public string EventName { get; set; }
        public string EventDate { get; set; }
        public string EventAddress { get; set; }

        public InvoiceDocument(string eventName, string eventDate, string eventAddress)
        {
            EventName = eventName;
            EventDate = eventDate;
            EventAddress = eventAddress;
        }

        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Margin(50);
                    page.Size(PageSizes.A4);

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                    //page.Footer().Element(ComposeFooter);
                });
        }

        void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Potwierdzenie zakupu biletu")
                        .FontSize(20)
                        .SemiBold()
                        .FontColor(Colors.Blue.Darken2);
                });

                // Można tu wstawić logo z Resources, jeśli chcesz
                row.ConstantItem(100).Height(50).Placeholder();
            });
        }

        void ComposeContent(IContainer container)
        {
            container.PaddingVertical(30).Column(col =>
            {
                col.Spacing(15);

                col.Item().Text($"Wydarzenie: {EventName}")
                    .FontSize(14);

                col.Item().Text($"Address: {EventAddress}")
                    .FontSize(14);

                col.Item().Text($"Data wydarzenia: {EventDate}")
                    .FontSize(14);

                // Dodaj kod QR z pliku
                string qrPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "QR.PNG");

                if (File.Exists(qrPath))
                {
                    var qrBytes = File.ReadAllBytes(qrPath);

                    col.Item().PaddingTop(200).Row(row =>
                    {


                        row.RelativeItem(); // lewa pusta przestrzeń

                        row.ConstantItem(300).Height(300).Image(qrBytes).FitArea();

                        row.RelativeItem(); // prawa pusta przestrzeń
                    });
                }

                else
                {
                    col.Item().Text("Kod QR nie został znaleziony.")
                        .FontColor(Colors.Red.Medium);
                }

            });
        }

        //void ComposeFooter(IContainer container)
        //{
        //    container.AlignCenter().Text(x =>
        //    {
        //        x.Span("Strona ");
        //        x.CurrentPageNumber();
        //    });
        //}
    }
}
