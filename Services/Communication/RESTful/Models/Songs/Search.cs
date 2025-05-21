using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Communication.RESTful.Models.Search
{
    public class Search
    {
        public string? ArtistName { get; set; }
        public string? SongName { get; set; }

        public int? IDGenre { get; set; }
        public int? Limit { get; set; }
        public int? Offset { get; set; }

        public bool IsRandom { get; set; } = false;

    }
}
