using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiffSharp.Interop.Float64;
using Optimization;

namespace GradientDescent
{
    class Program
    {
        static void Main(string[] args)
        {
            #region Intro
            Console.WriteLine(@"
///////////////////////////////////////
Task: Lab 2 - Gradient Descent
Written By: Christopher W. Blake
Date: 22 Nov. 2016
///////////////////////////////////////
Description:
Creates 2 different seach algorithms for finding
the minimum of a given function f(x) in a range. It then tests
these results and prints them to the console.

1. First-order gradient descent method.
   (one – dimensional minimization method for choosing the step)

2. Second-order gradient descent method
   (division method for choosing the step)

Verification: f(0.5, -0.44963) = 0.27696
///////////////////////////////////////
");
            #endregion

            //Required accuracy values
            List<double> epsValues = new List<double> { 0.1, 0.01, 0.001 }; //accuracy        

            //Objective function
            Func<DV,D> f = delegate (DV x)
            {
                D x1 = x[0];
                D x2 = x[1];
                return x1 * x1 + x2 * x2 + AD.Exp(x2 * x2) - x1 + 2 * x2;
                //return AD.Pow(x1-7, 2) + AD.Pow(x2-3, 2);
            };
            
            #region 1.) First Order, One-Dimensional Method         
            //Show the table header
            Console.WriteLine("----- Gradient Search, First Order, One-Dimensional Method -----");
            Console.WriteLine("     eps      X1      X2    f(x)   Calcs F   Calcs Gr");
            foreach (double eps in epsValues)
            {
                //Get solution
                int calcsF;
                int calcsGradient;
                DV startPoint = new DV(new D[] {0,0});
                DV xMin = Optimization.GradientDescent.FirstOrder_OneDimensionalMethod(f, startPoint, eps, out calcsF, out calcsGradient);

                //determine number of decimal places to show
                int dp = BitConverter.GetBytes(decimal.GetBits((decimal)eps)[3])[2] + 1;

                //Show on console
                Console.WriteLine("{0,8}{1,8:F"+dp+"}{2,8:F" + dp + "}{3,8:F" + dp + "}{4,8}{5,8}", eps, (double)xMin[0], (double) xMin[1], (double) f(xMin), calcsF, calcsGradient);

            }
            #endregion

            DV[] xFirstOrder_DivMethod = null;
            double[] fxFirstOrder_DivMethod = null;
            #region 2.) First Order, Division Method
            //Show the table header
            Console.WriteLine();
            Console.WriteLine("----- Gradient Search, First Order, Division Method -----");
            Console.WriteLine("     eps      X1      X2    f(x)   Calcs F   Calcs dF");
            foreach (double eps in epsValues)
            {
                //Get solution
                int calcsF;
                int calcsGradient;
                DV startPoint = new DV(new D[] { 0, 0 });
                DV xMin = Optimization.GradientDescent.FirstOrder_DivisionMethod(f, startPoint, eps, out calcsF, out calcsGradient, out xFirstOrder_DivMethod, out fxFirstOrder_DivMethod);

                //determine number of decimal places to show
                int dp = BitConverter.GetBytes(decimal.GetBits((decimal)eps)[3])[2] + 1;

                //Show on console
                Console.WriteLine("{0,8}{1,8:F" + dp + "}{2,8:F" + dp + "}{3,8:F" + dp + "}{4,8}{5,8}", eps, (double)xMin[0], (double)xMin[1], (double)f(xMin), calcsF, calcsGradient);

            }
            #endregion

            DV[] xSecondOrder_FullStep = null;
            double[] fxSecondOrder_FullStep = null;
            #region 3.) Second Order - Newtons Method, FullStep
            //Show the table header
            Console.WriteLine();
            Console.WriteLine("----- Gradient Search, Second Order, FullStep -----");
            Console.WriteLine("     eps      X1      X2    f(x)   Calcs F  Calcs Gr  Calcs Hess");

            //Show Results for each accuracy
            foreach (double eps in epsValues)
            {
                //Get solution
                int calcsF;
                int calcsGradient;
                int calcsHessian;
                DV startPoint = new DV(new D[] {0, 0});
                DV xMin = Optimization.GradientDescent.SecondOrder_FullStep(f, startPoint, eps, out calcsF, out calcsGradient, out calcsHessian, out xSecondOrder_FullStep, out fxSecondOrder_FullStep);

                //determine number of decimal places to show
                int dp = BitConverter.GetBytes(decimal.GetBits((decimal)eps)[3])[2] + 1;

                //Show on console
                Console.WriteLine("{0,8}{1,8:F" + dp + "}{2,8:F" + dp + "}{3,8:F" + dp + "}{4,8}{5,8}{6,8}", eps, (double)xMin[0], (double)xMin[1], (double)f(xMin), calcsF, calcsGradient, calcsHessian);

            }
            #endregion

            #region 4.) Second Order - Newtons Method, Division Method
            //Show the table header
            Console.WriteLine();
            Console.WriteLine("----- Gradient Search, Second Order, Division Method -----");
            Console.WriteLine("     eps      X1      X2    f(x)   Calcs F  Calcs Gr  Calcs Hess");

            //Show Results for each accuracy
            foreach (double eps in epsValues)
            {
                //Get solution
                int calcsF;
                int calcsGradient;
                int calcsHessian;
                DV startPoint = new DV(new D[] { 0, 0 });
                DV xMin = Optimization.GradientDescent.SecondOrder_DivisionMethod(f, startPoint, eps, out calcsF, out calcsGradient, out calcsHessian);

                //determine number of decimal places to show
                int dp = BitConverter.GetBytes(decimal.GetBits((decimal)eps)[3])[2] + 1;

                //Show on console
                Console.WriteLine("{0,8}{1,8:F" + dp + "}{2,8:F" + dp + "}{3,8:F" + dp + "}{4,8}{5,8}{6,8}", eps, (double)xMin[0], (double)xMin[1], (double)f(xMin), calcsF, calcsGradient, calcsHessian);

            }
            #endregion


            //Below is all extra work that she asked for (for fun) in class

            #region 5.) Speed Comparison, First Order, Div method
            DV xStarFirstOrder_DivMethod = xFirstOrder_DivMethod[xFirstOrder_DivMethod.Length - 1];

            Console.WriteLine();
            Console.WriteLine("---- Speed (q), First Order, Division Method --- ");
            Console.WriteLine("K         X1        X2      qX1      qX2");
            for (int k=0; k < xFirstOrder_DivMethod.Length-1; k++)
            {
                //Display row information
                Console.Write("{0}: {1,10:F3}{2,10:F3}", k, (double) xFirstOrder_DivMethod[k][0], (double)xFirstOrder_DivMethod[k][1]);

                if (k > 0 && k < xFirstOrder_DivMethod.Length - 2)
                {
                    DV x = xFirstOrder_DivMethod[k] - xStarFirstOrder_DivMethod;
                    DV xNext = xFirstOrder_DivMethod[k - 1] - xStarFirstOrder_DivMethod;

                    double qX1 = AD.Abs(xNext[0] - xStarFirstOrder_DivMethod[0]);
                    double qX2 = AD.Abs(xNext[1] - xStarFirstOrder_DivMethod[1]);

                    Console.Write("{0,10:F3}{1,10:F3}", qX1, qX2);
                }

                Console.WriteLine();

            }
            #endregion

            #region 6.) Speed Comparison, Second Order, Full Step
            DV xStarSecondOrder_FullStep = xSecondOrder_FullStep[xSecondOrder_FullStep.Length - 1];

            Console.WriteLine();
            Console.WriteLine("---- Speed (q), Second Order, Full Step --- ");
            Console.WriteLine("K         X1        X2      qX1      qX2");
            for (int k = 0; k < xSecondOrder_FullStep.Length - 1; k++)
            {
                //Display row information
                Console.Write("{0}: {1,10:F3}{2,10:F3}", k, (double)xSecondOrder_FullStep[k][0], (double)xSecondOrder_FullStep[k][1]);

                if (k > 0 && k < xSecondOrder_FullStep.Length - 2)
                {
                    DV x = xSecondOrder_FullStep[k] - xStarSecondOrder_FullStep;
                    DV xNext = xSecondOrder_FullStep[k - 1] - xStarSecondOrder_FullStep;

                    double qX1 = AD.Abs(xNext[0] - xStarSecondOrder_FullStep[0]);
                    double qX2 = AD.Abs(xNext[1] - xStarSecondOrder_FullStep[1]);

                    Console.Write("{0,10:F3}{1,10:F3}", qX1, qX2);
                }

                Console.WriteLine();

            }

            #endregion

            

            //Wait for use to click something to exit
            Console.ReadKey();

        }

        static void Main_Testing(string[] args)
        {
            //Functions
            Func<DV, D> F1 = delegate (DV x) {
                D x1 = x[0];

                return AD.Pow(x1 - 7, 2);
            };
            Func<DV, D> F2 = delegate (DV x) {
                D x1 = x[0];
                D x2 = x[1];

                return AD.Pow(x1, 2) + AD.Pow(x2, 3);
            };

            //Test poing
            //DV thePoint = new DV(new double[] { 10, 10 });
            DV thePoint = new DV(new double[] { 10 });
            Console.WriteLine(" f(10) = {0}", F1(thePoint));

            var v = AD.Grad(F1, thePoint);
            Console.WriteLine("Version 1");
            Console.WriteLine("  dx1 = {0}", v[0]);
            //Console.WriteLine("  dx2 = {0}", v[1]);

            var h = AD.Hessian(F1, thePoint);
            Console.WriteLine("\nVersion 2");
            Console.WriteLine(" d2x1 = {0}", h[0, 0]);
            //Console.WriteLine(" d2x2 = {0}", h[1, 1]);

            Console.ReadKey();
        }
    }
}
