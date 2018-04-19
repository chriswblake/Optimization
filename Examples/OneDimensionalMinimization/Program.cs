using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Optimization;
using UMM = Optimization.UnimodalMinimization;
using DiffSharp.Interop.Float64;


namespace OneDimensionalMinimization
{
    class Program
    {
        static void Main(string[] args)
        {
            //Objective Function for testing
            //double[] epsValues = { 0.1, 0.01, 0.001 }; //accuracy
            //double a = -100, b = 100;
            //Func<D, D> f = delegate (D x) {
            //    return AD.Pow(x - 7, 2);
            //};

            //Objective function for class           //f(x) = 10*x*ln(x)-x^2/2,  x E[0.2,1]
            List<double> epsValues = new List<double> { 0.1, 0.01, 0.001 }; //accuracy        
            double a = 0.2, b = 1; //range
            Func<D,D> f = delegate (D x)
            {
                double Fx = 10 * x * AD.Log(x) - AD.Pow(x, 2) / 2;
                return Fx;
            };

            #region Direct Uniform Search
            //Show the table header
            Console.WriteLine("-----Direct Uniform Search-----");
                Console.WriteLine((new UMM.SearchResult()).getTableHeader()); // console

                //Loop through all combinations of accuracy and interval
                List<int> intervals = new List<int> { 6, 10, 15, 20, 25, 30, 35, 40, 45, 50 };
                foreach (double eps in epsValues)
                    foreach (int n in intervals)
                    {
                        //Calculate results
                        UMM.SearchResult d = UMM.directUniformSearch(f, a, b, n, eps);

                        //Display results on console
                        Console.WriteLine(d.getTabbedResults());
                    
                    }
                #endregion

            #region Dichotomy Search
            Console.WriteLine();
            Console.WriteLine("-----Dichotomy Search-----");
            Console.WriteLine((new UMM.SearchResult()).getTableHeader()); // console

            //Loop through all accuracy options
            foreach (double eps in epsValues)
            {
                //Calculate results
                UMM.SearchResult d = UMM.dichotomySearch(f, a, b, eps);

                //Display results on console
                Console.WriteLine(d.getTabbedResults());
            }
            #endregion

            #region Golden Ration Search
            Console.WriteLine();
            Console.WriteLine("-----Golden Ratio Search-----");
            Console.WriteLine((new UMM.SearchResult()).getTableHeader()); // console

            //Loop through all accuracy options
            foreach (double eps in epsValues)
            {
                //Calculate results
                UMM.SearchResult d = UMM.goldenRatioSearch(f, a, b, eps);

                //Display results on console
                Console.WriteLine(d.getTabbedResults());
            }
            #endregion

            //Wait for user to exit
            Console.ReadKey();
        }
    }
}
