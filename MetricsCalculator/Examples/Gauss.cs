using System;

namespace MV_Pract_II_1_Gauss_V
{
    class Program
    {
        static void RunStraight(decimal[,] mx, int n)
        {
            for(int i = 0; i < n; i++)
            {
                int k = i;
                for (int r = i + 1; r < n; r++)
                    if (Math.Abs(mx[r, i]) > Math.Abs(mx[k, i]))
                        k = r;
                for(int j = i; j <= n; j++)
                {
                    var t = mx[k, j];
                    mx[k, j] = mx[i, j];
                    mx[i, j] = t;
                }

                for (int j = i + 1; j <= n; j++)
                    mx[i, j] /= mx[i, i];
                mx[i, i] = 1;

                for(int r = i + 1; r < n; r++)
                {
                    for (int j = i + 1; j <= n; j++)
                        mx[r, j] -= mx[i, j] * mx[r, i];
                    mx[r, i] = 0;
                }
            }
        }

        static decimal[] Solve(decimal[,] mx, int n)
        {
            //mx = mx.Clone() as decimal[,];
            RunStraight(mx, n);
            // Reverse:
            decimal[] result = new decimal[n];
            for(int i = n - 1; i >= 0; i--)
            {
                result[i] = mx[i, n];
                for (int j = i + 1; j < n; j++)
                    result[i] -= result[j] * mx[i, j];
            }
            return result;
        }

        static decimal[,] MakeMatrix(decimal v, int n)
        {
            decimal[,] result = new decimal[n, n + 1];
            const decimal mult = 0.01m;

            for (int i = 0; i < n; i++)
            {
                decimal e1 = v + i;
                decimal e2 = e1 * mult;
                for (int j = 0; j < n; j++)
                    result[i, j] = i == j ? e1 : e2;
            }

            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    result[i, n] += result[i, j] * result[j, j];

            return result;
        }

        static void Main(string[] args)
        {
            const decimal v = 3;
            Console.Write("n = ");
            int n = int.Parse(Console.ReadLine().Trim());

            var mx = MakeMatrix(v, n);
            //Console.WriteLine("Система уравнений:");
            //
            //for(int i = 0; i < n; i++)
            //{
            //    for (int j = 0; j < n; j++)
            //        Console.Write(mx[i, j] + " ");
            //    Console.WriteLine("| " + mx[i, n]);
            //}
            
            var result = Solve(mx, n);
            Console.WriteLine("Вектор-результат:");
            foreach (var e in result)
                Console.Write("{0:0000.0000000000} ", e);
            Console.ReadKey();
        }
    }
}
