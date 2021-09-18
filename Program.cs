using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Crossword
{
   class Program
   {
      static void Main(string[] args)
      {
         //args = new string[] { "input.txt", "-c", "24" };

         string input = null, output = null;
         int count = 0;
         for (int i = 0; i < args.Length; i++)
         {
            if (args[i] == "-c")
            {
               i++; if (i >= args.Length) { break; }
               count = int.Parse(args[i]);
               continue;
            }
            if (input == null) { input = args[i]; continue; }
            if (output == null) { output = args[i]; continue; }
         }
         if (output == null) { output = "output.xlsx"; }
         if (input == null)
         {
            Console.WriteLine("Crossword <input_file> [-c count] [output_file]");
            Console.WriteLine();
            Console.WriteLine("   <input_file>: Input words file; one per line.");
            Console.WriteLine("       -c count: Number of words to select from input file.");
            Console.WriteLine("  [output_file]: Output file name (default: output.xlsx)");
            return;
         }
         var words = new List<string>();
         foreach (var line in File.ReadAllLines(input))
         {
            if (string.IsNullOrWhiteSpace(line)) { continue; }
            var word = line.Trim().Replace(" ", "").ToUpper();
            if (word.Length < 3) { continue; }
            words.Add(word);
         }
         var rnd = new Random();
         for (int i = words.Count - 1; i > 0; i--)
         {
            int j = rnd.Next(i + 1);
            var s = words[i];
            words[i] = words[j];
            words[j] = s;
         }
         if (count <= 0) { count = words.Count; }
         var map = CrosswordMap.BuildAsync(rnd.Next(), words.Distinct().Take(count).ToArray(), 10000, 8, (result) =>
         {
            Console.WriteLine("Result found - all intersecting: {0}, intersections: {1}, area: {2}",
               result.AllIntersecting, result.Intersections, result.X * result.Y);
            return false;
         }).Result;
         if (map == null) { throw new InvalidOperationException("Cannot build after many tries."); }
         var xlsx = CrosswordExport.Export(map);
         File.WriteAllBytes(output, xlsx);
      }
   }
}
