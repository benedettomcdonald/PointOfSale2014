using System;
using System.Collections;
using pslbr;

namespace HsConnect.TimeCards.PosList.AppleOneImport
{
    public class CARules : OTRules
    {
        private HsConnect.Main.SysLog logger = new HsConnect.Main.SysLog(typeof(CARules));
        private float MINIMUM_WAGE = 6.75f;


        public void TransferCards(TimeCardList timeCards, ArrayList appleCards)
        {
            logger.Debug("CA Overtime Rules");
            foreach (AppleOneTimeCard card in appleCards)
            {
                try
                {
                    DateTime lastDay = new DateTime(0);
                    int consecDays = 0;
                    DateTime lastOutTime = new DateTime(0); // Used for split shift penalty
                    Hashtable empDayHours = new Hashtable();
                    float empWeekHours = 0.0f;

                    logger.Debug("CA Ovt: Debug 1");

                    TimeCard tCard = new TimeCard();
                    tCard.EmpPosId = card.EmployeeId;
                    tCard.JobExtId = card.JobId;
                    tCard.ClockIn = card.ClockIn;
                    tCard.ClockOut = card.ClockOut;
                    tCard.BusinessDate = card.ClockIn;

                    logger.Debug("CA Ovt: Debug 2");

                    bool otCalculated = false;
                    //                EmployeeJob empJob = punch.Job;
                    DateTime inDate = card.ClockIn;
                    DateTime outDate = card.ClockOut;
                    if (lastDay.Date != inDate.Date)
                    {
                        logger.Debug("CA Ovt: Comparing Dates");
                        if ((inDate.Day - lastDay.Day) == 1) consecDays++;
                        lastDay = inDate;
                        lastOutTime = new DateTime(0);
                    }

                    //                if (empJob == null)
                    //                {
                    //                    Console.WriteLine("Employee job was null");
                    //                    continue;
                    //                }

                    //                tCard.AltCode = punch.Job.JobCode;
                    //                tCard.JobDept = punch.Job.JobDept;
                    tCard.ClockIn = card.ClockIn;
                    tCard.ClockOut = card.ClockOut;

                    logger.Debug("CA Ovt: Duration");
                    TimeSpan duration = outDate - inDate;
                    float durationFloat = (duration.Hours + (duration.Minutes / 60.0f));
                    float regHours = durationFloat;
                    float regRate = consecDays < 6 ? card.PayRate : card.PayRate * 1.5f;
                    float otRate = consecDays < 6 ? card.PayRate * 1.5f : card.PayRate * 2.0f;
                    float dblRate = card.PayRate * 2.0f;

                    logger.Debug("CA Ovt: Debug 3");

                    String hrs = empDayHours[inDate.Date.ToString()] != null ? (String)empDayHours[inDate.Date.ToString()] : "0";
                    float dayHours = (float)Convert.ToDecimal(hrs);

                    logger.Debug("CA Ovt: Checking dayHours");
                    if (dayHours >= 12)
                    {
                        logger.Debug("CA Ovt: >12 hrs");
                        tCard.OvtHours = durationFloat;
                        tCard.OvtTotal = durationFloat * dblRate;
                        regHours = 0.0f;
                        otCalculated = true;
                    }
                    else if (dayHours < 12 && durationFloat + dayHours > 12)
                    {
                        logger.Debug("CA Ovt: total > 12 hrs");
                        tCard.RegHours = (dayHours <= 8) ? 8.0f - dayHours : 0.0f;
                        tCard.RegTotal = tCard.RegHours * regRate;
                        tCard.OvtHours = durationFloat - tCard.RegHours;
                        tCard.OvtTotal = (4.0f * otRate) + (((dayHours + durationFloat) - 12) * dblRate);
                        regHours = tCard.RegHours;
                        otCalculated = true;
                    }
                    else if (dayHours >= 8)
                    {
                        logger.Debug("CA Ovt: >= 8 hrs");
                        tCard.OvtHours = durationFloat;
                        tCard.OvtTotal = durationFloat * otRate;
                        regHours = 0.0f;
                        otCalculated = true;
                    }
                    else if (dayHours < 8 && durationFloat + dayHours > 8)
                    {
                        logger.Debug("CA Ovt: total > 8 hrs");
                        tCard.RegHours = 8.0f - dayHours;
                        tCard.RegTotal = tCard.RegHours * regRate;
                        tCard.OvtHours = durationFloat - tCard.RegHours;
                        tCard.OvtTotal = tCard.OvtHours * otRate;
                        regHours = tCard.RegHours;
                        otCalculated = true;
                    }

                    // add meal break
                    //                bool hasWaiver = empHasWaiver(this.empWeek.EmployeeId);
                    int maxDuration = 5;
                    logger.Debug("CA Ovt: Max Duration: " + maxDuration);
                    if (durationFloat > maxDuration)
                    {
                        logger.Debug("CA Ovt: Max Duration Check");
                        tCard.SpcTotal += card.PayRate;
                    }

                    // add meal break if less than 30 minutes was taken
                    else if (dayHours > 0 && (inDate - lastOutTime) < new TimeSpan(0, 30, 0) && (durationFloat + dayHours >= maxDuration))
                    {
                        logger.Debug("CA Ovt: Meal Break");
                        tCard.SpcTotal += card.PayRate;
                    }

                    // add split shift penalty
                    if (dayHours > 0 && (inDate - lastOutTime) > new TimeSpan(1, 0, 0))
                    {
                        logger.Debug("CA Ovt: Split Shift Penalty");
                        float premium = MINIMUM_WAGE - ((card.PayRate - MINIMUM_WAGE) * dayHours);
                        tCard.SpcTotal += (premium > 0 ? premium : 0);
                    }

                    if (empDayHours.ContainsKey(inDate.Date.ToString())) 
                        empDayHours.Remove(inDate.Date.ToString());
                    dayHours += durationFloat;
                    empDayHours.Add(inDate.Date.ToString(), dayHours.ToString());
                    lastOutTime = tCard.ClockOut;

                    logger.Debug("CA Ovt: Debug 4");

                    otRate = card.PayRate * 1.5f;// reset the otRate to 1.5, because > 8 hour day has been accounted for
                    if (!otCalculated && empWeekHours >= 40)
                    {
                        logger.Debug("CA Ovt: empWeekHours >= 40");
                        tCard.OvtHours = durationFloat;
                        tCard.OvtTotal = durationFloat * otRate;
                    }
                    else if (!otCalculated && empWeekHours < 40 && durationFloat + empWeekHours > 40)
                    {
                        // this shifts put emp into overtime
                        logger.Debug("CA Ovt: total > 40 hrs");
                        tCard.RegHours = 40.0f - empWeekHours;
                        tCard.RegTotal = tCard.RegHours * regRate;
                        tCard.OvtHours = durationFloat - tCard.RegHours;
                        tCard.OvtTotal = tCard.OvtHours * otRate;
                    }
                    else if (!otCalculated && empWeekHours < 40 && durationFloat + empWeekHours <= 40)
                    {
                        // no overtime
                        logger.Debug("CA Ovt: total <= 40 hrs");
                        tCard.RegHours = durationFloat;
                        tCard.RegTotal = durationFloat * regRate;
                    }
                    empWeekHours += regHours;
                    //Console.WriteLine( "\tReg Hours = {0}, Reg Dollars = {1}, Ot Hours = {2}, Ot Dollars = {3}\n", tCard.RegHours, tCard.RegDollars, tCard.OtHours, tCard.OtDollars );

                    tCard.RegWage = card.PayRate;
                    tCard.OvtWage = tCard.OvtHours == 0 ? 0 : (tCard.OvtTotal / tCard.OvtHours);
                    timeCards.Add(tCard);
                    logger.Debug("CA Ovt: Added Ovt Card");
                }
                catch (Exception ex)
                {
                    logger.Error("CA Ovt: Unable to calculate ovt");
                    logger.Error(ex.StackTrace);
                }
            }
        }
    }
}