using DMP_06.Models;
using Microsoft.AspNetCore.Mvc;
using DMP_06.Services;

namespace DMP_06.Controllers;

[Route("predict")]
public sealed class PredictController : Controller
{
    private readonly OnnxPredictor _predictor;

    public PredictController(OnnxPredictor predictor)
    {
        _predictor = predictor;
    }

    [HttpPost("all")]
    public IActionResult PredictAll([FromBody] PlayerFeatures input)
    {
        if (!_predictor.IsReady)
        {
            return StatusCode(503, new { status = "loading" });
        }

        var price = _predictor.PredictPrice(input);
        var position = _predictor.PredictPosition(input);

        return Json(new
        {
            predictedValueEur = price,
            predictedPosition = position
        });
    }
}