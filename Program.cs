using Shift.Demo.Jobs;
using Shift.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Shift.Demo.Client
{
    class Program
    {
        private static JobClient jobClient;
        private static IList<int> addedJobIDs;
        static void Main(string[] args)
        {
            InitShiftClient();

            ConsoleKeyInfo cki;
            do
            {
                
                cki = DisplayMenu();  // show the key as you read it
                switch (cki.KeyChar.ToString())
                {
                    case "1":
                        AddJob();
                        break;
                    case "2":
                        ShowJobsProgress();
                        break;
                    case "3":
                        StopJobs();
                        break;
                    case "4":
                        ResetJobs();
                        break;
                    case "5":
                        DeleteJobs();
                        break;
                }
            } while (cki.Key != ConsoleKey.Escape);

            //clean up jobs when exiting
            if(addedJobIDs.Count > 0)
            {
                DeleteJobs();
            }
        }

        static public ConsoleKeyInfo DisplayMenu()
        {
            Console.WriteLine();
            Console.WriteLine("Shift Client Demo");
            Console.WriteLine("1. Add a Hello World job.");
            Console.WriteLine("2. Show progress for Hello World job(s).");
            Console.WriteLine("3. Send 'STOP' command to Hello World job(s).");
            Console.WriteLine("4. Reset Hello World job(s).");
            Console.WriteLine("5. Delete Hello World job(s).");
            Console.WriteLine("Press escape (ESC) key to exit.");
            return Console.ReadKey(false);
        }

        private static void InitShiftClient()
        {
            var options = new Shift.Options();
            options.DBConnectionString = "Data Source=localhost\\SQL2014;Initial Catalog=BGProcess;user=bguser; password=bguser"; //should be in config or DB
            options.CacheConfigurationString= "localhost:6379,password=LZLxuFbuPCdxcizNuuDJ0EdoXit1YHoiln8lsTVzPgGTeNB1DkoETMeCZI3FNjvQ"; //should be in config

            jobClient = new JobClient(options);

            addedJobIDs = new List<int>();
        }

        private static void AddJob()
        {
            var job = new TestJob();
            var progress = new SynchronousProgress<ProgressInfo>();
            var jobID = jobClient.Add("Shift.Demo.Client", -100, "Demo.Job", () => job.Start("Hello World", progress));
            addedJobIDs.Add(jobID.GetValueOrDefault());
            Console.WriteLine();
            Console.WriteLine("==> New Job added to Shift DB table");
        }

        private static void StopJobs()
        {
            jobClient.SetCommandStop((List<int>)addedJobIDs);

            Console.WriteLine();
            Console.WriteLine("==> Send 'STOP' command to Job(s) on Shift DB is completed.");
        }

        private static void DeleteJobs()
        {
            jobClient.DeleteJobs((List<int>)addedJobIDs);

            Console.WriteLine();
            Console.WriteLine("==> Delete Job(s) on Shift DB is completed.");
        }

        private static void ResetJobs()
        {
            jobClient.ResetJobs((List<int>)addedJobIDs);

            Console.WriteLine();
            Console.WriteLine("==> Reset Job(s) on Shift DB table is completed.");
        }

        private static void ShowJobsProgress()
        {
            if (addedJobIDs.Count > 0)
            {
                Console.Clear();
                origRow = Console.CursorTop;
                origCol = Console.CursorLeft;
                WriteAt("*** Press ESC to exit progress ***", 0, 0);
                do
                {
                    var index = 1;
                    foreach (var jobID in addedJobIDs)
                    {
                        var progress = jobClient.GetProgress(jobID);
                        if (progress != null)
                        {
                            var msg = progress.JobID + ": " + string.Format("{0}%", progress.Percent.GetValueOrDefault()) + " " + progress.StatusLabel + " " + progress.Updated.GetValueOrDefault().ToString("MM/dd/yy hh:mm:ss tt");
                            WriteAt(msg, 0, index);
                            index++;
                        }
                        Thread.Sleep(1000);
                    }
                }
                while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape));

                Console.WriteLine();
                Console.WriteLine("==> Get Job(s) Progress is completed.");
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("==> No Job(s) Progress found.");
            }

        }

        protected static int origRow;
        protected static int origCol;
        protected static void WriteAt(string s, int x, int y)
        {
            try
            {
                Console.SetCursorPosition(origCol + x, origRow + y);
                Console.Write(s);
            }
            catch (ArgumentOutOfRangeException e)
            {
                Console.Clear();
                Console.WriteLine(e.Message);
            }
        }
    }
}
