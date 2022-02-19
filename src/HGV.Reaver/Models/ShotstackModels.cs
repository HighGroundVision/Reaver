using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace HGV.Reaver.Models
{
    public class Soundtrack
    {
        [JsonProperty("src")]
        public string Src { get; set; } = String.Empty;

        [JsonProperty("effect")]
        public string Effect { get; set; } = String.Empty;

        [JsonProperty("volume")]
        public int Volume { get; set; }
    }

    public class Font
    {
        [JsonProperty("src")]
        public string Src { get; set; } = String.Empty;
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
        public string Type { get; set; } = String.Empty;

        [JsonProperty("src")]
        public string Src { get; set; } = String.Empty;
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
        public string In { get; set; } = string.Empty;

        [JsonProperty("out")]
        public string Out { get; set; } = string.Empty;
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
        public Rotate Rotate { get; set; } = new Rotate();

        [JsonProperty("skew")]
        public Skew Skew { get; set; } = new Skew();

        [JsonProperty("flip")]
        public Flip Flip { get; set; } = new Flip();
    }

    public class Clip
    {
        [JsonProperty("asset")]
        public Asset Asset { get; set; } = new Asset();

        [JsonProperty("start")]
        public double Start { get; set; }

        [JsonProperty("length")]
        public double Length { get; set; }

        [JsonProperty("fit")]
        public string Fit { get; set; } = string.Empty;

        [JsonProperty("scale")]
        public int Scale { get; set; }

        [JsonProperty("position")]
        public string Position { get; set; } = string.Empty;

        [JsonProperty("offset")]
        public Offset Offset { get; set; } = new Offset();

        [JsonProperty("transition")]
        public Transition Transition { get; set; } = new Transition();

        [JsonProperty("effect")]
        public string Effect { get; set; } = string.Empty;

        [JsonProperty("filter")]
        public string Filter { get; set; } = string.Empty;

        [JsonProperty("opacity")]
        public int Opacity { get; set; }

        [JsonProperty("transform")]
        public Transform Transform { get; set; } = new Transform();
    }

    public class Track
    {
        [JsonProperty("clips")]
        public List<Clip> Clips { get; set; } = new List<Clip>();
    }

    public class Timeline
    {
        [JsonProperty("soundtrack")]
        public Soundtrack Soundtrack { get; set; } = new Soundtrack();

        [JsonProperty("background")]
        public string Background { get; set; } = String.Empty;

        [JsonProperty("fonts")]
        public List<Font> Fonts { get; set; } = new List<Font>();

        [JsonProperty("tracks")]
        public List<Track> Tracks { get; set; } = new List<Track>();

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
        public string Provider { get; set; } = String.Empty;

        [JsonProperty("exclude")]
        public bool Exclude { get; set; }
    }

    public class Output
    {
        [JsonProperty("format")]
        public string Format { get; set; } = String.Empty;

        [JsonProperty("resolution")]
        public string Resolution { get; set; } = String.Empty;

        [JsonProperty("aspectRatio")]
        public string AspectRatio { get; set; } = String.Empty;

        [JsonProperty("size")]
        public Size Size { get; set; } = new Size();

        [JsonProperty("fps")]
        public int Fps { get; set; }

        [JsonProperty("scaleTo")]
        public string ScaleTo { get; set; } = String.Empty;

        [JsonProperty("quality")]
        public string Quality { get; set; } = String.Empty;

        [JsonProperty("repeat")]
        public bool Repeat { get; set; }

        [JsonProperty("range")]
        public Range Range { get; set; } = new Range();

        [JsonProperty("poster")]
        public Poster Poster { get; set; } = new Poster();

        [JsonProperty("thumbnail")]
        public Thumbnail Thumbnail { get; set; } = new Thumbnail();

        [JsonProperty("destinations")]
        public List<Destination> Destinations { get; set; } = new List<Destination>();
    }

    public class Merge
    {
        [JsonProperty("find")]
        public string Find { get; set; } = String.Empty;

        [JsonProperty("replace")]
        public string Replace { get; set; } = String.Empty;
    }

    public class Edit
    {
        [JsonProperty("timeline")]
        public Timeline Timeline { get; set; } = new Timeline();

        [JsonProperty("output")]
        public Output Output { get; set; } = new Output();

        [JsonProperty("merge")]
        public List<Merge> Merge { get; set; } = new List<Merge>();

        [JsonProperty("callback")]
        public string Callback { get; set; } = String.Empty;

        [JsonProperty("disk")]
        public string Disk { get; set; } = String.Empty;
    }

    public class Queued
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; } = String.Empty;

        [JsonProperty("response")]
        public QueuedResponse Response { get; set; } = new QueuedResponse();
    }

    public class QueuedResponse
    {
        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

        [JsonProperty("id")]
        public Guid Id { get; set; }
    }

    public class Render
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

        [JsonProperty("response")]
        public RenderResponse Response { get; set; } = new RenderResponse();
    }


    public class RenderResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("owner")]
        public string Owner { get; set; } = String.Empty;

        [JsonProperty("plan")]
        public string Plan { get; set; } = String.Empty;

        [JsonProperty("status")]
        public string Status { get; set; } = String.Empty;

        [JsonProperty("error")]
        public string Error { get; set; } = String.Empty;

        [JsonProperty("duration")]
        public double Duration { get; set; }

        [JsonProperty("renderTime")]
        public double RenderTime { get; set; }

        [JsonProperty("url")]
        public Uri? Url { get; set; }

        [JsonProperty("poster")]
        public Uri? Poster { get; set; }

        [JsonProperty("thumbnail")]
        public Uri? Thumbnail { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("updated")]
        public DateTime Updated { get; set; }
    }


}
