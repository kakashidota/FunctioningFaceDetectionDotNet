using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication9.Models;

namespace WebApplication9.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FaceController : ControllerBase
    {
        private readonly IConfiguration _config;
        
        const string RECOGNITION_MODEL = RecognitionModel.Recognition01;

    // POST api/face
    [HttpPost]
    public async Task<IActionResult> GetFaceDetails([FromForm] IFormFile file)
    {
        if (file == null)
        {
            return BadRequest();
        }
        string SUBSCRIPTION_KEY = _config.GetValue<string>("Keys:SUBSCRIPTION_KEY");
        string ENDPOINT = _config.GetValue<string>("Keys:ENDPOINT");
        IFaceClient client = Authenticate(ENDPOINT, SUBSCRIPTION_KEY);
        FaceImage dataToReturn = await DetectFaceExtract(client, file, RECOGNITION_MODEL);
        return Ok(dataToReturn);
    }

    public static IFaceClient Authenticate(string endpoint, string key)
    {
        return new FaceClient(new ApiKeyServiceClientCredentials(key)) { Endpoint = endpoint };
    }


        public static async Task<FaceImage> DetectFaceExtract(IFaceClient client, IFormFile file, string recognitionModel)
        {
            var stream = file.OpenReadStream();
            FaceImage faceImage = new FaceImage();

            IList<DetectedFace> detectedFaces;

            // Detect faces with all attributes from image.
            detectedFaces = await client.Face.DetectWithStreamAsync(stream,
                    returnFaceAttributes: new List<FaceAttributeType> { FaceAttributeType.Accessories, FaceAttributeType.Age,
            FaceAttributeType.Blur, FaceAttributeType.Emotion, FaceAttributeType.Exposure, FaceAttributeType.FacialHair,
            FaceAttributeType.Gender, FaceAttributeType.Glasses, FaceAttributeType.Hair, FaceAttributeType.HeadPose,
            FaceAttributeType.Makeup, FaceAttributeType.Noise, FaceAttributeType.Occlusion, FaceAttributeType.Smile },
                    recognitionModel: recognitionModel);

            faceImage.FileName = file.FileName;
            faceImage.FaceCount = detectedFaces.Count;

            // Parse all attributes of each detected face.
            List<Attributes> faceAttributes = new List<Attributes>();

            foreach (var face in detectedFaces.Select((value, i) => new { i, value }))
            {
                Attributes attributes = new Attributes();

                attributes.FaceNumber = face.i + 1;

                // Get bounding box of the faces
                attributes.Rectangle.Left = face.value.FaceRectangle.Left;
                attributes.Rectangle.Top = face.value.FaceRectangle.Top;
                attributes.Rectangle.Width = face.value.FaceRectangle.Width;
                attributes.Rectangle.Height = face.value.FaceRectangle.Height;

                // Get accessories of the faces
                List<Accessory> accessoriesList = (List<Accessory>)face.value.FaceAttributes.Accessories;
                int count = face.value.FaceAttributes.Accessories.Count;
                string accessory; string[] accessoryArray = new string[count];
                if (count == 0) { accessory = "NoAccessories"; }
                else
                {
                    for (int i = 0; i < count; ++i) { accessoryArray[i] = accessoriesList[i].Type.ToString(); }
                    accessory = string.Join(",", accessoryArray);
                }

                attributes.Accessories = accessory;

                // Get face other attributes
                attributes.Age = face.value.FaceAttributes.Age;
                attributes.Blur = face.value.FaceAttributes.Blur.BlurLevel.ToString();

                // Get emotion on the face
                string emotionType = string.Empty;
                double emotionValue = 0.0;
                Emotion emotion = face.value.FaceAttributes.Emotion;
                if (emotion.Anger > emotionValue) { emotionValue = emotion.Anger; emotionType = "Anger"; }
                if (emotion.Contempt > emotionValue) { emotionValue = emotion.Contempt; emotionType = "Contempt"; }
                if (emotion.Disgust > emotionValue) { emotionValue = emotion.Disgust; emotionType = "Disgust"; }
                if (emotion.Fear > emotionValue) { emotionValue = emotion.Fear; emotionType = "Fear"; }
                if (emotion.Happiness > emotionValue) { emotionValue = emotion.Happiness; emotionType = "Happiness"; }
                if (emotion.Neutral > emotionValue) { emotionValue = emotion.Neutral; emotionType = "Neutral"; }
                if (emotion.Sadness > emotionValue) { emotionValue = emotion.Sadness; emotionType = "Sadness"; }
                if (emotion.Surprise > emotionValue) { emotionType = "Surprise"; }
                attributes.Emotion = emotionType;

                // Get more face attributes
                attributes.Exposure = face.value.FaceAttributes.Exposure.ExposureLevel.ToString();

                attributes.FacialHair = $"{ string.Format("{0}", face.value.FaceAttributes.FacialHair.Moustache + face.value.FaceAttributes.FacialHair.Beard + face.value.FaceAttributes.FacialHair.Sideburns > 0 ? "Yes" : "No")}";
                attributes.Gender = face.value.FaceAttributes.Gender.ToString();
                attributes.Glasses = face.value.FaceAttributes.Glasses.ToString();

                // Get hair color
                Hair hair = face.value.FaceAttributes.Hair;
                string color = null;
                if (hair.HairColor.Count == 0) { if (hair.Invisible) { color = "Invisible"; } else { color = "Bald"; } }
                HairColorType returnColor = HairColorType.Unknown;
                double maxConfidence = 0.0f;
                foreach (HairColor hairColor in hair.HairColor)
                {
                    if (hairColor.Confidence <= maxConfidence) { continue; }
                    maxConfidence = hairColor.Confidence; returnColor = hairColor.Color; color = returnColor.ToString();
                }

                attributes.Hair = color;

                // Get more attributes
                attributes.HeadPose.Pitch = Math.Round(face.value.FaceAttributes.HeadPose.Pitch, 2);
                attributes.HeadPose.Roll = Math.Round(face.value.FaceAttributes.HeadPose.Roll, 2);
                attributes.HeadPose.Yaw = Math.Round(face.value.FaceAttributes.HeadPose.Yaw, 2);

                attributes.Makeup = $"{string.Format("{0}", (face.value.FaceAttributes.Makeup.EyeMakeup || face.value.FaceAttributes.Makeup.LipMakeup) ? "Yes" : "No")}";
                attributes.Noise = face.value.FaceAttributes.Noise.NoiseLevel.ToString();

                attributes.Occlusion.EyeOccluded = face.value.FaceAttributes.Occlusion.EyeOccluded ? true : false;
                attributes.Occlusion.ForeheadOccluded = face.value.FaceAttributes.Occlusion.ForeheadOccluded ? true : false;
                attributes.Occlusion.MouthOccluded = face.value.FaceAttributes.Occlusion.MouthOccluded ? true : false;

                attributes.Smile = face.value.FaceAttributes.Smile.ToString();

                faceImage.Attributes.Add(attributes);
            }
            return faceImage;
        }

        public FaceController(IConfiguration config)
    {
        _config = config;
    }


}
}
