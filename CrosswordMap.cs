using System;
using System.Collections.Generic;
using System.Linq;

namespace Crossword
{
   public class CrosswordMap
   {
      public class Cell
      {
         public CrosswordWord WordX;
         public CrosswordWord WordY;
         public int IndexX;
         public int IndexY;

         public char Character
         {
            get
            {
               if (WordX != null) { return WordX.Value[IndexX]; }
               if (WordY != null) { return WordY.Value[IndexY]; }
               return ' ';
            }
         }
      }

      public struct Point
      {
         public readonly int X;
         public readonly int Y;
         public Point(int x, int y) { X = x; Y = y; }
      }

      private readonly List<CrosswordWord> words = new List<CrosswordWord>();
      private Cell[] map;
      private int mapX;
      private int mapY;
      private bool allIntersect;
      private int intersections;

      public int X => mapX;
      public int Y => mapY;
      public IReadOnlyCollection<CrosswordWord> Words => words;

      public Cell Get(int x, int y)
      {
         if (x < 0 || y < 0 || x >= mapX || y >= mapY) { return null; }
         var cell = map[y * mapX + x];
         if (cell == null) { return null; }
         if (cell.WordX == null && cell.WordY == null) { return null; }
         return cell;
      }

      private void Resize(int x, int y)
      {
         if (x == mapX && y == mapY) { return; }
         if (x <= 0 || y <= 0) { map = null; mapX = 0; mapY = 0; return; }
         var newMap = new Cell[x * y];
         int minY = Math.Min(y, mapY);
         int minX = Math.Min(x, mapX);
         for (int cy = 0; cy < minY; cy++)
         {
            for (int cx = 0, ci = cy * mapX, ni = cy * x; cx < minX; cx++, ci++, ni++) { newMap[ni] = map[ci]; }
         }
         map = newMap; mapX = x; mapY = y;
      }

      private bool RowEmpty(int y)
      {
         if (y < 0 || y >= mapY) { return true; }
         for (int cx = 0, ci = y * mapX; cx < mapX; cx++, ci++)
         {
            var cell = map[ci]; if (cell == null) { continue; }
            if (cell.WordX == null && cell.WordY == null) { map[ci] = null; continue; }
            return false;
         }
         return true;
      }

      private bool ColEmpty(int x)
      {
         if (x < 0 || x >= mapX) { return true; }
         for (int cy = 0, ci = x; cy < mapY; cy++, ci += mapX)
         {
            var cell = map[ci]; if (cell == null) { continue; }
            if (cell.WordX == null && cell.WordY == null) { map[ci] = null; continue; }
            return false;
         }
         return true;
      }

      private bool CanMove(int x, int y)
      {
         if (y < 0)
         {
            var maxY = -y; if (maxY > mapY) { maxY = mapY; }
            for (int cy = 0; cy < maxY; cy++) { if (!RowEmpty(cy)) { return false; } }
         }
         else if (y > 0)
         {
            var startY = mapY - y; if (startY < 0) { startY = 0; }
            for (int cy = startY; cy < mapY; cy++) { if (!RowEmpty(cy)) { return false; } }
         }
         if (x < 0)
         {
            var maxX = -x; if (maxX > mapX) { maxX = mapX; }
            for (int cx = 0; cx < maxX; cx++) { if (!ColEmpty(cx)) { return false; } }
         }
         else if (x > 0)
         {
            var startX = mapX - x; if (startX < 0) { startX = 0; }
            for (int cx = startX; cx < mapX; cx++) { if (!ColEmpty(cx)) { return false; } }
         }
         return true;
      }

