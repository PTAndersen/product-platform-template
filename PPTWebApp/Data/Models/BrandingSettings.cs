namespace PPTWebApp.Data.Models
{
    public class BrandingSettings
    {
        public class ProfileSettings
        {
            public required string CompanyName { get; set; }
            public required string LogoUrl { get; set; }
            public required string ThemeColor { get; set; }
            public required string FaviconPath { get; set; }
        }
    }

}
