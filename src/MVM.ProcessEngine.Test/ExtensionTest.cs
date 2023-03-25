using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MVM.ProcessEngine.Extension.EnergySuite.Helpers;
using MVM.ProcessEngine.Extension.EnergySuite;
using System.Collections.Generic;
using MVM.ProcessEngine.Extension.EnergySuite.Repositories;

namespace MVM.ProcessEngine.Test
{
    [TestClass]
    public class ExtensionTest
    {
        /// <summary>
        /// Selection Type Test
        /// </summary>
        [TestMethod]
        public void GetSelectionTypeDayTest()
        {
            //All , Normal, Holyday
            SelectedTypeDay selectionTypeDay;

            selectionTypeDay = EnergySuiteHelper.GetSelectionTypeDay(true, true, true);
            Assert.AreEqual(selectionTypeDay, SelectedTypeDay.ALL_Normal_Holiday);

            selectionTypeDay = EnergySuiteHelper.GetSelectionTypeDay(true, true, false);
            Assert.AreEqual(selectionTypeDay, SelectedTypeDay.ALL_Normal);

            selectionTypeDay = EnergySuiteHelper.GetSelectionTypeDay(true, false, true);
            Assert.AreEqual(selectionTypeDay, SelectedTypeDay.ALL_Holiday);

            selectionTypeDay = EnergySuiteHelper.GetSelectionTypeDay(false, true, true);
            Assert.AreEqual(selectionTypeDay, SelectedTypeDay.Specific);

            selectionTypeDay = EnergySuiteHelper.GetSelectionTypeDay(false, true, false);
            Assert.AreEqual(selectionTypeDay, SelectedTypeDay.Specific);

            selectionTypeDay = EnergySuiteHelper.GetSelectionTypeDay(false, false, true);
            Assert.AreEqual(selectionTypeDay, SelectedTypeDay.Specific);

            selectionTypeDay = EnergySuiteHelper.GetSelectionTypeDay(false, false, false);
            Assert.AreEqual(selectionTypeDay, SelectedTypeDay.Specific);

        }

        /// <summary>
        /// Day of Week Test
        /// </summary>
        [TestMethod]
        public void GetDayOfWeekTest()
        {
            // All Days 
            object[] days = new object[0];
            List<DayOfWeek> dayOfWeek;

            dayOfWeek = GetDays(days, SelectedTypeDay.ALL_Normal_Holiday);
            Assert.AreEqual(dayOfWeek.Count, 7);

            dayOfWeek = GetDays(days, SelectedTypeDay.ALL_Normal);
            Assert.AreEqual(dayOfWeek.Count, 6);

            dayOfWeek = GetDays(days, SelectedTypeDay.ALL_Holiday);
            Assert.AreEqual(dayOfWeek.Count, 1);

            // Only Monday
            days = new object[1] { "monday" };
            dayOfWeek = GetDays(days, SelectedTypeDay.Specific);
            Assert.AreEqual(dayOfWeek.Count, 1);

            days = new object[2] { "monday", "tuesday" };
            dayOfWeek = GetDays(days, SelectedTypeDay.Specific);
            Assert.AreEqual(dayOfWeek.Count, 2);

            // Only Monday
            days = new object[1] { "sunday" };
            dayOfWeek = GetDays(days, SelectedTypeDay.Specific);
            Assert.AreEqual(dayOfWeek.Count, 1);


        }

        [TestMethod]
        public void GetDatesHoursByCondition()
        {
            ContractConditionsExternalFunction ccef = new ContractConditionsExternalFunction();
            ccef.Holidays.Add(new DateTime(2017, 3, 6));
            //ccef.Holidays= new SQLRepository().GetHoliDays(new DateTime(2017, 3, 1), new DateTime(2017, 3, 31));
            ccef.StartDateOfExecution = new DateTime(2017, 3, 1);
            ccef.EndDateOfExecution = new DateTime(2017, 3, 31);

            FormulaCondition formula = new FormulaCondition();
            formula.StartDate = new DateTime(2017, 3, 1);
            formula.EndDate = new DateTime(2017, 3, 31);
            formula.Days = GetDays(new object[0], SelectedTypeDay.ALL_Normal_Holiday); ;
            //formula.Hours = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 };
            formula.Hours = new List<int> { 1 };
            formula.AllDays = true;
            formula.NormalDay = true;
            formula.Holiday = true;

            // ALL
            var results = GetCCEFDateHours(ccef, formula);
            Assert.AreEqual(results.Count, 31 * formula.Hours.Count);

            // ALL No Holiday
            formula.Days = GetDays(new object[0], SelectedTypeDay.ALL_Normal);
            formula.AllDays = true;
            formula.NormalDay = true;
            formula.Holiday = false;
            results = GetCCEFDateHours(ccef, formula);
            Assert.AreEqual(results.Count, 26 * formula.Hours.Count);

            // ALL Only Holiday and Sundays
            formula.Days = GetDays(new object[0], SelectedTypeDay.ALL_Holiday);
            formula.AllDays = true;
            formula.NormalDay = false;
            formula.Holiday = true;
            results = GetCCEFDateHours(ccef, formula);
            Assert.AreEqual(results.Count, 5 * formula.Hours.Count);

            // Spacific Normal and Holiday 
            formula.Days = GetDays(new object[2] { "monday", "tuesday" }, SelectedTypeDay.Specific);
            formula.AllDays = false;
            formula.NormalDay = true;
            formula.Holiday = true;
            results = GetCCEFDateHours(ccef, formula);
            Assert.AreEqual(results.Count, 8 * formula.Hours.Count);

            // Spacific Only Normal 
            formula.Days = GetDays(new object[2] { "monday", "tuesday" }, SelectedTypeDay.Specific);
            formula.AllDays = false;
            formula.NormalDay = true;
            formula.Holiday = false;
            results = GetCCEFDateHours(ccef, formula);
            Assert.AreEqual(results.Count, 7 * formula.Hours.Count);

            // Spacific Only Holiday 
            formula.Days = GetDays(new object[2] { "monday", "tuesday" }, SelectedTypeDay.Specific); 
            formula.AllDays = false;
            formula.NormalDay = false;
            formula.Holiday = true;
            results = GetCCEFDateHours(ccef, formula);
            Assert.AreEqual(results.Count, 1 * formula.Hours.Count);
        }

        private static List<DateHour> GetCCEFDateHours(ContractConditionsExternalFunction ccef, FormulaCondition formula)
        {
            List<DateHour> results;


            results = ccef.GetDatesHoursByCondition(formula);
            Console.WriteLine();

            foreach (var res in results)
            {
                Console.Write(res.Date.Day + "(" + res.Hour + ") ");
            }

            return results;
        }

        private static List<DayOfWeek> GetDays(object[] days, SelectedTypeDay selectedTypeDay)
        {
            List<DayOfWeek> dayOfWeek;
            Console.WriteLine();
            Console.WriteLine(selectedTypeDay.ToString());

            dayOfWeek = EnergySuiteHelper.GetDayOfWeek(days, selectedTypeDay);

            foreach (var day in dayOfWeek)
            {
                Console.WriteLine(day.ToString());
            }

            return dayOfWeek;
        }


    }
}
