namespace DMP_06.Models;

public sealed class PlayerFeatures
{
    public float Age { get; set; }
    public float HeightCm { get; set; }
    public float WeightKg { get; set; }
    public float Overall { get; set; }
    public float Pace { get; set; }
    public float Shooting { get; set; }
    public float Passing { get; set; }
    public float Dribbling { get; set; }
    public float Defending { get; set; }
    public float Physic { get; set; }

    public float[] ToArray() => new[]
    {
        Age, HeightCm, WeightKg,
        Overall, Pace, Shooting, Passing, Dribbling, Defending, Physic
    };
}