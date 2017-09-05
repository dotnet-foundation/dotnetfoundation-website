using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnetfoundation.Models
{
    public class MeetupFeed
    {
        public MeetupFeed()
        {
            Events = new List<MeetupEvent>();
        }

        public List<MeetupEvent> Events { get; set; }
    }
}
