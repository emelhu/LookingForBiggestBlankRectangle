using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LookingForBiggestBlankRectangle
{
  class Program
  {
    static void Main(string[] args)
    {
      Console.ResetColor();
      Console.WriteLine(" --- Looking for biggest blank rectangle  (c) eMeL with  MIT licence --- \n\n");

      bool[,] starmap = GetMap(@".\starmap.txt");                                                // TODO: filename from args

      #if DEBUG
      Console.WriteLine("'Star Map' readed...");
      Console.ReadKey();
      #endif

      DisplayMap(starmap);

      Rectangle rectangle = Processmap(starmap);

      #if DEBUG
      Console.WriteLine("'Star Map' processed...");
      Console.ReadKey();
      #endif

      DisplayMap(starmap, rectangle);

      #if DEBUG
      Console.WriteLine("--- end ---");
      Console.ReadKey();
      Console.WriteLine();
      #endif
    }

    #region consts
    private const int minLines = 5;
    private const int maxLines = 50;
    private const int minColumns = 5;
    private const int maxColumns = 200;
    #endregion

    private static bool[,] GetMap(string filename)
    {
      string[] lines = null; ;

      try
      {
        lines = File.ReadAllLines(filename);
      }
      catch (Exception exc)
      {
        Console.WriteLine("Read of 'Star Map' unsuccessful!\n" +
                          "Filename: {0}\n" +
                          "Error: {1}", filename, exc.Message);
        Environment.Exit(1);
      }

      if ((lines == null) || (lines.Length < minLines) || (lines.Length > maxLines))
      {
        Console.WriteLine($"Read of 'Star Map' unsuccessful!\n" +
                           "Filename: {filename}\n" +
                           "Error: count of lines not in {minLines} and {maxLines}");
        Environment.Exit(2);
      }

      int minColumnLen = (from line in lines
                          select line.Length).Min();

      int maxColumnLen = (from line in lines
                          select line.Length).Max();

      if ((minColumnLen < minColumns) || (maxColumnLen > maxColumns))
      {
        Console.WriteLine($"Read of 'Star Map' unsuccessful!\n" +
                           "Filename: {filename}\n" +
                           "Error: count of columns not in {minColumns} and {maxColumns}");
        Environment.Exit(3);
      }

      var starmap = new bool[lines.Length, maxColumnLen];
      int blanks  = 0;

      for (int lineLoop = 0; lineLoop < lines.Length; lineLoop++)
      {
        string line   = lines[lineLoop];       

        for (int columnLoop = 0; columnLoop < line.Length; columnLoop++)
        {
          char signalChar = line[columnLoop];

          if (signalChar == ' ')
          { // OK, default false
            blanks++;
          }
          else if (signalChar == '*')
          { // signed
            starmap[lineLoop, columnLoop] = true;
          }
          else
          {
            Console.WriteLine($"Read of 'Star Map' unsuccessful!\n" +
                               "Filename: {filename}\n" +
                               "Error: one of characters is not blank or asterix (' ' | '*')!  It's '{signalChar}'!");
            Environment.Exit(4);
          }
        }
      }

      if (blanks < 1)
      { // special case ... what i need do?        
      }

      return starmap;
    }

    private const char borderChar = '°';

    private static void DisplayMap(bool[,] starmap, Rectangle rectangle = null)
    {
      bool InsideRectangle(int row, int col)
      {
        if (rectangle != null)
        {
          if ((row >= rectangle.row) && (row < rectangle.row + rectangle.height) &&
              (col >= rectangle.col) && (col < rectangle.col + rectangle.width))
          {
            return true;
          }
        }

        return false;
      }

      //

      Console.ResetColor();
      Console.WriteLine();

      if (rectangle == null)
      {
        Console.WriteLine(">>>> Readed Star Map <<<<");
      }
      else
      {
        Console.WriteLine(">>>>  row:{0}  col:{1}   height:{2}   width:{3} | weight:{4}", rectangle.row, rectangle.col, rectangle.height, rectangle.width, rectangle.height * rectangle.width);        
      }

      Console.WriteLine(new string(borderChar, starmap.GetLength(1) + 2));

      for (int lineLoop = 0; lineLoop < starmap.GetLength(0); lineLoop++)                         // first dimension size
      {
        Console.ResetColor();
        Console.Write(borderChar);

        Console.BackgroundColor = ConsoleColor.DarkBlue;
        Console.ForegroundColor = ConsoleColor.White;

        for (int columnLoop = 0; columnLoop < starmap.GetLength(1); columnLoop++)                 // second dimension size
        {
          if (starmap[lineLoop, columnLoop])
          {
            Console.Write('*');
          }
          else
          {
            if (InsideRectangle(lineLoop, columnLoop))
            {
              Console.ForegroundColor = ConsoleColor.Yellow;
              Console.Write('░');
              Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
              Console.Write(' ');
            }
          }
        }

        Console.ResetColor();
        Console.Write(borderChar);
        Console.WriteLine();
      }

      Console.ResetColor();
      Console.WriteLine(new string(borderChar, starmap.GetLength(1) + 2));
    }

    private static Rectangle Processmap(bool[,] starmap)
    {
      var rectangles = new List<Rectangle>();

      for (int lineLoop = 0; lineLoop < starmap.GetLength(0); lineLoop++)                         // first dimension size
      {
        Rectangle rect = null;

        for (int columnLoop = 0; columnLoop < starmap.GetLength(1); columnLoop++)                 // second dimension size
        {
          if (starmap[lineLoop, columnLoop])
          { // a star found
            if (rect != null)
            { // end of blank space of first line. 
              rect.width = columnLoop - rect.col;
              rectangles.Add(rect);

              LookingForFullArea(starmap, rect);

              rect = null;
            }            
          }
          else
          { // piece of blank sky found
            if (rect == null)
            {
              rect        = new Rectangle();
              rect.row    = lineLoop;
              rect.col    = columnLoop;
            }
          }
        }

        if (rect != null)
        { // end of blank space of first line because line is ran out. 
          rect.width = starmap.GetLength(1) - rect.col;
          rectangles.Add(rect);

          LookingForFullArea(starmap, rect);

          rect = null;
        }
      }

      return BiggestRectangle(rectangles);
    }

    private static void LookingForFullArea(bool[,] starmap, Rectangle rectangle)
    {
      rectangle.height = 0;

      for (int lineLoop = rectangle.row; lineLoop < starmap.GetLength(0); lineLoop++)                         // first dimension size
      {
        for (int columnLoop = rectangle.col; columnLoop < rectangle.col + rectangle.width; columnLoop++)      // second dimension size
        {
          if (starmap[lineLoop, columnLoop])
          {
            // break two times;                                                                               // This line contains star si it isn't part of area already.
            goto breakbreak;
          }
        }

        rectangle.height++;                                                                                   // more line where we found only blank in own columns.
      }

      breakbreak:;

      #if DEBUG
      DisplayMap(starmap, rectangle);
      #endif
    }

    private static Rectangle BiggestRectangle(List<Rectangle> rectangles)
    {
      Rectangle biggest;

      bool IsBigger(Rectangle second)
      {
        if ((biggest.height * biggest.width) < (second.height * second.width))                    // algorithm for definition of 'bigger'
        { 
          return true;
        }

        return false;
      }

      //

      if ((rectangles == null) || (rectangles.Count < 1))
      {
        return null;
      }

      biggest = rectangles[0];

      foreach (Rectangle rectangle in rectangles)
      {
        if (IsBigger(rectangle))
        {
          biggest = rectangle;
        }
      }

      return biggest;
    }
  }
}