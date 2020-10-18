using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace YOGIYOCrawler
{
    class Program
    {

        private delegate void Func(string[] arg);
        private static Dictionary<string, Func> commands = new Dictionary<string, Func>();
        protected static Crawler crawler = null;

        static void Main(string[] args)
        {
            Init();
            GetInput();
        }

        private static void Init()
        {
            crawler = new Crawler();
            commands.Clear();
            commands.Add("click", (string[] arg) => crawler.Click());
            commands.Add("data", (string[] arg) => crawler.GetData(arg[0]));
            commands.Add("child", (string[] arg) => crawler.GetChild(arg[0], arg[1]));
            commands.Add("list", (string[] arg) => crawler.MakeList());
            commands.Add("attr", (string[] arg) => crawler.GetAttr(arg[0]));
            commands.Add("css", (string[] arg) => crawler.GetCss(arg[0]));
        }

        private static void GetInput()
        {
            Console.Write("Command : ");
            string str = Console.ReadLine();
            try
            {
                ProcessInput(str.Trim());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                GetInput();
            }
        }

        private static void ProcessInput(string command)
        {
            var tmp = command.Split(" ");

            foreach(var e in tmp) {
                Console.WriteLine(e);
            }
            

            string[] args = new string[tmp.Length - 1];

            string comm = tmp[0];
            for(int i = 1; i<tmp.Length; i++)
            {
                args[i - 1] = tmp[i];
            }
            tmp = null;

            if (!commands.ContainsKey(comm))
                throw new InvalidOperationException($"no such command : {comm}");
            else
                commands[comm](args);
        }
    }
}
