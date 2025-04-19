using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WebApplication1.Models
{
    public class TicketmasterResponse
    {
        [JsonPropertyName("_embedded")]
        public Embedded Embedded { get; set; }
    }

    public class Embedded
    {
        [JsonPropertyName("events")]
        public List<EventObject> Events { get; set; }
    }

    public class EventObject
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("locale")]
        public string Locale { get; set; }

        [JsonPropertyName("images")]
        public List<Image> Images { get; set; }

        [JsonPropertyName("sales")]
        public Sales Sales { get; set; }

        [JsonPropertyName("dates")]
        public Dates Dates { get; set; }

        [JsonPropertyName("_embedded")]
        public EventEmbedded Embedded { get; set; } // np. venue info
    }

    public class Image
    {
        [JsonPropertyName("ratio")]
        public string Ratio { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }

    public class Sales
    {
        [JsonPropertyName("public")]
        public PublicSales Public { get; set; }
    }

    public class PublicSales
    {
        [JsonPropertyName("startDateTime")]
        public string StartDateTime { get; set; }

        [JsonPropertyName("endDateTime")]
        public string EndDateTime { get; set; }
    }

    public class Dates
    {
        [JsonPropertyName("start")]
        public Start Start { get; set; }

        [JsonPropertyName("end")]
        public End End { get; set; }

        [JsonPropertyName("timezone")]
        public string Timezone { get; set; }

        [JsonPropertyName("status")]
        public Status Status { get; set; }
    }

    public class Start
    {
        [JsonPropertyName("dateTime")]
        public string DateTime { get; set; }
    }

    public class End
    {
        [JsonPropertyName("dateTime")]
        public string DateTime { get; set; }
    }

    public class Status
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }
    }

    public class EventEmbedded
    {
        [JsonPropertyName("venues")]
        public List<Venue> Venues { get; set; }
    }

    public class Venue
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("address")]
        public Address Address { get; set; }
    }

    public class Address
    {
        [JsonPropertyName("line1")]
        public string Line1 { get; set; }
    }
}
