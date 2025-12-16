using Newtonsoft.Json;
using ScottPlot;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace DiscordDataSummarizer
{

    internal class MessageObject
    {

        public long ID;
        public DateTime Timestamp;
        public String Contents;
    }

    internal class Program
    {

        static void Main()
        {

            // Get directory info from user
            Console.WriteLine("Please enter directory/path of unzipped discord data packet\nExample: C:\\Downloads\\package\nJust copy paste it or something");
            string read_path = @"" + Console.ReadLine();
            if (!Directory.Exists(read_path))
            {

                Console.WriteLine("FAIL: Path does not exist\nPress enter to exit.");
                Console.ReadLine(); // Wait for input
                throw new Exception("Path doesnt exist");
            }
            else if (!File.Exists(read_path + "\\messages\\index.json")) // Ensure path has messages and index.json
            {

                Console.WriteLine("FAIL: Path does not contain 'messages\\index.json', this may not be a data packet, or it may be missing messages\npress enter to exit.");
                Console.ReadLine(); // Wait for input
                throw new Exception("Path not data packet");
            }

            // Get debug info
            Console.WriteLine("Show extra debug info? Y/N");
            string read = @"" + Console.ReadLine(); // Worlds worst implementation idk how ppl normally do this
            bool show_extra_debug = (read) == "Y" || (read == "y");

            // Get debug info
            Console.WriteLine("Create graphs for message activity? Y/N\nNote: this will create a file in " + read_path + " and will open the file upon completion.");
            string read_graph = @"" + Console.ReadLine(); // Still the worlds worst implementation
            bool do_graphing = (read_graph) == "Y" || (read_graph == "y");

            DoStuff(read_path, show_extra_debug, do_graphing);
        }

        static void DoStuff(string dir, bool show_extra_debug, bool do_graphing)
        {

            int MessagesIn2025 = 0;
            int MessagesIn2024 = 0;
            int MessagesIn2023 = 0;
            int MessagesIn2022 = 0;
            int MessagesIn2021 = 0;
            int MessagesIn2020 = 0;

            SortedDictionary<long, long> MessagesPerChannel2025 = new SortedDictionary<long, long>();
            SortedDictionary<long, long> MessagesPerChannel2024 = new SortedDictionary<long, long>();
            SortedDictionary<long, long> MessagesPerChannel2023 = new SortedDictionary<long, long>();
            SortedDictionary<long, long> MessagesPerChannel2022 = new SortedDictionary<long, long>();
            SortedDictionary<long, long> MessagesPerChannel2021 = new SortedDictionary<long, long>();
            SortedDictionary<long, long> MessagesPerChannel2020 = new SortedDictionary<long, long>();

            // graph plotting data, wont be written to unless do_graphing is true
            ScottPlot.Plot summaryPlot = new();
            List<double> xs = new List<double>();
            List<double> ys = new List<double>();

            // Setup vertical ticks
            ScottPlot.TickGenerators.NumericManual verticalTicks = new();

            verticalTicks.AddMajor(0, "00:00");
            verticalTicks.AddMajor(0.25, "06:00");
            verticalTicks.AddMajor(0.5, "12:00");
            verticalTicks.AddMajor(0.75, "18:00");
            verticalTicks.AddMajor(1, "24:00");

            // Setup title
            summaryPlot.Title("Lifetime messages scatter (UTC)");
            summaryPlot.Axes.Title.FullFigureCenter = false;

            // setup numeric tick generator
            ScottPlot.TickGenerators.NumericManual yearTicks = new();

            // Too lazy to learn hashset
            SortedDictionary<int, bool> YearsWithData = new SortedDictionary<int, bool>();
            int yearsAmount = 0;

            Console.WriteLine("Processing messages, this may take a while");

            StreamReader shitr = new StreamReader(dir + "\\messages\\index.json");
            string jsonr = shitr.ReadToEnd();
            SortedDictionary<long, string> channel_map = JsonConvert.DeserializeObject<SortedDictionary<long, string>>(jsonr);

            if (channel_map == null)
            {

                Console.WriteLine("FAIL: Something went really wrong, and the program was unable to parse " + dir + "\\messages\\index.json\nprocessing cannot continue without this, please try again or make a github issue or something idk if u really want\nPress enter to exit.");
                Console.ReadLine(); // Wait for input
                throw new Exception("Bad index");
            }

            // Do stuff here
            foreach (string path in Directory.GetDirectories(dir + "\\messages"))
            {

                // Check to see if its a folder
                string shitass = Path.GetFileName(path).Substring(1);
                long channel_id = long.Parse(shitass);

                MessagesPerChannel2025[channel_id] = 0;
                MessagesPerChannel2024[channel_id] = 0;
                MessagesPerChannel2023[channel_id] = 0;
                MessagesPerChannel2022[channel_id] = 0;
                MessagesPerChannel2021[channel_id] = 0;
                MessagesPerChannel2020[channel_id] = 0;

                if (show_extra_debug)
                {

                    Console.WriteLine(channel_id.ToString() + ": " + channel_map[channel_id]);
                }
                
                using (StreamReader r = new StreamReader(path + "\\messages.json"))
                {

                    string json = r.ReadToEnd();
                    List<MessageObject> result = JsonConvert.DeserializeObject<List<MessageObject>>(json);
                    if (result == null)
                    {

                        Console.WriteLine("FAIL: Something went wrong parsing file: " + path + "\\messages.json\nPress enter to exit.");
                        Console.ReadLine(); // Wait for input
                        throw new Exception("Bad message");
                    }

                    foreach (MessageObject msg in result.ToArray())
                    {

                        if (do_graphing) {

                            DateTime timestamp = msg.Timestamp;
                            ys.Add((double)timestamp.TimeOfDay.TotalSeconds / 86400);
                            xs.Add((double)timestamp.Year + ((double)timestamp.DayOfYear / 365));
                            //Console.WriteLine(timestamp.TimeOfDay);

                            if (!YearsWithData.ContainsKey(timestamp.Year))
                            {

                                int year = timestamp.Year;
                                yearsAmount = yearsAmount + 1;
                                YearsWithData[timestamp.Year] = true;
                                yearTicks.AddMajor((float)year, year.ToString() + " Jan");
                                yearTicks.AddMajor(year + 0.0833, "Feb");
                                yearTicks.AddMajor(year + 0.1666, "Mar");
                                yearTicks.AddMajor(year + 0.25, "Apr");
                                yearTicks.AddMajor(year + 0.3333, "May");
                                yearTicks.AddMajor(year + 0.4166, "Jun");
                                yearTicks.AddMajor(year + 0.5, "July");
                                yearTicks.AddMajor(year + 0.5833, "Aug");
                                yearTicks.AddMajor(year + 0.666666, "Sep");
                                yearTicks.AddMajor(year + 0.75, "Oct");
                                yearTicks.AddMajor(year + 0.83333, "Nov");
                                yearTicks.AddMajor(year + 0.916666, "Dec");
                            }
                        }

                        if (msg.Timestamp.Year == 2025)
                        {

                            //Console.WriteLine(msg.Contents);
                            MessagesIn2025 += 1;
                            MessagesPerChannel2025[channel_id] += 1;
                        }
                        else if (msg.Timestamp.Year == 2024)
                        {

                            //Console.WriteLine(msg.Contents);
                            MessagesIn2024 += 1;
                            MessagesPerChannel2024[channel_id] += 1;
                        }
                        else if (msg.Timestamp.Year == 2023)
                        {

                            //Console.WriteLine(msg.Contents);
                            MessagesIn2023 += 1;
                            MessagesPerChannel2023[channel_id] += 1;
                        }
                        else if (msg.Timestamp.Year == 2022)
                        {

                            //Console.WriteLine(msg.Contents);
                            MessagesIn2022 += 1;
                            MessagesPerChannel2022[channel_id] += 1;
                        }
                        else if (msg.Timestamp.Year == 2021)
                        {

                            //Console.WriteLine(msg.Contents);
                            MessagesIn2021 += 1;
                            MessagesPerChannel2021[channel_id] += 1;
                        }
                        else if(msg.Timestamp.Year == 2020)
                        {

                            //Console.WriteLine(msg.Contents);
                            MessagesIn2020 += 1;
                            MessagesPerChannel2020[channel_id] += 1;
                        }
                        // Console.WriteLine(msg.Timestamp.Year);
                        //Console.WriteLine(msg.Contents);
                    }
                }
            }

            // Add gaps between debug info
            Console.WriteLine("\n\n\n");

            Console.WriteLine("this many messages were in 2025: " + MessagesIn2025.ToString());
            Console.WriteLine("this many messages were in 2024: " + MessagesIn2024.ToString());
            Console.WriteLine("this many messages were in 2023: " + MessagesIn2023.ToString());
            Console.WriteLine("this many messages were in 2022: " + MessagesIn2022.ToString());
            Console.WriteLine("this many messages were in 2021: " + MessagesIn2021.ToString());
            Console.WriteLine("this many messages were in 2020: " + MessagesIn2020.ToString());

            Console.WriteLine("\n");
            Console.WriteLine("WARN!: if there were no messages in a year, it might show random channels here but idk what it does it might just be wrong\n");

            var max_2025 = MessagesPerChannel2025.OrderByDescending(d => d.Value).ToArray();
            Console.WriteLine("most popular channels in 2025: \n    " + 
                "#1: " + max_2025[0].Value.ToString() + " | " + channel_map[max_2025[0].Key] + " \n    " + 
                "#2: " + max_2025[1].Value.ToString() + " | " + channel_map[max_2025[1].Key] + " \n    " +
                "#3: " + max_2025[2].Value.ToString() + " | " + channel_map[max_2025[2].Key] + " \n    " +
                "#4: " + max_2025[3].Value.ToString() + " | " + channel_map[max_2025[3].Key] + " \n    " +
                "#5: " + max_2025[4].Value.ToString() + " | " + channel_map[max_2025[4].Key] + " \n ");

            var max_2024 = MessagesPerChannel2024.OrderByDescending(d => d.Value).ToArray();
            Console.WriteLine("most popular channels in 2024: \n    " +
                "#1: " + max_2024[0].Value.ToString() + " | " + channel_map[max_2024[0].Key] + " \n    " +
                "#2: " + max_2024[1].Value.ToString() + " | " + channel_map[max_2024[1].Key] + " \n    " +
                "#3: " + max_2024[2].Value.ToString() + " | " + channel_map[max_2024[2].Key] + " \n    " +
                "#4: " + max_2024[3].Value.ToString() + " | " + channel_map[max_2024[3].Key] + " \n    " +
                "#5: " + max_2024[4].Value.ToString() + " | " + channel_map[max_2024[4].Key] + " \n ");

            var max_2023 = MessagesPerChannel2023.OrderByDescending(d => d.Value).ToArray();
            Console.WriteLine("most popular channels in 2023: \n    " +
                "#1: " + max_2023[0].Value.ToString() + " | " + channel_map[max_2023[0].Key] + " \n    " +
                "#2: " + max_2023[1].Value.ToString() + " | " + channel_map[max_2023[1].Key] + " \n    " +
                "#3: " + max_2023[2].Value.ToString() + " | " + channel_map[max_2023[2].Key] + " \n    " +
                "#4: " + max_2023[3].Value.ToString() + " | " + channel_map[max_2023[3].Key] + " \n    " +
                "#5: " + max_2023[4].Value.ToString() + " | " + channel_map[max_2023[4].Key] + " \n ");

            var max_2022 = MessagesPerChannel2022.OrderByDescending(d => d.Value).ToArray();
            Console.WriteLine("most popular channels in 2022: \n    " +
                "#1: " + max_2022[0].Value.ToString() + " | " + channel_map[max_2022[0].Key] + " \n    " +
                "#2: " + max_2022[1].Value.ToString() + " | " + channel_map[max_2022[1].Key] + " \n    " +
                "#3: " + max_2022[2].Value.ToString() + " | " + channel_map[max_2022[2].Key] + " \n    " +
                "#4: " + max_2022[3].Value.ToString() + " | " + channel_map[max_2022[3].Key] + " \n    " +
                "#5: " + max_2022[4].Value.ToString() + " | " + channel_map[max_2022[4].Key] + " \n ");

            var max_2021 = MessagesPerChannel2021.OrderByDescending(d => d.Value).ToArray();
            Console.WriteLine("most popular channels in 2021: \n    " +
                "#1: " + max_2021[0].Value.ToString() + " | " + channel_map[max_2021[0].Key] + " \n    " +
                "#2: " + max_2021[1].Value.ToString() + " | " + channel_map[max_2021[1].Key] + " \n    " +
                "#3: " + max_2021[2].Value.ToString() + " | " + channel_map[max_2021[2].Key] + " \n    " +
                "#4: " + max_2021[3].Value.ToString() + " | " + channel_map[max_2021[3].Key] + " \n    " +
                "#5: " + max_2021[4].Value.ToString() + " | " + channel_map[max_2021[4].Key] + " \n ");

            var max_2020 = MessagesPerChannel2020.OrderByDescending(d => d.Value).ToArray();
            Console.WriteLine("most popular channels in 2020: \n    " +
                "#1: " + max_2020[0].Value.ToString() + " | " + channel_map[max_2020[0].Key] + " \n    " +
                "#2: " + max_2020[1].Value.ToString() + " | " + channel_map[max_2020[1].Key] + " \n    " +
                "#3: " + max_2020[2].Value.ToString() + " | " + channel_map[max_2020[2].Key] + " \n    " +
                "#4: " + max_2020[3].Value.ToString() + " | " + channel_map[max_2020[3].Key] + " \n    " +
                "#5: " + max_2020[4].Value.ToString() + " | " + channel_map[max_2020[4].Key] + " \n ");


            // Finish graph
            if (do_graphing)
            {

                var sp = summaryPlot.Add.Scatter(xs, ys);
                sp.MarkerSize = 1;
                sp.LineWidth = 0;

                summaryPlot.Axes.Left.TickGenerator = verticalTicks;
                summaryPlot.Axes.Bottom.TickGenerator = yearTicks;
                summaryPlot.Axes.Bottom.TickLabelStyle.Rotation = -55;
                summaryPlot.Axes.Bottom.TickLabelStyle.Alignment = Alignment.MiddleRight;

                summaryPlot.YLabel("Time of day");
                summaryPlot.XLabel("Date");
                summaryPlot.ScaleFactor = 3;

                //summaryPlot.Layout.Fixed(new PixelPadding(128, 128, 128, 128));

                Console.WriteLine("Creating graph image...");
                string bruh_dir = dir + "\\SummaryScatter.png";
                summaryPlot.SavePng(bruh_dir, (Int32)yearsAmount * 750, 1000);//yearsAmount * 1000, 1000);

                Console.WriteLine("Opening image...");
                var p = new Process();
                p.StartInfo = new ProcessStartInfo(bruh_dir)
                {
                    UseShellExecute = true
                };
                p.Start();
                //Process.Start(bruh_dir);
            };

            Console.WriteLine("Finished, press enter to exit, scroll up to see summarized data");
            Console.ReadLine(); // Wait for user input, once program ends itll close probably
        }
    }
}