      private bool Move(int x, int y)
      {
         if (!CanMove(x, y)) { return false; }
         if (y < 0)
         {
            var maxY = -y; if (maxY > mapY) { maxY = mapY; }
            for (int cy = 0; cy < maxY; cy++)
            {
               var sy = cy + maxY;
               if (sy >= mapY) { for (int cx = 0, ci = cy * mapX; cx < mapX; cx++, ci++) { map[ci] = null; } }
               else { for (int cx = 0, ci = cy * mapX, si = sy * mapX; cx < mapX; cx++, ci++, si++) { map[ci] = map[si]; map[si] = null; } }
            }
         }
         else if (y > 0)
         {
            for (int cy = mapY - 1; cy >= y; cy--)
            {
               var sy = cy - y;
               if (sy < 0) { for (int cx = 0, ci = cy * mapX; cx < mapX; cx++, ci++) { map[ci] = null; } }
               else { for (int cx = 0, ci = cy * mapX, si = sy * mapX; cx < mapX; cx++, ci++, si++) { map[ci] = map[si]; map[si] = null; } }
            }
         }
         if (x < 0)
         {
            var maxX = -x; if (maxX > mapX) { maxX = mapX; }
            for (int cx = 0; cx < maxX; cx++)
            {
               var sx = cx + maxX;
               if (sx >= mapX) { for (int cy = 0, ci = cx; cy < mapY; cy++, ci++) { map[ci] = null; } }
               else { for (int cy = 0, si = sx, ci = cx; cy < mapY; cy++, ci += mapX, si += mapX) { map[ci] = map[si]; map[si] = null; } }
            }
         }
         else if (x > 0)
         {
            for (int cx = mapX - 1; cx >= x; cx--)
            {
               var sx = cx - x;
               if (sx < 0) { for (int cy = 0, ci = cx; cy < mapY; cy++, ci++) { map[ci] = null; } }
               else { for (int cy = 0, si = sx, ci = cx; cy < mapY; cy++, ci += mapX, si += mapX) { map[ci] = map[si]; map[si] = null; } }
            }
         }
         return true;
      }

      private void Trim()
      {
         int minY; for (minY = 0; minY < mapY; minY++) { if (!RowEmpty(minY)) { break; } }
         if (minY > 0) { Move(0, -minY); }
         int minX; for (minX = 0; minX < mapX; minX++) { if (!ColEmpty(minX)) { break; } }
         if (minX > 0) { Move(-minX, 0); }
         int maxX = mapX, maxY = mapY;
         while (maxY > 0) { if (!RowEmpty(maxY - 1)) { break; } maxY--; }
         while (maxX > 0) { if (!ColEmpty(maxX - 1)) { break; } maxX--; }
         Resize(maxX, maxY);
      }

      private void Clear()
      {
         map = null;
         mapX = 0;
         mapY = 0;
         allIntersect = false;
         intersections = 0;
      }

      private bool AssignIndices()
      {
         int index = 1;
         for (int i = words.Count - 1; i >= 0; i--)
         {
            words[i].ReferenceIndex = 0;
            words[i].Intersections = 0;
         }
         for (int i = 0, j = mapX * mapY; i < j; i++)
         {
            var cell = map[i]; if (cell == null) { continue; }
            var inc = false;
            if (cell.IndexX == 0 && cell.WordX != null)
            {
               if (cell.WordX.ReferenceIndex != 0) { return false; }
               if (!cell.WordX.AxisX) { return false; }
               cell.WordX.ReferenceIndex = index; inc = true;
            }
            if (cell.IndexY == 0 && cell.WordY != null)
            {
               if (cell.WordY.ReferenceIndex != 0) { return false; }
               if (cell.WordY.AxisX) { return false; }
               cell.WordY.ReferenceIndex = index; inc = true;
            }
            if (inc) { index++; }
            if (cell.WordX != null && cell.WordY != null)
            {
               cell.WordX.Intersections++;
               cell.WordY.Intersections++;
            }
         }
         intersections = words.Select(x => x.Intersections).DefaultIfEmpty(0).Sum();
         allIntersect = words.All(x => x.Intersections > 0);
         return words.All(x => x.ReferenceIndex != 0);
      }

      private void BalanceAxis(Random rnd)
      {
         for (int i = 0; i < words.Count; i++) { words[i].AxisX = rnd.Next(2) == 0; }
         var xWords = words.Where(x => x.AxisX).ToList();
         var yWords = words.Where(x => !x.AxisX).ToList();
         var xTotal = xWords.Select(x => x.Length).DefaultIfEmpty(0).Sum();
         var yTotal = yWords.Select(x => x.Length).DefaultIfEmpty(0).Sum();
         int tries = 0;
         while (Math.Abs(xTotal - yTotal) > 3 && tries < 100)
         {
            if (xTotal > yTotal)
            {
               while (xTotal > yTotal)
               {
                  var i = rnd.Next(0, xWords.Count);
                  var word = xWords[i];
                  word.AxisX = false;
                  xWords.RemoveAt(i);
                  yWords.Add(word);
                  xTotal -= word.Length;
                  yTotal += word.Length;
               }
            }
            else
            {
               while (yTotal > xTotal)
               {
                  var i = rnd.Next(0, yWords.Count);
                  var word = yWords[i];
                  word.AxisX = true;
                  yWords.RemoveAt(i);
                  xWords.Add(word);
                  yTotal -= word.Length;
                  xTotal += word.Length;
               }
            }
            tries++;
         }
         if (xWords.Count == 0) { if (yWords.Count > 1) { yWords[0].AxisX = true; } }
         else if (yWords.Count == 0) { if (xWords.Count > 1) { xWords[0].AxisX = false; } }
      }

