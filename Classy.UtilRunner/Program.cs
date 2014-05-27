using System;
using System.Linq;
using System.Reflection;
using classy.Extensions;
using Funq;
using ServiceStack.Common;

namespace Classy.UtilRunner
{
    public class Program
    {
        private static Container _container;

        // First parameter should be the name of the utility to run
        // the rest of the parameters will be passed to the utility
        public static void Main(string[] args)
        {
            var utilityName = args[0];
            var utilityToRun = GetUtilityToRun(utilityName);
            if (utilityToRun == null)
            {
                Console.WriteLine("Couldn't find/resolve utility : " + utilityName);
                return;
            }
            
            var utility = Activator.CreateInstance(utilityToRun, _container) as IUtility;
            if (utility == null)
            {
                Console.WriteLine("Couldn't find/resolve utility : " + utilityName);
                return;
            }

            Console.WriteLine("This will run the utility '{0}' - Are you sure ?", utilityName);
            var input = Console.ReadLine();

            if (!input.IsNullOrEmpty() && !input.ToLower().StartsWith("y"))
                return;

            var startTime = DateTime.Now;
            Console.WriteLine("Starting to run utility '{0}' :: {1}", utilityName, startTime.TimeOfDay);
            var status = StatusCode.Failure;
            try
            {
                status = utility.Run(args.Skip(1).ToArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while running utility :: " + ex.Message);
            }
            Console.WriteLine("Ended with status {0} :: {1}", status, DateTime.Now);
            Console.WriteLine("Time elapsed :: {0} seconds", (DateTime.Now - startTime).TotalSeconds);
            Console.ReadKey(); // add option not to wait for input too
        }

        private static Type GetUtilityToRun(string utilityName)
        {
            _container = new Container();
            _container.WireUp();

            var utility = Assembly.GetExecutingAssembly()
                .GetTypes().Where(x => typeof (IUtility).IsAssignableFrom(x))
                .FirstOrDefault(x => x.Name == utilityName);

            return utility;
        }
    }
}
