using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crossword
{
   public static class CrosswordExport
   {
      const int cellSize = 4;

      static readonly Dictionary<char, int> letterFrequencies = new Dictionary<char, int>
      {
         { 'A', 812 },
         { 'B', 149 },
         { 'C', 271 },
         { 'D', 432 },
         { 'E', 1050 },
         { 'F', 230 },
         { 'G', 203 },
         { 'H', 592 },
         { 'I', 731 },
         { 'J', 10 },
         { 'K', 69 },
         { 'L', 398 },
         { 'M', 261 },
         { 'N', 695 },
         { 'O', 768 },
         { 'P', 182 },
         { 'Q', 11 },
         { 'R', 602 },
         { 'S', 628 },
         { 'T', 910 },
         { 'U', 288 },
         { 'V', 111 },
         { 'W', 209 },
         { 'X', 17 },
         { 'Y', 211 },
         { 'Z', 7 },
      };

      static readonly string letters;

      static CrosswordExport()
      {
         var rnd = new Random();
         var list = new List<char>();
         foreach (var c in letterFrequencies)
         {
            for (int i = 0; i < c.Value; i++) { list.Add(c.Key); }
         }
         for (int i = list.Count - 1; i > 0; i--)
         {
            int j = rnd.Next(i + 1);
            var s = list[i];
            list[i] = list[j];
            list[j] = s;
         }
         letters = string.Join("", list);
      }

      public static byte[] Export(CrosswordMap map)
      {
         var rnd = new Random();
         using (var stream = new MemoryStream())
         using (var package = new ExcelPackage(stream))
         {
            var sheet = package.Workbook.Worksheets.Add("Crossword");
            for (int x = 0; x < map.X; x++) { sheet.Column(x + 1).Width = cellSize; }
            for (int y = 0; y < map.Y; y++) { sheet.Row(y + 1).Height = cellSize * 5; }
            for (int y = 0; y < map.Y; y++)
            {
               for (int x = 0; x < map.X; x++)
               {
                  var celRef = sheet.Cells[y + 1, x + 1].Style;
                  celRef.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                  celRef.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                  var cell = map.Get(x, y);
                  if (cell == null)
                  {
                     celRef.Fill.BackgroundColor.SetColor(System.Drawing.Color.Black);
                  }
                  else
                  {
                     if (cell.IndexX == 0 && cell.WordX != null) { sheet.SetValue(y + 1, x + 1, cell.WordX.ReferenceIndex); }
                     else if (cell.IndexY == 0 && cell.WordY != null) { sheet.SetValue(y + 1, x + 1, cell.WordY.ReferenceIndex); }
                     celRef.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
                     celRef.Font.Size = 8;
                     celRef.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                     celRef.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
                  }
               }
            }
            sheet.Column(map.X + 2).Width = cellSize;
            sheet.SetValue(1, map.X + 3, "Horizontal");
            sheet.SetValue(1, map.X + 4, "Vertical");
            sheet.Cells[1, map.X + 3].Style.Font.Bold = true;
            sheet.Cells[1, map.X + 4].Style.Font.Bold = true;
            var words = map.Words.OrderBy(x => x.ReferenceIndex).ToArray();
            int xRow = 1, yRow = 1;
            for (int i = 0; i < words.Length; i++)
            {
               var label = words[i].ReferenceIndex + ": " + words[i].Value;
               if (words[i].AxisX) { sheet.SetValue(++xRow, map.X + 3, label); }
               else { sheet.SetValue(++yRow, map.X + 4, label); }
            }
            for (int y = Math.Max(xRow, yRow) - 1; y >= map.Y; y--) { sheet.Row(y + 1).Height = cellSize * 5; }
            sheet.Column(map.X + 3).AutoFit();
            sheet.Column(map.X + 4).AutoFit();

            sheet = package.Workbook.Worksheets.Add("Alternative");
            for (int x = 0; x < map.X; x++) { sheet.Column(x + 1).Width = cellSize; }
            for (int y = 0; y < map.Y; y++) { sheet.Row(y + 1).Height = cellSize * 5; }
            for (int y = 0; y < map.Y; y++)
            {
               for (int x = 0; x < map.X; x++)
               {
                  var celRef = sheet.Cells[y + 1, x + 1].Style;
                  celRef.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                  celRef.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
                  celRef.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                  celRef.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                  celRef.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                  celRef.Font.Size = 12;
                  sheet.SetValue(y + 1, x + 1, (map.Get(x, y)?.Character ?? letters[rnd.Next(0, letters.Length)]).ToString());
               }
            }
            sheet.Column(map.X + 2).Width = cellSize;
            sheet.SetValue(1, map.X + 3, "Words");
            sheet.Cells[1, map.X + 3].Style.Font.Bold = true;
            for (int i = 0; i < map.Words.Count; i++) { sheet.SetValue(i + 2, map.X + 3, words[i].Value); }
            for (int y = map.Words.Count; y >= map.Y; y--) { sheet.Row(y + 1).Height = cellSize * 5; }
            sheet.Column(map.X + 3).AutoFit();

            sheet = package.Workbook.Worksheets.Add("Solution");
            for (int x = 0; x < map.X; x++) { sheet.Column(x + 1).Width = cellSize; }
            for (int y = 0; y < map.Y; y++) { sheet.Row(y + 1).Height = cellSize * 5; }
            for (int y = 0; y < map.Y; y++)
            {
               for (int x = 0; x < map.X; x++)
               {
                  var celRef = sheet.Cells[y + 1, x + 1].Style;
                  celRef.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                  celRef.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                  var cell = map.Get(x, y);
                  if (cell == null)
                  {
                     celRef.Fill.BackgroundColor.SetColor(System.Drawing.Color.Black);
                  }
                  else
                  {
                     sheet.SetValue(y + 1, x + 1, cell.Character.ToString());
                     celRef.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
                     celRef.Font.Size = 12;
                     celRef.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                     celRef.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                  }
               }
            }

            package.Save();
            return stream.ToArray();
         }
      }
   }
}
