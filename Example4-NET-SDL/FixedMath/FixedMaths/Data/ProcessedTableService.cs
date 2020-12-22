using System.Collections.ObjectModel;

namespace FixedMaths.Data
{
    public class ProcessedTableService
    {
        public readonly ReadOnlyDictionary<int, FixedPoint>        AcosData;
        public readonly ReadOnlyDictionary<FixedPoint, FixedPoint> AcoshData;
        public readonly ReadOnlyDictionary<int, FixedPoint>        AsinData;
        public readonly ReadOnlyDictionary<FixedPoint, FixedPoint> AsinhData;
        public readonly ReadOnlyDictionary<int, FixedPoint>        AtanData;
        public readonly ReadOnlyDictionary<int, FixedPoint>        AtanhData;
        public readonly ReadOnlyDictionary<FixedPoint, FixedPoint> CosData;
        public readonly ReadOnlyDictionary<FixedPoint, FixedPoint> CoshData;
        public readonly ReadOnlyDictionary<FixedPoint, FixedPoint> SinData;
        public readonly ReadOnlyDictionary<FixedPoint, FixedPoint> SinhData;
        public readonly ReadOnlyDictionary<FixedPoint, FixedPoint> SqrtData;
        public static   ProcessedTableService                      Instance { get; }

        static ProcessedTableService()
        {
            var repository = ProcessedTableRepository.Init();

            Instance = new ProcessedTableService(new ReadOnlyDictionary<int, FixedPoint>(repository.CalculateAcosData())
                                               , new ReadOnlyDictionary<FixedPoint, FixedPoint>(
                                                     repository.CalculateAcoshData())
                                               , new ReadOnlyDictionary<int, FixedPoint>(repository.CalculateAsinData())
                                               , new ReadOnlyDictionary<FixedPoint, FixedPoint>(
                                                     repository.CalculateAsinhData())
                                               , new ReadOnlyDictionary<int, FixedPoint>(repository.CalculateAtanData())
                                               , new ReadOnlyDictionary<int, FixedPoint>(
                                                     repository.CalculateAtanhData())
                                               , new ReadOnlyDictionary<FixedPoint, FixedPoint>(
                                                     repository.CalculateCosData())
                                               , new ReadOnlyDictionary<FixedPoint, FixedPoint>(
                                                     repository.CalculateCoshData())
                                               , new ReadOnlyDictionary<FixedPoint, FixedPoint>(
                                                     repository.CalculateSinData())
                                               , new ReadOnlyDictionary<FixedPoint, FixedPoint>(
                                                     repository.CalculateSinhData())
                                               , new ReadOnlyDictionary<FixedPoint, FixedPoint>(
                                                     repository.CalculateSqrtData()));
        }

        ProcessedTableService
        (ReadOnlyDictionary<int, FixedPoint> acosData, ReadOnlyDictionary<FixedPoint, FixedPoint> acoshData
       , ReadOnlyDictionary<int, FixedPoint> asinData, ReadOnlyDictionary<FixedPoint, FixedPoint> asinhData
       , ReadOnlyDictionary<int, FixedPoint> atanData, ReadOnlyDictionary<int, FixedPoint> atanhData
       , ReadOnlyDictionary<FixedPoint, FixedPoint> cosData, ReadOnlyDictionary<FixedPoint, FixedPoint> coshData
       , ReadOnlyDictionary<FixedPoint, FixedPoint> sinData, ReadOnlyDictionary<FixedPoint, FixedPoint> sinhData
       , ReadOnlyDictionary<FixedPoint, FixedPoint> sqrtData)
        {
            AcosData  = acosData;
            AcoshData = acoshData;
            AsinData  = asinData;
            AsinhData = asinhData;
            AtanData  = atanData;
            AtanhData = atanhData;
            CosData   = cosData;
            CoshData  = coshData;
            SinData   = sinData;
            SinhData  = sinhData;
            SqrtData  = sqrtData;
        }
    }
}