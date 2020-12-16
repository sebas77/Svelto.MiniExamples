using System.Collections.Generic;

namespace FixedMaths.Data
{
    public interface IProcessedTableRepository
    {
        Dictionary<int, FixedPoint>        CalculateAcosData();
        Dictionary<FixedPoint, FixedPoint> CalculateAcoshData();
        Dictionary<int, FixedPoint>        CalculateAsinData();
        Dictionary<FixedPoint, FixedPoint> CalculateAsinhData();
        Dictionary<int, FixedPoint>        CalculateAtanData();
        Dictionary<int, FixedPoint>        CalculateAtanhData();
        Dictionary<FixedPoint, FixedPoint> CalculateCosData();
        Dictionary<FixedPoint, FixedPoint> CalculateCoshData();
        Dictionary<FixedPoint, FixedPoint> CalculateSinData();
        Dictionary<FixedPoint, FixedPoint> CalculateSinhData();
        Dictionary<FixedPoint, FixedPoint> CalculateSqrtData();
    }
}