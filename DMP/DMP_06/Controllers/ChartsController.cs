using Microsoft.AspNetCore.Mvc;
using DMP_06.Services;
using System.Globalization;

namespace DMP_06.Controllers;

[Route("charts")]
public sealed class ChartsController : Controller
{
    private readonly OnnxPredictor _predictor;

    public ChartsController(OnnxPredictor predictor)
    {
        _predictor = predictor;
    }

    [HttpGet("reg-true-vs-pred")]
    public IActionResult RegTrueVsPred()
    {
        if (!_predictor.IsReady)
            return StatusCode(503, new { error = "Model is loading" });

        var csvPath = Path.Combine(AppContext.BaseDirectory, "Data", "fifa_dataset_cleaned.csv");
        if (!System.IO.File.Exists(csvPath))
            return NotFound(new { error = "CSV not found in /Data (copy to output)." });

        var lines = System.IO.File.ReadAllLines(csvPath);
        if (lines.Length < 2)
            return BadRequest(new { error = "CSV is empty." });

        var header = SplitCsvLine(lines[0]);
        int idxValue = Array.IndexOf(header, "value_eur");
        int idxAge = Array.IndexOf(header, "age");
        int idxHeight = Array.IndexOf(header, "height_cm");
        int idxWeight = Array.IndexOf(header, "weight_kg");
        int idxOverall = Array.IndexOf(header, "overall");
        int idxPace = Array.IndexOf(header, "pace");
        int idxShooting = Array.IndexOf(header, "shooting");
        int idxPassing = Array.IndexOf(header, "passing");
        int idxDribbling = Array.IndexOf(header, "dribbling");
        int idxDefending = Array.IndexOf(header, "defending");
        int idxPhysic = Array.IndexOf(header, "physic");

        int[] needed = { idxValue, idxAge, idxHeight, idxWeight, idxOverall, idxPace, idxShooting, idxPassing, idxDribbling, idxDefending, idxPhysic };
        if (needed.Any(i => i < 0))
            return BadRequest(new { error = "CSV must contain: value_eur, age, height_cm, weight_kg, overall, pace, shooting, passing, dribbling, defending, physic." });

        var rnd = new Random(42);
        var sampleLines = lines.Skip(1).Where(l => !string.IsNullOrWhiteSpace(l)).OrderBy(_ => rnd.Next()).Take(600).ToList();

        var points = new List<object>(sampleLines.Count);
        double sumAbs = 0;
        double sumY = 0;
        double sumSqTot = 0;
        double sumSqRes = 0;

        var ys = new List<double>(sampleLines.Count);
        var preds = new List<double>(sampleLines.Count);

        foreach (var line in sampleLines)
        {
            var cols = SplitCsvLine(line);
            if (cols.Length != header.Length) continue;

            if (!TryFloat(cols[idxAge], out var age)) continue;
            if (!TryFloat(cols[idxHeight], out var height)) continue;
            if (!TryFloat(cols[idxWeight], out var weight)) continue;
            if (!TryFloat(cols[idxOverall], out var overall)) continue;
            if (!TryFloat(cols[idxPace], out var pace)) continue;
            if (!TryFloat(cols[idxShooting], out var shooting)) continue;
            if (!TryFloat(cols[idxPassing], out var passing)) continue;
            if (!TryFloat(cols[idxDribbling], out var dribbling)) continue;
            if (!TryFloat(cols[idxDefending], out var defending)) continue;
            if (!TryFloat(cols[idxPhysic], out var physic)) continue;
            if (!TryDouble(cols[idxValue], out var yTrue)) continue;

            var pf = new Models.PlayerFeatures
            {
                Age = age,
                HeightCm = height,
                WeightKg = weight,
                Overall = overall,
                Pace = pace,
                Shooting = shooting,
                Passing = passing,
                Dribbling = dribbling,
                Defending = defending,
                Physic = physic
            };

            if (!_predictor.IsReady)
            {
                return StatusCode(503, new { status = "loading" });
            }

            var yPred = _predictor.PredictPrice(pf);

            ys.Add(yTrue);
            preds.Add(yPred);
            points.Add(new { x = yTrue, y = yPred });
        }

        if (ys.Count < 10)
            return BadRequest(new { error = "Not enough valid rows for plotting." });

        var meanY = ys.Average();

        for (int i = 0; i < ys.Count; i++)
        {
            var y = ys[i];
            var p = preds[i];
            sumAbs += Math.Abs(y - p);
            sumSqRes += (y - p) * (y - p);
            sumSqTot += (y - meanY) * (y - meanY);
        }

        var mae = sumAbs / ys.Count;
        var r2 = 1.0 - (sumSqRes / sumSqTot);

        var minV = Math.Min(ys.Min(), preds.Min());
        var maxV = Math.Max(ys.Max(), preds.Max());

        return Json(new
        {
            mae,
            r2,
            points,
            line = new[] { new { x = minV, y = minV }, new { x = maxV, y = maxV } }
        });
    }

    private static bool TryFloat(string s, out float v) =>
        float.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out v);

    private static bool TryDouble(string s, out double v) =>
        double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out v);

    private static string[] SplitCsvLine(string line)
    {
        var res = new List<string>();
        var sb = new System.Text.StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (c == ',' && !inQuotes)
            {
                res.Add(sb.ToString());
                sb.Clear();
                continue;
            }

            sb.Append(c);
        }

        res.Add(sb.ToString());
        return res.ToArray();
    }
}