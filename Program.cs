﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Crossword
{
   class Program
   {
      static void Main(string[] args)
      {
         //args = new string[] { "input.txt" };
         if (args.Length == 0)
         {
            Console.WriteLine("Crossword <input_file> [output_file]");
            Console.WriteLine();
            Console.WriteLine("   <input_file>: Input words file; one per line.");
            Console.WriteLine("  [output_file]: Output file name (default: output.xlsx)");
            return;
         }
         var words = new List<string>();
         foreach (var line in File.ReadAllLines(args[0]))
         {
            if (string.IsNullOrWhiteSpace(line)) { continue; }
            words.Add(line.Trim().Replace(" ", "").ToUpper());
         }
         var map = CrosswordMap.Build(1, words.Distinct().ToArray());
         if (map == null) { throw new InvalidOperationException("Cannot build after many tries."); }
         var xlsx = CrosswordExport.Export(map);
         File.WriteAllBytes(args.Length > 1 ? args[1] : "output.xlsx", xlsx);
      }
   }
}
