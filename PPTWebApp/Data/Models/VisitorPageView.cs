using System;

namespace PPTWebApp.Data.Models
{
    public class VisitorPageView
    {
        public int Id { get; set; }
        public Guid SessionId { get; set; }
        public string PageUrl { get; set; } = string.Empty;
        public DateTime ViewedAt { get; set; }
        public string? Referrer { get; set; }
    }
}
