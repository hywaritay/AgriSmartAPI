namespace AgriSmartAPI.DTO;

public class FlaskResult
{
    public List<FlaskDetection> Detections { get; set; }
    public double Confidence { get; set; }
}
