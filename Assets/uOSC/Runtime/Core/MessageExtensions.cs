using System;
using System.Diagnostics;
using UnityEngine;
using uOSC;

namespace uOSC
{
    public static class MessageExtensions
    {
        private static bool CheckValueType(this Message self, int index, OCSValue.ValueType expectType)
        {
            if (index < 0 || index >= self.values.Length) return false;
            var v = self.values[index];
            return v.Type == expectType;
        }
        
        private static void CheckValueTypeThrow(this Message self, int index, OCSValue.ValueType expectType)
        {
            if (index < 0 || index >= self.values.Length)
                throw new IndexOutOfRangeException($"Message {self.address}: value index {index} is out of range. size is {self.values.Length}");
            var v = self.values[index];
            if (v.Type != expectType)
                throw new ArgumentException($"Message {self.address}: value index {index} type check failed. expect '{expectType}', but got '{v.Type}'");
        }
        
        public static OCSValue GetValue(this Message self, int index)
        {
            if (index < 0 || index >= self.values.Length)
                throw new IndexOutOfRangeException($"value index {index} is out of range. size is {self.values.Length}");

            return self.values[index];
        }
        
        public static OCSValue GetValue(this Message self, int index, OCSValue.ValueType expectType)
        {
            if (index < 0 || index >= self.values.Length)
                throw new IndexOutOfRangeException($"value index {index} is out of range. size is {self.values.Length}");

            self.CheckValueType(index, expectType);
            return self.values[index];
        }
        
        public static bool TryGetValue(this Message self, int index, out OCSValue value)
        {
            value = default;
            if (index < 0 || index >= self.values.Length) return false;

            value = self.values[index];
            return true;
        }
        
        public static bool TryGetValue(this Message self, int index, out OCSValue value, OCSValue.ValueType expectType)
        {
            value = default;
            if (index < 0 || index >= self.values.Length) return false;

            if (!self.CheckValueType(index, expectType)) return false;
            value = self.values[index];
            return true;
        }
        
        public static string GetString(this Message self, int index)
        {
            return GetValue(self, index, OCSValue.ValueType.String).StringValue;
        }  
        
        public static string GetStringWithDefault(this Message self, int index, string defaultValue = "")
        {
            return TryGetValue(self, index, out var v, OCSValue.ValueType.String) ? v.StringValue : defaultValue;
        } 
        
        public static int GetInt(this Message self, int index)
        {
            return GetValue(self, index, OCSValue.ValueType.Int).IntValue;
        }  
        
        public static int GetIntWithDefault(this Message self, int index, int defaultValue = default)
        {
            return TryGetValue(self, index, out var v, OCSValue.ValueType.Int) ? v.IntValue : defaultValue;
        } 
        
        public static float GetFloat(this Message self, int index)
        {
            return GetValue(self, index, OCSValue.ValueType.Float).IntValue;
        }  
        
        public static float GetFloatWithDefault(this Message self, int index, float defaultValue = default)
        {
            return TryGetValue(self, index, out var v, OCSValue.ValueType.Float) ? v.IntValue : defaultValue;
        }
        
        public static Vector3 GetVector3(this Message self, int index)
        {
            var (x, y, z) = GetValues3(self, index, OCSValue.ValueType.Float);
            return new Vector3(x, y, z);
        }
        
        public static Vector3? GetVector3Nullable(this Message self, int index)
        {
            return TryGetValues3(self, index, out var v, OCSValue.ValueType.Float) ? new Vector3(v.Item1, v.Item2, v.Item3) : null;
        }

        public static Vector3 GetVector3WithDefault(this Message self, int index, Vector3 defaultValue)
        {
            return TryGetValues3(self, index, out var v, OCSValue.ValueType.Float) ? new Vector3(v.Item1, v.Item2, v.Item3) : defaultValue;
        }

        public static Quaternion GetQuaternion(this Message self, int index)
        {
            var (x, y, z, w) = GetValues4(self, index, OCSValue.ValueType.Float);
            return new Quaternion(x, y, z, w);
        } 
        
        public static Quaternion? GetQuaternionNullable(this Message self, int index)
        {
            return TryGetValues4(self, index, out var v, OCSValue.ValueType.Float) ? new Quaternion(v.Item1, v.Item2, v.Item3, v.Item4) : null;
        } 
        
        public static Quaternion GetQuaternionWithDefault(this Message self, int index, Quaternion defaultValue)
        {
            return TryGetValues4(self, index, out var v, OCSValue.ValueType.Float) ? new Quaternion(v.Item1, v.Item2, v.Item3, v.Item4) : defaultValue;
        }
        
        private static (OCSValue, OCSValue, OCSValue) GetValues3(this Message self, int index, OCSValue.ValueType expectType)
        {
            if (index < 0 || index + 2 >= self.values.Length)
                throw new IndexOutOfRangeException($"value index {index} is out of range. size is {self.values.Length}");

            self.CheckValueType(index, expectType);
            self.CheckValueType(index + 1, expectType);
            self.CheckValueType(index + 2, expectType);
            
            var v1 = self.values[index];
            var v2 = self.values[index + 1];
            var v3 = self.values[index + 2];
            return (v1, v2, v3);
        }
        
        private static bool TryGetValues3(this Message self, int index, out (OCSValue, OCSValue, OCSValue) value, OCSValue.ValueType expectType)
        {
            value = default;
            if (index < 0 || index + 2 >= self.values.Length) return false;

            if (!self.CheckValueType(index, expectType)) return false;
            if (!self.CheckValueType(index + 1, expectType)) return false;
            if (!self.CheckValueType(index + 2, expectType)) return false;
            
            var v1 = self.values[index];
            var v2 = self.values[index + 1];
            var v3 = self.values[index + 2];
            
            value = (v1, v2, v3);
            return true;
        }

        private static (OCSValue, OCSValue, OCSValue, OCSValue) GetValues4(this Message self, int index, OCSValue.ValueType expectType)
        {
            if (index < 0 || index + 3 >= self.values.Length)
                throw new IndexOutOfRangeException($"value index {index} is out of range. size is {self.values.Length}");

            self.CheckValueType(index, expectType);
            self.CheckValueType(index + 1, expectType);
            self.CheckValueType(index + 2, expectType);
            self.CheckValueType(index + 3, expectType);
            
            var v1 = self.values[index];
            var v2 = self.values[index + 1];
            var v3 = self.values[index + 2];
            var v4 = self.values[index + 3];
            return (v1, v2, v3, v4);
        }
        
        private static bool TryGetValues4(this Message self, int index, out (OCSValue, OCSValue, OCSValue, OCSValue) value, OCSValue.ValueType expectType)
        {
            value = default;
            if (index < 0 || index + 3 >= self.values.Length) return false;

            if (!self.CheckValueType(index, expectType)) return false;
            if (!self.CheckValueType(index + 1, expectType)) return false;
            if (!self.CheckValueType(index + 2, expectType)) return false;
            if (!self.CheckValueType(index + 3, expectType)) return false;
            
            var v1 = self.values[index];
            var v2 = self.values[index + 1];
            var v3 = self.values[index + 2];
            var v4 = self.values[index + 3];
            
            value = (v1, v2, v3, v4);
            return true;
        }
    }
}