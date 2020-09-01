using System;
using System.Collections.Generic;

namespace HooplaNewReleaseCheck
{
    public class DigitalBook : IComparable<DigitalBook>
    {
        public int TitleId { get; set; }
        public string Title { get; set; }
        public int KindId { get; set; }
        public string Kind { get; set; }
        public string ArtistName { get; set; }
        public string ArtKey { get; set; }
        public long ReleaseDate { get; set; }

        private static DateTime BaseDate
        {
            get => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }

        public string ReleaseDateFormatted
        {
            get => BaseDate.AddMilliseconds(ReleaseDate).ToLocalTime().ToShortDateString();
        }

        public int CompareTo(DigitalBook otherBook)
        {
            if (otherBook == null)
                return 1;
            else
                return this.Title.CompareTo(otherBook.Title);
        }
    }
}
