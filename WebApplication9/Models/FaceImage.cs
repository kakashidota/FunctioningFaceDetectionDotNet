using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication9.Models
{

    public class FaceImage
    {
        public FaceImage()
        {
            Attributes = new Collection<Attributes>();
        }
        public string FileName { get; set; }
        public int FaceCount { get; set; }
        public ICollection<Attributes> Attributes { get; set; }
    }

    public class Attributes
    {
        public Attributes()
        {
            Rectangle = new Rectangle();
            HeadPose = new HeadPose();
            Occlusion = new Occlusion();
        }
        public int FaceNumber { get; set; }
        public Rectangle Rectangle { get; set; }
        public string Accessories { get; set; }
        public double? Age { get; set; }
        public string Blur { get; set; }
        public string Emotion { get; set; }
        public string Exposure { get; set; }
        public string FacialHair { get; set; }
        public string Gender { get; set; }
        public string Glasses { get; set; }
        public string Hair { get; set; }
        public HeadPose HeadPose { get; set; }
        public string Makeup { get; set; }
        public string Noise { get; set; }
        public Occlusion Occlusion { get; set; }
        public string Smile { get; set; }
    }

    public class Rectangle
    {
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class HeadPose
    {
        public double Pitch { get; set; }
        public double Roll { get; set; }
        public double Yaw { get; set; }
    }

    public class Occlusion
    {
        public bool EyeOccluded { get; set; }
        public bool ForeheadOccluded { get; set; }
        public bool MouthOccluded { get; set; }
    }
}
