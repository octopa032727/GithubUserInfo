using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GithubUserInfo.Jsons
{
    class Event
    {
        public string type { get; set; }
        public EventRepository repo { get; set; }
        public DateTime created_at { get; set; }
    }

    class EventRepository
    {
        public string name { get; set; }
    }
}
