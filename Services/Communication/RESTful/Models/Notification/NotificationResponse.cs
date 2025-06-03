using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Services.Communication.RESTful.Models.Notification
{
    public class NotificationResponse
    {
        [JsonPropertyName("_id")]
        public string? Id { get; set; }
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("sender")]
        public string? Sender { get; set; }

        [JsonPropertyName("user_id")]
        public int? UserId { get; set; }

        [JsonPropertyName("user")]
        public string? User { get; set; }

        [JsonPropertyName("notification")]
        public string? Notification { get; set; }

        [JsonPropertyName("relevance")]
        public Relevance? Relevance { get; set; }

        [JsonPropertyName("read")]
        public bool? Read { get; set; }

        [JsonPropertyName("createdAt")]
        public string? CreatedAt { get; set; }

        [JsonPropertyName("__v")]
        public int? Version { get; set; }
    }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Relevance
    {
        [JsonPropertyName("low")]
        low,

        [JsonPropertyName("medium")]
        medium,

        [JsonPropertyName("high")]
        high
    }
}