      private bool CanDrawAt(CrosswordWord word, int x, int y)
      {
         Cell cell;
         if (word.AxisX)
         {
            cell = Get(x - 1, y); if (cell != null) { return false; }
            cell = Get(x + word.Length, y); if (cell != null) { return false; }
            for (int i = 0, ci = y * mapX + x; i < word.Length; i++, ci++)
            {
               var cx = x + i;
               if (cx >= mapX) { break; }
               if (cx < 0) { continue; }
               if (y >= 0 && y < mapY)
               {
                  cell = map[ci];
                  if (cell != null)
                  {
                     if (cell.WordX != null) { return false; }
                     if (cell.WordY != null)
                     {
                        if (cell.Character == word.Value[i]) { continue; }
                        return false;
                     }
                  }
               }
               if (y > 0 && (y - 1) < mapY)
               {
                  cell = map[ci - mapX];
                  if (cell != null && ((cell.WordX != null && cell.WordY == null) || (cell.WordY != null))) { return false; }
               }
               if ((y + 1) < mapY && (y + 1) >= 0)
               {
                  cell = map[ci + mapX];
                  if (cell != null && ((cell.WordX != null && cell.WordY == null) || (cell.WordY != null))) { return false; }
               }
            }
         }
         else
         {
            cell = Get(x, y - 1); if (cell != null) { return false; }
            cell = Get(x, y + word.Length); if (cell != null) { return false; }
            for (int i = 0, ci = y * mapX + x; i < word.Length; i++, ci += mapX)
            {
               var cy = y + i;
               if (cy >= mapY) { break; }
               if (cy < 0) { continue; }
               if (x >= 0 && x < mapX)
               {
                  cell = map[ci];
                  if (cell != null)
                  {
                     if (cell.WordY != null) { return false; }
                     if (cell.WordX != null)
                     {
                        if (cell.Character == word.Value[i]) { continue; }
                        return false;
                     }
                  }
               }
               if (x > 0 && (x - 1) < mapX)
               {
                  cell = map[ci - 1];
                  if (cell != null && ((cell.WordY != null && cell.WordX == null) || (cell.WordX != null))) { return false; }
               }
               if ((x + 1) < mapX && (x + 1) >= 0)
               {
                  cell = map[ci + 1];
                  if (cell != null && ((cell.WordY != null && cell.WordX == null) || (cell.WordX != null))) { return false; }
               }
            }
         }
         return true;
      }

      private bool DrawAt(CrosswordWord word, int x, int y)
      {
         if (!CanDrawAt(word, x, y)) { return false; }
         if (x < 0) { Resize(mapX - x, mapY); if (!Move(-x, 0)) { return false; } x = 0; } 
         else if (x >= mapX) { Resize(x + 1, Math.Max(mapY, 1)); }
         if (y < 0) { Resize(mapX, mapY - y); if (!Move(0, -y)) { return false; } y = 0; }
         else if (y >= mapY) { Resize(Math.Max(mapX, 1), y + 1); }
         if (word.AxisX)
         {
            if ((x + word.Length) >= mapX) { Resize(x + word.Length, mapY); }
            for (int i = 0, ci = y * mapX + x; i < word.Length; i++, ci++)
            {
               var cell = map[ci];
               if (cell == null) { map[ci] = cell = new Cell(); }
               cell.WordX = word;
               cell.IndexX = i;
            }
         }
         else
         {
            if ((y + word.Length) >= mapY) { Resize(mapX, y + word.Length); }
            for (int i = 0, ci = y * mapX + x; i < word.Length; i++, ci += mapX)
            {
               var cell = map[ci];
               if (cell == null) { map[ci] = cell = new Cell(); }
               cell.WordY = word;
               cell.IndexY = i;
            }
         }
         return true;
      }

