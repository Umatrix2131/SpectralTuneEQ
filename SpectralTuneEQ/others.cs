﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectralTuneEQ
{
    public class others
    {
        unsafe public static T[] ToData<T>(byte[] bytes)
        {
            T[] Arrays = new T[bytes.Length / sizeof(T)];
            Buffer.BlockCopy(bytes, 0, Arrays, 0, bytes.Length);
            return Arrays;
        }
        public static (double[] L, double[] R) ConvertDataTypesToDouble(object DataSrc)
        {
            if (DataSrc is byte[] A1)
            {
                var evenArray = A1.Where((value, index) => index % 2 == 0).Select(value => (double)value * 0.1d).ToArray();
                var oddArray = A1.Where((value, index) => index % 2 != 0).Select(value => (double)value * 0.1d).ToArray();
                return (evenArray, oddArray);
            }
            if (DataSrc is short[] A2)
            {
                var evenArray = A2.Where((value, index) => index % 2 == 0).Select(value => (double)value * 0.1d).ToArray();
                var oddArray = A2.Where((value, index) => index % 2 != 0).Select(value => (double)value * 0.1d).ToArray();
                return (evenArray, oddArray);
            }
            if (DataSrc is int[] A3)
            {
                var evenArray = A3.Where((value, index) => index % 2 == 0).Select(value => (double)value * 0.1d).ToArray();
                var oddArray = A3.Where((value, index) => index % 2 != 0).Select(value => (double)value * 0.1d).ToArray();
                return (evenArray, oddArray);
            }
            if (DataSrc is float[] A4)
            {
                var evenArray = A4.Where((value, index) => index % 2 == 0).Select(value => (double)value * 0.1d).ToArray();
                var oddArray = A4.Where((value, index) => index % 2 != 0).Select(value => (double)value * 0.1d).ToArray();
                return (evenArray, oddArray);
            }
            if (DataSrc is double[] A5)
            {
                var evenArray = A5.Where((value, index) => index % 2 == 0).Select(value => (double)value * 0.1d).ToArray();
                var oddArray = A5.Where((value, index) => index % 2 != 0).Select(value => (double)value * 0.1d).ToArray();
                return (evenArray, oddArray);
            }

            return (null, null);
        }
        public static double[] Combind(double[] data1, double[] data2)
        {
            return data1.Zip(data2, (a, b) => new double[] { a, b }).SelectMany(pair => pair).ToArray();
        }
        public static byte[] ToBytes<T>(T[] arrays)
        {
            if (typeof(T).Name == typeof(short).Name)
            {
                int byteLength = arrays.Length * 2;
                byte[] byteArray = new byte[byteLength];
                Buffer.BlockCopy(arrays, 0, byteArray, 0, byteLength);
                return byteArray;
            }
            if (typeof(T).Name == typeof(int).Name)
            {
                int byteLength = arrays.Length * 4;
                byte[] byteArray = new byte[byteLength];
                Buffer.BlockCopy(arrays, 0, byteArray, 0, byteLength);
                return byteArray;
            }
            if (typeof(T).Name == typeof(uint).Name)
            {
                int byteLength = arrays.Length * 4;
                byte[] byteArray = new byte[byteLength];
                Buffer.BlockCopy(arrays, 0, byteArray, 0, byteLength);
                return byteArray;
            }
            if (typeof(T).Name == typeof(float).Name)
            {
                int byteLength = arrays.Length * 4;
                byte[] byteArray = new byte[byteLength];
                Buffer.BlockCopy(arrays, 0, byteArray, 0, byteLength);
                return byteArray;
            }
            if (typeof(T).Name == typeof(double).Name)
            {
                int byteLength = arrays.Length * 8;
                byte[] byteArray = new byte[byteLength];
                Buffer.BlockCopy(arrays, 0, byteArray, 0, byteLength);
                return byteArray;
            }
            return null;
        }
        public static bool IsPowerOfTwo(int n)
        { 
            return (n > 0) && ((n & (n - 1)) == 0);
        }
    }
}
