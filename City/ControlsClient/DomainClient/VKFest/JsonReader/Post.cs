using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace City.ControlsClient.DomainClient.VKFest.JsonReader
{
    public class SentimentResult
    {
        public List<object> morphed_words { get; set; }
        public int spolarity { get; set; }
    }

    public class Likes
    {
        public int count { get; set; }
    }

    public class Comments
    {
        public int count { get; set; }
    }

    public class Photo
    {
        public string text { get; set; }
        public int post_id { get; set; }
        public string photo_75 { get; set; }
        public int date { get; set; }
        public string photo_130 { get; set; }
        public int width { get; set; }
        public string photo_807 { get; set; }
        public int height { get; set; }
        public string photo_604 { get; set; }
        public string access_key { get; set; }
        public int id { get; set; }
        public int owner_id { get; set; }
        public int album_id { get; set; }
        public string photo_1280 { get; set; }
        public int user_id { get; set; }
        public string photo_2560 { get; set; }
        public double? @long { get; set; }
        public double? lat { get; set; }
    }

    public class Link
    {
        public string description { get; set; }
        public string image_big { get; set; }
        public string url { get; set; }
        public string image_src { get; set; }
        public string title { get; set; }
    }

    public class Audio
    {
        public int genre_id { get; set; }
        public int lyrics_id { get; set; }
        public string url { get; set; }
        public int duration { get; set; }
        public int date { get; set; }
        public string artist { get; set; }
        public int id { get; set; }
        public int owner_id { get; set; }
        public string title { get; set; }
        public int album_id { get; set; }
    }

    public class Attachment
    {
        public Photo photo { get; set; }
        public string type { get; set; }
        public Link link { get; set; }
        public Audio audio { get; set; }
    }

    public class Reposts
    {
        public int count { get; set; }
    }

    public class Place
    {
        public int created { get; set; }
        public int id { get; set; }
        public string icon { get; set; }
        public double longitude { get; set; }
        public string city { get; set; }
        public double latitude { get; set; }
        public string country { get; set; }
        public string title { get; set; }
        public int checkins { get; set; }
        public int updated { get; set; }
        public int type { get; set; }
    }

    public class Geo
    {
        public string type { get; set; }
        public string coordinates { get; set; }
        public Place place { get; set; }
    }

    public class PostSource
    {
        public string type { get; set; }
    }

    public class Photo2
    {
        public int user_id { get; set; }
        public string text { get; set; }
        public string photo_807 { get; set; }
        public string photo_75 { get; set; }
        public int date { get; set; }
        public string photo_130 { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public string photo_604 { get; set; }
        public string access_key { get; set; }
        public int id { get; set; }
        public int owner_id { get; set; }
        public int album_id { get; set; }
        public string photo_1280 { get; set; }
    }

    public class Audio2
    {
        public int genre_id { get; set; }
        public int lyrics_id { get; set; }
        public string url { get; set; }
        public int duration { get; set; }
        public int date { get; set; }
        public string artist { get; set; }
        public int id { get; set; }
        public int owner_id { get; set; }
        public string title { get; set; }
    }

    public class Attachment2
    {
        public Photo2 photo { get; set; }
        public string type { get; set; }
        public Audio2 audio { get; set; }
    }

    public class CopyHistory
    {
        public string text { get; set; }
        public PostSource post_source { get; set; }
        public int from_id { get; set; }
        public List<Attachment2> attachments { get; set; }
        public int date { get; set; }
        public int id { get; set; }
        public int owner_id { get; set; }
        public string post_type { get; set; }
    }

    public class Post
    {
        public double @long { get; set; }
        public SentimentResult sentiment_result { get; set; }
        public string text { get; set; }
        public string photo_807 { get; set; }
        public string photo_75 { get; set; }
        public int date { get; set; }
        public string photo_130 { get; set; }
        public int width { get; set; }
        public string photo_url { get; set; }
        public int height { get; set; }
        public string photo_604 { get; set; }
        public int album_id { get; set; }
        public double lat { get; set; }
        public int id { get; set; }
        public int owner_id { get; set; }
        public string photo_1280 { get; set; }
        public string photo_2560 { get; set; }
        public int? post_id { get; set; }
        public string post_type { get; set; }
        public Likes likes { get; set; }
        public Comments comments { get; set; }
        public int from_id { get; set; }
        public List<Attachment> attachments { get; set; }
        public Reposts reposts { get; set; }
        public Geo geo { get; set; }
        public List<CopyHistory> copy_history { get; set; }
        public int signer_id { get; set; }
    }

    public class RootObject
    {
        public List<Post> posts { get; set; }
    }
}
