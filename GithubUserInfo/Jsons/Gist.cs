using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GithubUserInfo.Jsons
{
    class Gist
    {
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string description { get; set; }
        public int? comments { get; set; }
        public string comments_url { get; set; }
    }
}
