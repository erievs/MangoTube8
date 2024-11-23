using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MangoTube8
{
    public class YouTubeModal
    {

        public class VideoDetails
        {
            public string Title { get; set; }
            public string Author { get; set; }
            public string VideoId { get; set; }
            public string Thumbnail { get; set; }
            public string Views { get; set; }
            public string Date { get; set; }
            public string Length { get; set; }
            public string AuthorProfilePictureUrl { get; set; }
        }

        public class SearchVideoDetail
        {
            public string VideoId { get; set; }
            public string Title { get; set; }
            public string Author { get; set; }
            public string Thumbnail { get; set; }
            public string Views { get; set; }
            public string Length { get; set; }
            public string Date { get; set; }
        }

        public class RealtedVideoDetail
        {
            public string VideoId { get; set; }
            public string Thumbnail { get; set; }
            public string Title { get; set; }
            public string Author { get; set; }
            public string Views { get; set; }
            public string Length { get; set; }
            public string Date { get; set; }
        }

        public class VideoComment {
            public string CommentId { get; set; }
            public string Avatar { get; set; }
            public string Author { get; set; }
            public string Date { get; set; }
            public string Content { get; set; }
            public string AuthorChannelId { get; set; }
        }

        public class AuthorDetails {
            public string AvatarUrl { get; set; }    
            public string Name { get; set; }
        }

        public class VideoDetailsTab
        {
            public string Title { get; set; }
            public string ViewCount { get; set; }
            public string Description { get; set; }
            public string UploadDate { get; set; }
            public string Subcribers { get; set; }
        }

        public class VideoAuthorDetails
        {
            public string VideoDetailsJson { get; set; }
            public string AuthorDetailsJson { get; set; }
        }

        public class LikesDislikesInfo
        {
            public string Likes { get; set; }
            public string Dislikes { get; set; }
        }

        public class VideoStreamFormat
        {
            public int Itag { get; set; }
            public string Url { get; set; }
            public string MimeType { get; set; }
            public int Bitrate { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public string Quality { get; set; }
            public string QualityLabel { get; set; }
            public string ProjectionType { get; set; }
            public int AverageBitrate { get; set; }
            public string AudioQuality { get; set; }
            public int ApproxDurationMs { get; set; }
            public int Fps { get; set; }
            public bool HighReplication { get; set; }
            public bool IsAudio { get; set; }  
        }




    }
}
