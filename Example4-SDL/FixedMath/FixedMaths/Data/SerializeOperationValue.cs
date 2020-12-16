using System;

namespace FixedMaths.Data
{
    public class SerializeOperationValue
    {
        public string OperationName { get; set; }
        public int    Key           { get; set; }
        public int    Value         { get; set; }

        public static SerializeOperationValue From(string csv)
        {
            var bits = csv.Split(',');

            if (bits.Length != 3)
                throw new Exception("received entry with incorrect entries");

            if (!int.TryParse(bits[1], out var key))
                throw new Exception("non-int value at index 1");

            if (!int.TryParse(bits[2], out var value))
                throw new Exception("non-int value at index 2");

            return new SerializeOperationValue
            {
                OperationName = bits[0]
              , Key           = key
              , Value         = value
            };
        }

        public override string ToString() { return $"{OperationName},{Key},{Value}"; }
    }
}