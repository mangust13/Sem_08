using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Globalization;
using DMP_06.Models;

namespace DMP_06.Services;

public sealed class OnnxPredictor : IDisposable
{
    private readonly string _regModelPath;
    private readonly string _clfModelPath;

    private InferenceSession? _regSession;
    private InferenceSession? _clfSession;

    private string? _regInputName;
    private string? _clfInputName;

    private string? _regOutputName;
    private string? _clfOutputName;

    private readonly object _lock = new();

    public bool IsReady { get; private set; } = false;

    public OnnxPredictor(string regModelPath, string clfModelPath)
    {
        _regModelPath = regModelPath;
        _clfModelPath = clfModelPath;

        Console.WriteLine("OnnxPredictor created (models not loaded yet)");
    }

    public async Task WarmUpAsync()
    {
        if (IsReady) return;

        await Task.Run(() =>
        {
            lock (_lock)
            {
                if (IsReady) return;

                Console.WriteLine("Loading ONNX models...");

                _regSession = new InferenceSession(_regModelPath);
                _clfSession = new InferenceSession(_clfModelPath);

                _regInputName = _regSession.InputMetadata.Keys.First();
                _clfInputName = _clfSession.InputMetadata.Keys.First();

                _regOutputName = _regSession.OutputMetadata.Keys.First();
                _clfOutputName = _clfSession.OutputMetadata.Keys.First();

                IsReady = true;

                Console.WriteLine("ONNX models loaded successfully.");
            }
        });
    }

    public double PredictPrice(PlayerFeatures x)
    {
        if (!IsReady || _regSession == null)
            throw new InvalidOperationException("Regression model not ready");

        var input = new DenseTensor<float>(x.ToArray(), new[] { 1, 10 });

        using var results = _regSession.Run(new[]
        {
            NamedOnnxValue.CreateFromTensor(_regInputName!, input)
        });

        var first = results.First(v => v.Name == _regOutputName).Value;

        if (first is DenseTensor<float> tf) return tf[0];
        if (first is DenseTensor<double> td) return td[0];

        throw new InvalidOperationException("Unsupported regression output tensor type.");
    }

    public string PredictPosition(PlayerFeatures x, string[]? labels = null)
    {
        if (!IsReady || _clfSession == null)
            throw new InvalidOperationException("Classifier model not ready");

        var input = new DenseTensor<float>(x.ToArray(), new[] { 1, 10 });

        using var results = _clfSession.Run(new[]
        {
            NamedOnnxValue.CreateFromTensor(_clfInputName!, input)
        });

        var first = results.First(v => v.Name == _clfOutputName).Value;

        if (first is DenseTensor<string> ts) return ts[0];

        if (first is DenseTensor<long> tl)
        {
            var idx = (int)tl[0];
            if (labels == null) return idx.ToString(CultureInfo.InvariantCulture);
            if (idx < 0 || idx >= labels.Length) return idx.ToString(CultureInfo.InvariantCulture);
            return labels[idx];
        }

        if (first is DenseTensor<int> ti)
        {
            var idx = ti[0];
            if (labels == null) return idx.ToString(CultureInfo.InvariantCulture);
            if (idx < 0 || idx >= labels.Length) return idx.ToString(CultureInfo.InvariantCulture);
            return labels[idx];
        }

        throw new InvalidOperationException("Unsupported classifier output tensor type.");
    }

    public void Dispose()
    {
        _regSession?.Dispose();
        _clfSession?.Dispose();
    }
}