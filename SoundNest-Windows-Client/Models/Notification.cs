using Services.Communication.RESTful.Models.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundNest_Windows_Client.Models
{
    public class Notification
    {
        public string Title { get; set; }
        public string Sender { get; set; }
        public string Message { get; set; }
        public Relevance Priority { get; set; }
        public string Id { get; set; }

        public Notification(string title, string sender, string message, Relevance priority, string id)
        {
            Title = title;
            Sender = sender;
            Message = message;
            Priority = priority;
            Id = id;
        }
    }
}