      public CrosswordMap Clone()
      {
         var clone = new CrosswordMap();
         var newWords = new Dictionary<CrosswordWord, CrosswordWord>();
         for (int i = 0; i < words.Count; i++)
         {
            var oldWord = words[i];
            var newWord = oldWord.Clone();
            newWords[oldWord] = newWord;
            clone.words.Add(newWord);
         }
         if (map != null)
         {
            clone.map = new Cell[map.Length];
            clone.mapX = mapX;
            clone.mapY = mapY;
            for (int i = 0; i < map.Length; i++)
            {
               var cell = map[i];
               if (cell == null) { continue; }
               clone.map[i] = new Cell()
               {
                  WordX = cell.WordX != null ? newWords[cell.WordX] : null,
                  WordY = cell.WordY != null ? newWords[cell.WordY] : null,
                  IndexX = cell.IndexX,
                  IndexY = cell.IndexY,
               };
            }
         }
         return clone;
      }

      private bool TryInsert(CrosswordWord word, List<Point> insertions, Random rnd)
      {
         insertions.Clear();
         for (int ci = 0; ci < map.Length; ci++)
         {
            var cell = map[ci];
            if (cell == null || (cell.WordX == null && cell.WordY == null)) { continue; }
            var c = cell.Character;
            for (int idx = 0; idx < word.Length; idx++)
            {
               if (word.Value[idx] == c)
               {
                  int x = ci % mapX, y = ci / mapX;
                  if (word.AxisX) { x -= idx; } else { y -= idx; }
                  insertions.Add(new Point(x, y));
               }
            }
         }
         if (insertions.Count == 0) { return false; }
         Shuffle(insertions, rnd);
         for (int j = 0; j < insertions.Count; j++)
         {
            if (DrawAt(word, insertions[j].X, insertions[j].Y)) { return true; }
         }
         return false;
      }

      private bool TryInsertBrute(CrosswordWord word, List<Point> insertions, Random rnd)
      {
         insertions.Clear();
         for (int y = word.AxisX ? 0 : -word.Length, ym = mapY + 2; y < ym; y++)
         {
            for (int x = word.AxisX ? -word.Length : 0, xm = mapX + 2; x < xm; x++)
            {
               insertions.Add(new Point(x, y));
            }
         }
         Shuffle(insertions, rnd);
         for (int j = 0; j < insertions.Count; j++)
         {
            if (DrawAt(word, insertions[j].X, insertions[j].Y)) { return true; }
         }
         return false;
      }

      private bool TryFill(Random rnd)
      {
         var insertions = new List<Point>();
         BalanceAxis(rnd);
         Shuffle(words, rnd);
         var left = new Queue<CrosswordWord>(words);
         Clear();
         if (left.Count > 0) { DrawAt(left.Dequeue(), 0, 0); }
         int fails = 0;
         while (left.Count > 0)
         {
            var brute = fails > left.Count;
            var word = left.Dequeue();
            if (brute)
            {
               word.AxisX = !word.AxisX;
               if (TryInsert(word, insertions, rnd)) { fails = 0; }
               else
               {
                  word.AxisX = !word.AxisX;
                  if (!TryInsertBrute(word, insertions, rnd)) { return false; }
               }
            }
            else
            {
               if (TryInsert(word, insertions, rnd)) { fails = 0; }
               else { fails++; left.Enqueue(word); }
            }
         }
         return true;
      }

      public static CrosswordMap Build(int seed, params string[] list)
      {
         var map = new CrosswordMap();
         var rnd = new Random(seed);
         CrosswordMap best = null;
         if (list == null || list.Length == 0) { return map; }
         map.words.Clear();
         map.words.AddRange(list.Select(x => new CrosswordWord(x)));
         for (int tries = 0; tries < 100000; tries++)
         {
            if (!map.TryFill(rnd)) { continue; }
            map.Trim();
            if (!map.AssignIndices()) { continue; }
            if (best == null ||
               (!best.allIntersect && map.allIntersect) ||
               (best.intersections < map.intersections) ||
               (best.X * best.Y) > (map.X * map.Y)) { best = map.Clone(); }
         }
         return best;
      }

      private static void Shuffle<T>(List<T> list, Random rnd)
      {
         for (int i = list.Count - 1; i > 0; i--)
         {
            int j = rnd.Next(i + 1);
            var s = list[i]; list[i] = list[j]; list[j] = s;
         }
      }
   }
}
