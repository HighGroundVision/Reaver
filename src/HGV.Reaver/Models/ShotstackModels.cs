using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace HGV.Reaver.Models
{
    public class Soundtrack
    {
        [JsonProperty("src")]
        public string Src { get; set; }

        [JsonProperty("effect")]
        public string Effect { get; set; }

        [JsonProperty("volume")]
        public int Volume { get; set; }
    }

    public class Font
    {
        [JsonProperty("src")]
        public string Src { get; set; }
    }

    public class Crop
    {
        [JsonProperty("top")]
        public double Top { get; set; }

        [JsonProperty("bottom")]
        public double Bottom { get; set; }

        [JsonProperty("left")]
        public int Left { get; set; }

        [JsonProperty("right")]
        public int Right { get; set; }
    }

    public class Asset
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("src")]
        public string Src { get; set; }
    }

    public class ImageAsset : Asset
    {
        public ImageAsset(string url)
        {
            this.Type = "image";
            this.Src = url;
        }
        public ImageAsset(Uri url)
        {
            this.Type = "image";
            this.Src = url.ToString();
        }
    }

    public class Offset
    {
        [JsonProperty("x")]
        public double X { get; set; }

        [JsonProperty("y")]
        public double Y { get; set; }
    }

    public class Transition
    {
        [JsonProperty("in")]
        public string In { get; set; }

        [JsonProperty("out")]
        public string Out { get; set; }
    }

    public class Rotate
    {
        [JsonProperty("angle")]
        public int Angle { get; set; }
    }

    public class Skew
    {
        [JsonProperty("x")]
        public double X { get; set; }

        [JsonProperty("y")]
        public double Y { get; set; }
    }

    public class Flip
    {
        [JsonProperty("horizontal")]
        public bool Horizontal { get; set; }

        [JsonProperty("vertical")]
        public bool Vertical { get; set; }
    }

    public class Transform
    {
        [JsonProperty("rotate")]
        public Rotate Rotate { get; set; }

        [JsonProperty("skew")]
        public Skew Skew { get; set; }

        [JsonProperty("flip")]
        public Flip Flip { get; set; }
    }

    public class Clip
    {
        [JsonProperty("asset")]
        public Asset Asset { get; set; }

        [JsonProperty("start")]
        public double Start { get; set; }

        [JsonProperty("length")]
        public double Length { get; set; }

        [JsonProperty("fit")]
        public string Fit { get; set; }

        [JsonProperty("scale")]
        public int Scale { get; set; }

        [JsonProperty("position")]
        public string Position { get; set; }

        [JsonProperty("offset")]
        public Offset Offset { get; set; }

        [JsonProperty("transition")]
        public Transition Transition { get; set; }

        [JsonProperty("effect")]
        public string Effect { get; set; }

        [JsonProperty("filter")]
        public string Filter { get; set; }

        [JsonProperty("opacity")]
        public int Opacity { get; set; }

        [JsonProperty("transform")]
        public Transform Transform { get; set; }
    }

    public class Track
    {
        [JsonProperty("clips")]
        public List<Clip> Clips { get; set; }
    }

    public class Timeline
    {
        [JsonProperty("soundtrack")]
        public Soundtrack Soundtrack { get; set; }

        [JsonProperty("background")]
        public string Background { get; set; }

        [JsonProperty("fonts")]
        public List<Font> Fonts { get; set; }

        [JsonProperty("tracks")]
        public List<Track> Tracks { get; set; }

        [JsonProperty("cache")]
        public bool Cache { get; set; }
    }

    public class Size
    {
        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }
    }

    public class Range
    {
        [JsonProperty("start")]
        public int Start { get; set; }

        [JsonProperty("length")]
        public int Length { get; set; }
    }

    public class Poster
    {
        [JsonProperty("capture")]
        public int Capture { get; set; }
    }

    public class Thumbnail
    {
        [JsonProperty("capture")]
        public int Capture { get; set; }

        [JsonProperty("scale")]
        public double Scale { get; set; }
    }

    public class Destination
    {
        [JsonProperty("provider")]
        public string Provider { get; set; }

        [JsonProperty("exclude")]
        public bool Exclude { get; set; }
    }

    public class Output
    {
        [JsonProperty("format")]
        public string Format { get; set; }

        [JsonProperty("resolution")]
        public string Resolution { get; set; }

        [JsonProperty("aspectRatio")]
        public string AspectRatio { get; set; }

        [JsonProperty("size")]
        public Size Size { get; set; }

        [JsonProperty("fps")]
        public int Fps { get; set; }

        [JsonProperty("scaleTo")]
        public string ScaleTo { get; set; }

        [JsonProperty("quality")]
        public string Quality { get; set; }

        [JsonProperty("repeat")]
        public bool Repeat { get; set; }

        [JsonProperty("range")]
        public Range Range { get; set; }

        [JsonProperty("poster")]
        public Poster Poster { get; set; }

        [JsonProperty("thumbnail")]
        public Thumbnail Thumbnail { get; set; }

        [JsonProperty("destinations")]
        public List<Destination> Destinations { get; set; }
    }

    public class Merge
    {
        [JsonProperty("find")]
        public string Find { get; set; }

        [JsonProperty("replace")]
        public string Replace { get; set; }
    }

    public class Edit
    {
        [JsonProperty("timeline")]
        public Timeline Timeline { get; set; }

        [JsonProperty("output")]
        public Output Output { get; set; }

        [JsonProperty("merge")]
        public List<Merge> Merge { get; set; }

        [JsonProperty("callback")]
        public string Callback { get; set; }

        [JsonProperty("disk")]
        public string Disk { get; set; }
    }

    public class Queued
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("response")]
        public QueuedResponse Response { get; set; }
    }

    public class QueuedResponse
    {
        [JsonProperty("message")]
        public string Message { get; set; }
        
        [JsonProperty("id")]
        public Guid Id { get; set; }
    }

    public class Render
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("response")]
        public RenderResponse Response { get; set; }
    }


    public class RenderResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("owner")]
        public string Owner { get; set; }

        [JsonProperty("plan")]
        public string Plan { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("duration")]
        public double Duration { get; set; }

        [JsonProperty("renderTime")]
        public double RenderTime { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("poster")]
        public Uri Poster { get; set; }

        [JsonProperty("thumbnail")]
        public Uri Thumbnail { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("updated")]
        public DateTime Updated { get; set; }
    }


}
