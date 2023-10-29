using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TournamentRandom
{
    class Config
    {
        private string[] names;
        private int[,] mx;
        private int seed;

        public Config(int seed, params string[] names)
        {
            this.names = names;
            mx = new int[names.Length, names.Length];
            this.seed = seed;

            for (int i = 0; i < names.Length; i++)
                for (int j = 0; j < names.Length; j++)
                    mx[i, j] = i == j ? 0 : -1;
        }

        private Config() { }

        public static Config FromFile(string filename)
        {
            var f = File.ReadAllLines(filename).Where(s => s.Length > 0).ToArray();

            int n = int.Parse(f[0]); // 0
            Config res = new Config();
            res.names = new string[n];
            for (int i = 1; i <= n; i++) // n
                res.names[i - 1] = f[i];

            res.seed = int.Parse(f[n + 1]); // n + 1
            res.mx = new int[n, n];

            int k = n + 2;
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    res.mx[i, j] = int.Parse(f[k++]);

            return res;
        }

        public void Save(string cfgfile)
        {
            var f = new List<string>();

            f.Add(names.Length.ToString());
            f.AddRange(names);
            f.Add(seed.ToString());

            for (int i = 0; i < names.Length; i++)
                for (int j = 0; j < names.Length; j++)
                    f.Add(mx[i, j].ToString());

            File.WriteAllLines(cfgfile, f);
        }

        public bool Finish()
        {
            for (int i = 0; i < names.Length; i++)
                for (int j = 0; j < names.Length; j++)
                    if (mx[i, j] == -1)
                        return false;
            return true;
        }

        public (int, int) GetNext()
        {
            Random r = new Random(seed);
            while(true)
            {
                int a = r.Next(names.Length);
                int b = r.Next(names.Length);

                if (a != b && mx[a, b] == -1)
                    return (a, b);
            }
        }

        public string GetName(int who)
            => names[who];

        public void SetResult(int a, int b, int res)
        {
            mx[a, b] = res;
        }

        public string GetReport()
        {
            StringBuilder result = new StringBuilder();

            for(int i = 0; i < names.Length; i++)
            {
                int sum = 0;
                for (int j = 0; j < names.Length; j++)
                    sum += mx[i, j];
                result.AppendFormat("{0} has {1} points\n", names[i], sum);
            }

            return result.ToString();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Config file: ");
            string name = Console.ReadLine().Trim();
            name += ".cfg";

            FileInfo fi = new FileInfo(name);

            Console.WriteLine("If does not exist, print names:");
            Config cfg = fi.Exists ? Config.FromFile(name) : new Config(new Random().Next(), Console.ReadLine().Split().Where(s => s.Length > 0).ToArray());
            cfg.Save(name);

            Console.WriteLine("Ok");
            while(!cfg.Finish())
            {
                var (a, b) = cfg.GetNext();
                Console.WriteLine("{0} vs {1}", cfg.GetName(a), cfg.GetName(b));
                string r = Console.ReadLine().Trim();
                if (r == "1")
                {
                    cfg.SetResult(a, b, 1);
                    cfg.Save(name);
                }
                else if (r == "0")
                {
                    cfg.SetResult(a, b, 0);
                    cfg.Save(name);
                }
                else
                {
                    cfg.Save(name);
                    Console.WriteLine("exit");
                    return;
                }
            }

            cfg.Save(name);
            Console.WriteLine("Finished");
            Console.WriteLine(cfg.GetReport());
        }
    }
}
