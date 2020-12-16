using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FixedMaths.Data
{
    public class ProcessedTableRepository : IProcessedTableRepository
    {
        public Dictionary<int, FixedPoint> CalculateAcosData()
        {
            var data = GetTableDto(Operation.Acos);

            return data.ToDictionary(x => x.Key, x => FixedPoint.FromExplicit(x.Value));
        }

        public Dictionary<FixedPoint, FixedPoint> CalculateAcoshData()
        {
            var data = GetTableDto(Operation.Acosh);

            return data.ToDictionary(x => FixedPoint.FromExplicit(x.Key), x => FixedPoint.FromExplicit(x.Value)
                                   , FixedPointEqualityComparer.Instance);
        }

        public Dictionary<int, FixedPoint> CalculateAsinData()
        {
            var data = GetTableDto(Operation.Asin);

            return data.ToDictionary(x => x.Key, x => FixedPoint.FromExplicit(x.Value));
        }

        public Dictionary<FixedPoint, FixedPoint> CalculateAsinhData()
        {
            var data = GetTableDto(Operation.Asinh);

            return data.ToDictionary(x => FixedPoint.FromExplicit(x.Key), x => FixedPoint.FromExplicit(x.Value)
                                   , FixedPointEqualityComparer.Instance);
        }

        public Dictionary<int, FixedPoint> CalculateAtanData()
        {
            var data = GetTableDto(Operation.Atan);

            return data.ToDictionary(x => x.Key, x => FixedPoint.FromExplicit(x.Value));
        }

        public Dictionary<int, FixedPoint> CalculateAtanhData()
        {
            var data = GetTableDto(Operation.Atanh);

            return data.ToDictionary(x => x.Key, x => FixedPoint.FromExplicit(x.Value));
        }

        public Dictionary<FixedPoint, FixedPoint> CalculateCosData()
        {
            var data = GetTableDto(Operation.Cos);

            return data.ToDictionary(x => FixedPoint.From(x.Key), x => FixedPoint.FromExplicit(x.Value)
                                   , FixedPointEqualityComparer.Instance);
        }

        public Dictionary<FixedPoint, FixedPoint> CalculateCoshData()
        {
            var data = GetTableDto(Operation.Cosh);

            return data.ToDictionary(x => FixedPoint.FromExplicit(x.Key), x => FixedPoint.FromExplicit(x.Value)
                                   , FixedPointEqualityComparer.Instance);
        }

        public Dictionary<FixedPoint, FixedPoint> CalculateSinData()
        {
            var data = GetTableDto(Operation.Sin);

            return data.ToDictionary(x => FixedPoint.From(x.Key), x => FixedPoint.FromExplicit(x.Value)
                                   , FixedPointEqualityComparer.Instance);
        }

        public Dictionary<FixedPoint, FixedPoint> CalculateSinhData()
        {
            var data = GetTableDto(Operation.Sinh);

            return data.ToDictionary(x => FixedPoint.FromExplicit(x.Key), x => FixedPoint.FromExplicit(x.Value)
                                   , FixedPointEqualityComparer.Instance);
        }

        public Dictionary<FixedPoint, FixedPoint> CalculateSqrtData()
        {
            var data = GetTableDto(Operation.Sqrt);

            return data.ToDictionary(x => FixedPoint.FromExplicit(x.Key), x => FixedPoint.FromExplicit(x.Value)
                                   , FixedPointEqualityComparer.Instance);
        }
        
        internal static ProcessedTableRepository Init()
        {
            var data     = new Dictionary<Operation, Dictionary<int, int>>();
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("FixedMath.FixedMaths.ProcessedTableData.math-lut.dat"))
            {
                using var sr = new StreamReader(stream);

                var lines    = new string[3];
                var readLine = 0;
                while (sr.Peek() >= 0)
                {
                    lines[readLine] =  sr.ReadLine();
                    readLine        += 1;

                    if (readLine != 3)
                        continue;

                    if (!int.TryParse(lines[0], out var operation))
                        throw new Exception("non-int value at index 0");

                    if (!int.TryParse(lines[1], out var key))
                        throw new Exception("non-int value at index 1");

                    if (!int.TryParse(lines[2], out var value))
                        throw new Exception("non-int value at index 2");

                    if (!data.ContainsKey((Operation) operation))
                        data[(Operation) operation] = new Dictionary<int, int>();

                    data[(Operation) operation][key] = value;

                    readLine = 0;
                }
            }

            return new ProcessedTableRepository(data);
        }

        readonly Dictionary<Operation, Dictionary<int, int>> _data;

        ProcessedTableRepository(Dictionary<Operation, Dictionary<int, int>> data) { _data = data; }

        Dictionary<int, int> GetTableDto(Operation operation)
        {
            if (_data.ContainsKey(operation))
                return _data[operation];
            throw new Exception($"Unable to find Table data for {operation}.");
        }
    }
}