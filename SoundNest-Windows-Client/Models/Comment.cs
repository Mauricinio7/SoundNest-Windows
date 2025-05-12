using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundNest_Windows_Client.Models
{
    public class Comment
    {
        public string Username { get; set; } = "";
        public string Text { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public bool IsMine { get; set; }
        public string CommentId { get; set; } = "";
    }

}
