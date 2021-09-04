using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crossword
{
   public class CrosswordWord
   {
      public string Value { get; private set; }
      public int Length { get; private set; }
      public bool AxisX { get; set; }
      public int ReferenceIndex { get; set; }
      public int Intersections { get; set; }

      public CrosswordWord(string value) { Value = value; Length = value.Length; }

      public CrosswordWord Clone()
      {
         return new CrosswordWord(Value)
         {
            AxisX = AxisX,
            ReferenceIndex = ReferenceIndex,
            Intersections = Intersections,
         };
      }
   }
}
