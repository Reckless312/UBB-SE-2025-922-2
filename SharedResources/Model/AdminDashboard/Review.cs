namespace DataAccess.Model.AdminDashboard
{
    using System;
    public class Review
    {
        public Review()
        {
            this.Content = string.Empty;
        }

        public Review(int reviewId, Guid userId, int rating, string content, DateTime createdDate, int numberOfFlags = 0, bool isHidden = false)
        {
            this.ReviewId = reviewId;
            this.UserId = userId;
            this.Rating = rating;
            this.Content = content;
            this.CreatedDate = createdDate;
            this.NumberOfFlags = numberOfFlags;
            this.IsHidden = isHidden;
        }
        public int ReviewId { get; set; }

        public Guid UserId { get; set; }

        public int Rating { get; set; }

        public string Content { get; set; }

        public DateTime CreatedDate { get; set; }

        public int NumberOfFlags { get; set; }

        public bool IsHidden { get; set; }
    }
}