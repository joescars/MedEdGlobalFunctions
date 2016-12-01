#r "System.Runtime"
#r "System.Threading.Tasks"
#r "System.IO"
#r "System.Linq"
#r "Newtonsoft.Json"

using System;
using System.Configuration;
using System.Runtime;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Face; 
using Microsoft.ProjectOxford.Face.Contract;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using Twilio;

public static void Run(Stream myBlob, string name, TraceWriter log)
{
    log.Info($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

    IFaceServiceClient faceServiceClient = new FaceServiceClient(ConfigurationManager.AppSettings["CognitiveServiceAPIKey"]);

    var recognizedFaces = faceServiceClient.DetectAsync(myBlob).Result;

    var faceRects = recognizedFaces.Select(face => face.FaceRectangle);

    log.Info($"Detected {faceRects.Count()} Faces");

    // Put into Array to send off
    Face[] faces = faceRects.Select(faceRect => new Face(
        faceRect.Width, 
        faceRect.Height, 
        faceRect.Left, 
        faceRect.Top)).ToArray();

    // TODO: Do something with the result

    // Create a message
    string msg = $"Notification! {faceRects.Count()} Faces Detected";

    // Setup Twilio
    string AccountSid = ConfigurationManager.AppSettings["TwilioAccountSID"];
    string AuthToken = ConfigurationManager.AppSettings["TwilioAuthToken"];
    var twilio = new TwilioRestClient(AccountSid, AuthToken);

    // Send to Twilio
    log.Info("Attempting to Send Message");
    var message = twilio.SendMessage(
        ConfigurationManager.AppSettings["TwilioFrom"],
        ConfigurationManager.AppSettings["TwilioTo"],
        msg
    );

    log.Info("Message sent with ID: " + message.Sid);

}

// Face Class to Return Data into
public class Face
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int Loc_x { get; set; }
    public int Loc_y { get; set; }

    public Face(int Width, int Height, int Loc_x, int Loc_y){
        this.Width = Width;
        this.Height = Height;
        this.Loc_x = Loc_x;
        this.Loc_y = Loc_y;
    }
}