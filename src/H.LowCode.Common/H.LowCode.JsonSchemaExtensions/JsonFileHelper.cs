using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace H.LowCode.Utility
{
    /// <summary>
    /// 
    /// </summary>
    public static class JsonFileHelper
    {
        public static T Read<T>(string jsonFileName, JsonSerializerOptions options = null)
        {
            if (string.IsNullOrEmpty(jsonFileName))
                throw new ArgumentNullException(nameof(jsonFileName));

            if (!File.Exists(jsonFileName))
                throw new FileNotFoundException($"文件{jsonFileName}不存在!");

            string json = File.ReadAllText(jsonFileName);
            return JsonSerializer.Deserialize<T>(json, options);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonFileName"></param>
        /// <returns></returns>
        public static List<T> ReadList<T>(string jsonFileName)
        {
            return Read<List<T>>(jsonFileName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonFileName"></param>
        /// <param name="t"></param>
        public static void Write<T>(string jsonFileName, T t)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonFileName"></param>
        public static void Remove(string jsonFileName)
        {
            throw new NotImplementedException();
        }
    }
}
