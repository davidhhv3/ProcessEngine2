using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVM.ProcessEngine.Extension.EnergySuite.Helpers
{
    public class EnergySuiteHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="days"></param>
        /// <returns></returns>
        public static List<DayOfWeek> GetDayOfWeek(object[] days, SelectedTypeDay selectedTypeDay)
        {
            List<DayOfWeek> daysOfWeek = new List<DayOfWeek>();

            if (selectedTypeDay.Equals(SelectedTypeDay.Specific))
            {
                foreach (var day in days)
                {
                    daysOfWeek.AddRange(GetDayByName(day.ToString(), selectedTypeDay));

                }
            }
            // All
            else
            {
                daysOfWeek.AddRange(GetDayByName("", selectedTypeDay)); 

            }

            return daysOfWeek;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static List<DayOfWeek> GetDayByName(string name, SelectedTypeDay selectedTypeDay)
        {
            List<DayOfWeek> daysOfWeek = new List<DayOfWeek>();

            // All Days , Normals and Holiday
            if (selectedTypeDay.Equals(SelectedTypeDay.ALL_Normal_Holiday))
            {
                daysOfWeek.Add(DayOfWeek.Monday);
                daysOfWeek.Add(DayOfWeek.Tuesday);
                daysOfWeek.Add(DayOfWeek.Wednesday);
                daysOfWeek.Add(DayOfWeek.Thursday);
                daysOfWeek.Add(DayOfWeek.Friday);
                daysOfWeek.Add(DayOfWeek.Saturday);
                daysOfWeek.Add(DayOfWeek.Sunday);
                return daysOfWeek;
            }

            // All Days , Normals and No Holiday (No Sunday)
            if (selectedTypeDay.Equals(SelectedTypeDay.ALL_Normal))
            {
                daysOfWeek.Add(DayOfWeek.Monday);
                daysOfWeek.Add(DayOfWeek.Tuesday);
                daysOfWeek.Add(DayOfWeek.Wednesday);
                daysOfWeek.Add(DayOfWeek.Thursday);
                daysOfWeek.Add(DayOfWeek.Friday);
                daysOfWeek.Add(DayOfWeek.Saturday);
                return daysOfWeek;
            }

            // All Days , Holiday ,No Normals (Only Sunday)
            if (selectedTypeDay.Equals(SelectedTypeDay.ALL_Holiday))
            {
                daysOfWeek.Add(DayOfWeek.Sunday);
                return daysOfWeek;
            }

            // Days Specifics
            switch (name.ToLower())
            {
                case "monday":
                    daysOfWeek.Add(DayOfWeek.Monday); break;
                case "tuesday":
                    daysOfWeek.Add(DayOfWeek.Tuesday); break;
                case "wednesday":
                    daysOfWeek.Add(DayOfWeek.Wednesday); break;
                case "thursday":
                    daysOfWeek.Add(DayOfWeek.Thursday); break;
                case "friday":
                    daysOfWeek.Add(DayOfWeek.Friday); break;
                case "saturday":
                    daysOfWeek.Add(DayOfWeek.Saturday); break;
                case "sunday":
                    daysOfWeek.Add(DayOfWeek.Sunday); break;
                default:
                    throw new Exception();
            }

            return daysOfWeek;

        }

        public static List<int> GetHourOfRange(object[] ranges)
        {
            List<int> hours = new List<int>();

            foreach (var range in ranges)
            {
                string sRange = range.ToString();

                if (sRange.Contains("-"))
                {
                    int low = Int32.Parse(sRange.Split('-')[0]);
                    int max = Int32.Parse(sRange.Split('-')[1]);
                    for (int hour = low; hour <= max; hour++)
                    {
                        hours.Add(hour);
                    }
                }
                else
                {
                    hours.Add(Int32.Parse(sRange));
                }
            }

            return hours.Distinct().ToList();

        }

        public static List<int> GetHourOfLoadType(LoadType loadType)
        {
            List<int> hours = new List<int>();
            switch (loadType)
            {
                case LoadType.Low:
                    hours = GetHourOfRange(new string[] { "1-8" }); break;
                case LoadType.Medium:
                    hours = GetHourOfRange(new string[] { "9-16" }); break;
                case LoadType.High:
                    hours = GetHourOfRange(new string[] { "17-24" }); break;
                default:
                    throw new Exception();
            }

            return hours;

        }

        /// <summary>
        /// Get Selection Type Day
        /// </summary>
        /// <param name="allDay">Select all days , No = specific</param>
        /// <param name="normal">Select normal days</param>
        /// <param name="holiday">Select holiday days</param>
        /// <returns></returns>
        public static SelectedTypeDay GetSelectionTypeDay(bool allDay, bool normal, bool holiday)
        {
            if (allDay && normal && holiday) return SelectedTypeDay.ALL_Normal_Holiday;
            if (allDay && normal) return SelectedTypeDay.ALL_Normal;
            if (allDay && holiday) return SelectedTypeDay.ALL_Holiday;
            return SelectedTypeDay.Specific;
        }


    }
}
