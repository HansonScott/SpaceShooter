using System;
using System.Collections.Generic;
using System.Text;

namespace GraphicsApp
{
    class MyRnd
    {
        private static Random rnd = new Random();

        public static void test(int count) {
            int testFigure = 1000;
            int countQ1 = 0;
            int countQ2 = 0;
            int countQ3 = 0;
            int countQ4 = 0;
            for (int i = 0; i < count; i++) {
                int tmp = rnd.Next(testFigure);
                if (tmp < (1 * testFigure / 4)) { countQ1 += 1;}
                else if (tmp < (2 * testFigure / 4)) { countQ2 += 1; }
                else if (tmp < (3 * testFigure / 4)) { countQ3 += 1; }
                else { countQ4 += 1; }
            } // end for loop

            GameWindow.Tell("Test Results:\nQ1: " + countQ1 +
                                         "\nQ2: " + countQ2 +
                                         "\nQ3: " + countQ3 +
                                         "\nQ4: " + countQ4);
        } // end test
        public static int next(int max)
        {
            return rnd.Next(max) + 1;
        }
    } // end class
} // end namespace
