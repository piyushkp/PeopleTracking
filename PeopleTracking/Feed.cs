using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeopleTracking
{
    public class Feed
    {
        public Guid FaceID { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public int TimeSpent { get; set; }

        public string Gender { get; set; }

        public string Age { get; set; }

        public string FaceType { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Time { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

    }
}
