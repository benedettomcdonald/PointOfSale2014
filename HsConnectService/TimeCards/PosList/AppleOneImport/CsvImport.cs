using System;
using System.IO;
using System.Collections;
using HsSharedObjects.Client;
using HsSharedObjects.Client.Preferences;

namespace HsConnect.TimeCards.PosList.AppleOneImport
{
    public class CsvImport
    {
        public CsvImport(String filePath, ClientDetails details)
        {
            _filePath = filePath;
            _timecardsByWeek = new Hashtable();
            _details = details;
            _weekStart = _details.WeekStartDay;
        }

        private String _filePath = "";
        private String _weekStart = "";
        private Hashtable _timecardsByWeek;
        private ClientDetails _details;
        System.IFormatProvider frmt = new System.Globalization.CultureInfo("en-US", true);

        public ICollection GetWeekCards()
        {
            return _timecardsByWeek.Values;
        }

        public void Init()
        {
            DateTime startDate = DateTime.Today;

            while (startDate.DayOfWeek.ToString().CompareTo(_weekStart) != 0)
            {
                startDate = startDate.AddDays(-1);
            }
            startDate = startDate.AddDays(-14);

            StreamReader text = null;

            try
            {
                text = File.OpenText(_filePath);

                while (text.Peek() > 0)
                {
                    String inStr = text.ReadLine();
                    String[] strArray = LineToArray(inStr);

                    try
                    {
                        DateTime df = DateTime.ParseExact(strArray[1], "yyyyMMdd", frmt);

                        if (DateTime.Compare(df, startDate) >= 0)
                        {
                            AppleOneTimeCard card = new AppleOneTimeCard();
                            card.EmployeeId = Convert.ToInt32(strArray[0]);

                            int rootCode = (int)Convert.ToDouble(strArray[7]);

                            if (_details.Preferences.PrefExists(Preference.APPLE_DOS_EMP_JOBS_TRAILING_ZERO)) //add pref here for extra 0's on the emp jobs
                            {
                                rootCode = rootCode / 10;
                            }

                            card.JobId = Convert.ToInt32(rootCode);
                            card.PayRate = (float)Convert.ToDouble(strArray[8]);

                            DateTime inDate = DateTime.ParseExact(strArray[1], "yyyyMMdd", frmt);
                            DateTime inTime = DateTime.ParseExact(strArray[3], "H.mm", frmt);
                            card.ClockIn = new DateTime(inDate.Year, inDate.Month, inDate.Day, inTime.Hour, inTime.Minute, 0);

                            DateTime outDate = DateTime.ParseExact(strArray[4], "yyyyMMdd", frmt);
                            DateTime outTime = DateTime.ParseExact(strArray[5], "H.mm", frmt);
                            card.ClockOut = new DateTime(outDate.Year, outDate.Month, outDate.Day, outTime.Hour, outTime.Minute, 0);

                            ArrayList weekCards;
                            if (_timecardsByWeek.ContainsKey(getWeekStart(inDate).ToShortDateString()))
                            {
                                weekCards = (ArrayList)_timecardsByWeek[getWeekStart(inDate).ToShortDateString()];
                                weekCards.Add(card);
                            }
                            else
                            {
                                weekCards = new ArrayList();
                                weekCards.Add(card);
                                _timecardsByWeek.Add(getWeekStart(inDate).ToShortDateString(), weekCards);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }

                }
            }
            finally
            {
                text.Close();
            }
        }


        public void MakeTimeCards(TimeCardList timeCards, ArrayList appleCards)
        {
            OTRules rules = null;
            switch (timeCards.Details.OvertimeRule)
            {
                case 0:
                    rules = new StandardRules();
                    break;
                case 1: // CA
                case 4:
                    rules = new CARules();
                    break;
                case 2: // NV
                    rules = new StandardRules();
                    break;
            }
            rules.TransferCards(timeCards, appleCards);
        }

        private DateTime getWeekStart(DateTime date)
        {
            DateTime startDate = new DateTime(date.Date.Ticks);
            while (startDate.DayOfWeek.ToString().CompareTo(_weekStart) != 0)
            {
                startDate = startDate.AddDays(-1);
            }
            return startDate;
        }

        private String[] LineToArray(String line)
        {
            int startIndex = 0;
            int length = 0;
            ArrayList strings = new ArrayList();
            while (startIndex < line.Length)
            {
                if (line.Substring(startIndex, 1).CompareTo("\"") == 0)
                {
                    length = line.IndexOf("\",", startIndex + 1) - (startIndex + 1);
                    length = length < 0 ? (line.Length - 1) - (startIndex + 1) : length;
                    String s = line.Substring(startIndex + 1, length);
                    strings.Add(s);
                    startIndex += (length + 3);
                }
                else
                {
                    length = line.IndexOf(",", startIndex + 1) - startIndex;
                    length = length < 0 ? line.Length - startIndex : length;
                    String s = line.Substring(startIndex, length);
                    strings.Add(s);
                    startIndex += (length + 1);
                }
            }
            String[] strs = new String[strings.Count];
            int cnt = 0;
            foreach (String str in strings)
            {
                strs[cnt] = str.Trim();
                cnt++;
            }
            return strs;
        }
    }
}
