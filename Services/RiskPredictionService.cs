using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.EntityFrameworkCore;
using Sensore_Project.Models;
using Sensore_Project.Repositories;

namespace Sensore_Project.Services
{
    public class RiskPredictionService
    {
        private readonly ApplicationDbContext _context;
        private readonly RiskPredictionRepository _repository;
        private readonly MLContext _ml;

        public RiskPredictionService(ApplicationDbContext context, RiskPredictionRepository repo)
        {
            _context = context;
            _repository = repo;
            _ml = new MLContext();
        }

        // ML input class
        public class PressureInput
        {
            public float Pressure { get; set; }
        }

        // ML output class
        public class RiskOutput
        {
            [ColumnName("Score")]
            public float RiskScore { get; set; }
        }

        /// <summary>
        /// Train ML model on historical data and predict risk level.
        /// </summary>
        public async Task<RiskPrediction?> PredictRiskAsync(double pressure, CancellationToken ct = default)
        {
            // 1. Load historical pressure data
            var dataEntities = await _context.SensorData
                .OrderBy(s => s.Timestamp)
                .ToListAsync(ct);

            if (!dataEntities.Any())
                return null;

            // Convert DB data into ML input format
            var trainingData = dataEntities.Select(x => new PressureInput
            {
                Pressure = (float)x.Pressure
            });

            // Load training data into ML.NET DataView
            var trainingDataView = _ml.Data.LoadFromEnumerable(trainingData);

            // 2. Build ML pipeline
            var pipeline = _ml.Transforms.CopyColumns("Label", nameof(PressureInput.Pressure))
                .Append(_ml.Transforms.NormalizeMinMax(nameof(PressureInput.Pressure)))
                .Append(_ml.Transforms.Concatenate("Features", nameof(PressureInput.Pressure)))  // REQUIRED
                .Append(_ml.Regression.Trainers.FastTree());

            // 3. Train the model
            var model = pipeline.Fit(trainingDataView);

            // 4. Create predictor engine
            var predictor = _ml.Model.CreatePredictionEngine<PressureInput, RiskOutput>(model);

            // 5. Make prediction
            var prediction = predictor.Predict(new PressureInput { Pressure = (float)pressure });

            // 6. Convert score to human-readable risk level
            string level = prediction.RiskScore switch
            {
                < 0.33f => "Low",
                < 0.66f => "Medium",
                _ => "High"
            };

            // 7. Save result in database
            var result = new RiskPrediction
            {
                Pressure = pressure,
                RiskScore = prediction.RiskScore,
                RiskLevel = level,
                Timestamp = DateTime.UtcNow
            };

            await _repository.AddAsync(result, ct);

            return result;
        }
    }
}