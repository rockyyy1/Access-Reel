using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessReel
{
    //Posts refer to Blog Posts
    public class Posts
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Author { get; set; }
        public string? AuthorUrl { get; set; }
        public string? Url { get; set; }
        public DateTime? Date { get; set; }
        public string FormattedDate
        {
            get
            {
                if (Date.HasValue)
                {
                    return Date.Value.ToString("dd MMM, yyyy");
                }
                else
                {
                    return "";
                }
            }
        }
        public ImageSource? Image { get; set; }
        
        

        public class SortByTitle : IComparer<Posts>
        {
            bool ascending;
            public SortByTitle(bool ascending = false)
            {
                this.ascending = ascending;
            }

            public int Compare(Posts p1, Posts p2)
            {
                if (p1 == null || p2 == null)
                {
                    return 0;
                }
                return ascending ? string.Compare(p1.Title, p2.Title) : string.Compare(p2.Title, p1.Title);
            }
            
        }

        public class SortByAuthor : IComparer<Posts>
        {
            bool ascending;
            public SortByAuthor(bool ascending = false)
            {
                this.ascending = ascending;
            }
            public int Compare(Posts p1, Posts p2)
            {
                if (p1 == null || p2 == null)
                {
                    return 0;
                }
                return ascending ? string.Compare(p1.Author, p2.Author) : string.Compare(p2.Author,p1.Author);
            }
        }

        public class SortByDate : IComparer<Posts>
        {
            bool ascending;
            public SortByDate(bool ascending = false)
            {
                this.ascending = ascending;
            }
            public int Compare(Posts p1, Posts p2)
            {
                if (p1 == null || p2 == null)
                {
                    return 0;
                }

                return ascending ? DateTime.Compare((DateTime)p1.Date,(DateTime) p2.Date) : DateTime.Compare((DateTime)p2.Date, (DateTime)p1.Date);
            }
        }
    }
    
    public class Review : Posts
    {
        public string? ReviewScore { get; set; }
        public string? MemberReviewScore { get; set; }

        public class SortbyReviewScore() : IComparer<Review>
        {
            public int Compare(Review p1, Review p2)
                
            {
                if (p1 == null || p2 == null)
                {
                    return 0;
                }
                return p2.ReviewScore.CompareTo(p1.ReviewScore);
            }
        }

        public class SortByMemberScore : IComparer<Review>
        {
            public int Compare(Review p1, Review p2)
            {
                if (p1 == null || p2 == null)
                {
                    return 0;
                }
                return p2.MemberReviewScore.CompareTo(p1.MemberReviewScore);
            }
        }

    }
    
}
