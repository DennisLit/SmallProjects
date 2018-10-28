using Newtonsoft.Json.Linq;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;

namespace StudyTime.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Type in a group name!");
            string groupName = Console.ReadLine();

            var result = RequestsHelper.GroupsScheduleByGroupName(groupName);

            if(string.IsNullOrWhiteSpace(result))
            {
                Console.WriteLine("Wrong group name!");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Do u want the whole month to be calculated? y/n");

            string isWholeMonthInput = Console.ReadLine();

            if ((isWholeMonthInput.ToUpper() != "N") && (isWholeMonthInput.ToUpper() != "Y"))
            {
                Console.WriteLine("Wrong input!");
                Console.ReadLine();
                return;
            }

            var IsWholeMonth = false;

            string week = "1";

            if (isWholeMonthInput.ToUpper() == "N")
            {
                IsWholeMonth = false;
                Console.WriteLine("Type in a week! 1 - 4");
                week = Console.ReadLine();
            }
            else
                IsWholeMonth = true;



            var jsonResult = result.ToJObject();

            var parser = new BsuirParser();

            if(!int.TryParse(week, out var weekInt))
            {
                Console.WriteLine("Wrong week!");
                Console.ReadLine();
                return;
            }

            var parseResult = parser.Parse(jsonResult, (byte)weekInt, IsWholeMonth);

            if (parseResult == null)
            {
                Console.WriteLine("Wrong week!");
                Console.ReadLine();
                return;
            }


            int sum = 0;

            foreach(var subj in parseResult)
            {
                sum += subj.Value;
            }

            var sortedRes = parseResult.OrderByDescending(x => x.Value).ToList();
            
            foreach (var subj in sortedRes)
            {
                Console.WriteLine("Subject : {0}, Total hours: {1}, Percentage: {2} %", subj.Key, (float)subj.Value / 60, ((float)subj.Value / sum) * 100);
            }

            Console.ReadLine();
        }
    }
}
