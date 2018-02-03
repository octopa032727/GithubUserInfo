using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GithubUserInfo.Jsons
{
    class Repository
    {
        public string name { get; set; }
        public string description { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public DateTime pushed_at { get; set; }
        public int? stargazers_count { get; set; }
        public string language { get; set; }
        public int? forks_count { get; set; }
    }
}
