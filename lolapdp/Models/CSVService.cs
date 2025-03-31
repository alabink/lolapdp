using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace lolapdp.Models
{
    public class CSVService : ICSVService
    {
        public List<T> ReadCSV<T>(string filePath) where T : class, new()
        {
            var results = new List<T>();
            if (!File.Exists(filePath))
                return results;

            var lines = File.ReadAllLines(filePath);
            if (lines.Length <= 1) 
                return results;

            var headers = lines[0].Split(',');
            var properties = typeof(T).GetProperties();

            for (int i = 1; i < lines.Length; i++)
            {
                var values = lines[i].Split(',');
                var item = new T();

                for (int j = 0; j < headers.Length && j < values.Length; j++)
                {
                    var property = properties.FirstOrDefault(p => 
                        p.Name.Equals(headers[j], StringComparison.OrdinalIgnoreCase));
                    
                    if (property != null && !string.IsNullOrEmpty(values[j]))
                    {
                        var value = Convert.ChangeType(values[j], property.PropertyType);
                        property.SetValue(item, value);
                    }
                }

                results.Add(item);
            }

            return results;
        }

        public void WriteCSV<T>(string filePath, List<T> data) where T : class
        {
            var properties = typeof(T).GetProperties();
            var headers = string.Join(",", properties.Select(p => p.Name));
            var lines = new List<string> { headers };

            foreach (var item in data)
            {
                var values = properties.Select(p => p.GetValue(item)?.ToString() ?? "");
                lines.Add(string.Join(",", values));
            }

            File.WriteAllLines(filePath, lines);
        }
    }
}
//dbl