using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Services.Communication.gRPC.Models.Event
{
    public class CommentReply
    {
        [JsonPropertyName("id_comment")]
        public string IdComment { get; set; }
        [JsonPropertyName("id_author")]
        public int IdAuthorComment { get; set; }
        [JsonPropertyName("name_author")]
        public string NameAuthorComment { get; set; }
        [JsonPropertyName("message")]
        public string Message { get; set; }

        public CommentReply(string idComment, int idAuthorComment, string nameAuthorComment, string message)
        {
            IdComment = idComment;
            IdAuthorComment = idAuthorComment;
            NameAuthorComment = nameAuthorComment;
            Message = message;
        }
    }
}
