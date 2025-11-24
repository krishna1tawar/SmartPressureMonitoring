using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace Sensore_Project.Services
{
    // ============================
    //  ML.NET INPUT / OUTPUT TYPES
    // ============================

    public class PressureRiskInput
    {
        [LoadColumn(0)]
        public float Pressure { get; set; }
    }

    public class PressureRiskOutput
    {
        [ColumnName("Score")]
        public float RiskScore { get; set; }
    }

    public class RiskResult
    {
        public double RiskScore { get; set; }
        public string RiskLevel { get; set; } = "Low";
    }

    /// <summary>
    /// Handles:
    ///  - ML pipeline skeleton
    ///  - loading ML.NET model (optional)
    ///  - fallback rule-based risk
    /// </summary>
    public class RiskPredictionService
    {
        private readonly MLContext _mlContext;
        private ITransformer? _model;
        private PredictionEngine<PressureRiskInput, PressureRiskOutput>? _predictionEngine;

        private const string ModelPath = "MLModels/PressureRiskModel.zip";

        public RiskPredictionService()
        {
            _mlContext = new MLContext();
            TryLoadModel();
        }

        // ==============================================
        //  9) ML PIPELINE SKELETON  (for future training)
        // ==============================================

        public IEstimator<ITransformer> BuildPipeline()
        {
            var dataProcessPipeline =
                _mlContext.Transforms.CopyColumns("Label", nameof(PressureRiskInput.Pressure))
                .Append(_mlContext.Transforms.Concatenate("Features", nameof(PressureRiskInput.Pressure)));

            var trainer = _mlContext.Regression.Trainers.Sdca(
                labelColumnName: "Label",
                featureColumnName: "Features");

            return dataProcessPipeline.Append(trainer);
        }

        // ======================================
        // 10) MODEL LOADER (ML.NET integration)
        // ======================================

        private void TryLoadModel()
        {
            try
            {
                if (File.Exists(ModelPath))
                {
                    using var stream = File.OpenRead(ModelPath);
                    _model = _mlContext.Model.Load(stream, out _);

                    _predictionEngine = _mlContext.Model.CreatePredictionEngine<
                        PressureRiskInput, PressureRiskOutput>(_model);

                    Console.WriteLine("[ML] Model loaded.");
                }
                else
                {
                    Console.WriteLine("[ML] No model file found. Using rule-based logic.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ML] Error loading model: " + ex.Message);
                _model = null;
                _predictionEngine = null;
            }
        }

        // ===========================================
        // 11) PUBLIC API – SAFE FOR CONTROLLER USE
        // ===========================================

        public (double riskScore, string riskLevel) GetRisk(double pressure)
        {
            double score;

            if (_predictionEngine != null)
            {
                var input = new PressureRiskInput { Pressure = (float)pressure };
                var output = _predictionEngine.Predict(input);

                score = Math.Clamp(output.RiskScore, 0, 1);
            }
            else
            {
                score = CalculateRuleBasedRisk(pressure);
            }

            string level =
                score >= 0.75 ? "High" :
                score >= 0.40 ? "Medium" :
                "Low";

            return (score, level);
        }

        // Keep compatibility with old code
        public (double riskScore, string riskLevel) GetRiskScore(double p) => GetRisk(p);
        public (double riskScore, string riskLevel) CalculateRisk(double p) => GetRisk(p);

        // ===========================
        //  ASYNC VERSION (IMPORTANT)
        // ===========================
        public Task<RiskResult> PredictRiskAsync(double pressure)
        {
            var (score, level) = GetRisk(pressure);

            return Task.FromResult(new RiskResult
            {
                RiskScore = score,
                RiskLevel = level
            });
        }

        // ========================================
        // RULE-BASED RISK (fallback method)
        // ========================================

        private double CalculateRuleBasedRisk(double pressure)
        {
            const double minSafe = 80.0;
            const double maxSafe = 120.0;

            double diff = 0.0;

            if (pressure < minSafe)
                diff = minSafe - pressure;
            else if (pressure > maxSafe)
                diff = pressure - maxSafe;

            double normalized = Math.Min(diff / 80.0, 1.0);

            return normalized;
        }
    }
}   