using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Services.Communication.RESTful.Models.Comment
{
    public class RespondCommentRequest
    {
        public string CommentId { get; set; }

        public string Message { get; set; }
    }
}
