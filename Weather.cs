//  Copyright 2005 University of Wisconsin
//  Authors:  Robert M. Scheller
//  License:  Available at  
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using System.Collections.Generic;
using System.IO;
using System;

namespace Landis.PestCalc
{

    public class Weather
    {

        private double[] monthlyTemp = new double[12];
        private double[] monthlyMinTemp = new double[12];
        private double[] monthlyPrecip = new double[12];

        private double latitude;
        private double longitude;
        private int beginGrowing;
        private int endGrowing;
        private int growingDegreeDays;
        private int chillingDays;
        //private int lateChillingDays;

        static Weather()
        {
        }

        public Weather(double latitude, double longitude)
        {
            this.latitude = latitude;
            this.longitude = longitude;
        }

        //---------------------------------------------------------------------
        public double[] MonthlyTemp
        {
            get {
                return monthlyTemp;
            }
        }
        //---------------------------------------------------------------------
        public double[] MonthlyMinTemp
        {
            get {
                return monthlyMinTemp;
            }
        }
        //---------------------------------------------------------------------
        public double[] MonthlyPrecip
        {
            get {
                return monthlyPrecip;
            }
        }
        //---------------------------------------------------------------------
        public int BeginGrowing
        {
            get {
                return beginGrowing;
            }
        }
        //---------------------------------------------------------------------
        public int EndGrowing
        {
            get {
                return endGrowing;
            }
        }
        //---------------------------------------------------------------------
        public int GrowingDegreeDays
        {
            get {
                return growingDegreeDays;
            }
        }
        //---------------------------------------------------------------------
        public int ChillingDays
        {
            get {
                return chillingDays;
            }
        }
        //---------------------------------------------------------------------
        public double Longitude
        {
            get {
                return longitude;
            }
        }
        //---------------------------------------------------------------------
        public double Latitude
        {
            get {
                return latitude;
            }
        }

        public void InitializeWeather(IMonthlyWeather[] annualClimate, int seed)//, int lateChillingDays)
        {
            NormalRandomVar randVar = new NormalRandomVar(0, 1);
            Random autoRand = new Random(seed * (int) DateTime.Now.Ticks);
            
            for (int i = 0; i < 12; i++)
            {
                double MonthlyAvgTemp = (annualClimate[i].AvgMinTemp + annualClimate[i].AvgMaxTemp) / 2.0;
                //Console.WriteLine("TempRandNum = {0}.", randVar.GenerateNumber(autoRand));
                this.monthlyTemp[i] = MonthlyAvgTemp + (annualClimate[i].StdDevTemp * (randVar.GenerateNumber(autoRand)));
                this.monthlyMinTemp[i] = annualClimate[i].AvgMinTemp + (annualClimate[i].StdDevTemp * (randVar.GenerateNumber(autoRand)));
                //Console.WriteLine("Month = {0}.  Mean T = {1:0.0}.", i, this.monthlyTemp[i]);
            }
            for (int i = 0; i < 12; i++)
            {
                this.monthlyPrecip[i] = annualClimate[i].AvgPpt + (annualClimate[i].StdDevPpt * (randVar.GenerateNumber(autoRand)));
                if (this.monthlyPrecip[i] < 0) this.monthlyPrecip[i] = 0;
                //Console.WriteLine("Month = {0}.  Mean Ppt = {1:0.0}.", i, this.monthlyPrecip[i]);
            }
            this.beginGrowing = CalculateBeginGrowingSeason(annualClimate, autoRand);
            this.endGrowing = CalculateEndGrowingSeason(annualClimate, autoRand);
            this.growingDegreeDays = GrowSeasonDegreeDays(1968);
            this.chillingDays = CalculateChillingDays(annualClimate, autoRand);
        }

        
        public double MinJanuaryTempModifier(int speciesMinimum)
        // Is the January mean temperature greater than the species specified minimum?
        {
            if (MonthlyMinTemp[0] < speciesMinimum)
                return 0.0;
            else
                return 1.0;
        }

        public double BotkinDegreeDayMultiplier(double max_Grow_Deg_Days, double min_Grow_Deg_Days)
        {

            //Calc species degree day multipliers  
            //Botkin et al. 1972. J. Ecol. 60:849 - 87
            
            double Deg_Day_GF = 0.0;
            double Deg_Days = (double) this.GrowingDegreeDays; 
            double totalGDD = max_Grow_Deg_Days - min_Grow_Deg_Days;
            
            Deg_Day_GF = (4.0 * (Deg_Days - min_Grow_Deg_Days) * 
                  (max_Grow_Deg_Days - Deg_Days)) / (totalGDD * totalGDD);
            
           if (Deg_Day_GF < 0) Deg_Day_GF = 0.0;     
           
           return Deg_Day_GF;
        }

        public double SykesMinimumGDDMultiplier(ISpeciesData sppData)
        {
            int b = sppData.B;
            double k = sppData.K;
            double minGDDstar = sppData.MinGDDstar;
            
            //double GDDnot = b * Math.Exp(-1 * k * this.ChillingDays);
            double GDDnot = b * Math.Exp(1) - (k * this.ChillingDays);
            
            double GDDstar = this.growingDegreeDays - GDDnot;
            
            Console.WriteLine("GDD={0}, GDDnot={1:0.0}, GDDstar={2:0.0}.", this.growingDegreeDays, GDDnot, GDDstar);
            
            if(GDDstar < minGDDstar)
                return 0.0;
                
            return 1.0;
        }

        public int GrowSeasonDegreeDays(int currentYear)
        //Calc growing season degree days (Degree_Day) based on monthly temperatures
        //normally distributed around a specified mean with a specified standard
        //deviation.

        {
            //degDayBase is temperature (C) above which degree days (Degree_Day)
            //are counted
            double degDayBase = 5.00;      // 42F.

            double Deg_Days = 0.0;

            //Calc monthly temperatures (mean +/- normally distributed
            //random number times standard deviation) and 
            //sum degree days for consecutve months.
            for (int i = 0; i < 12; i++) //12 months in year
            {    
                if (MonthlyTemp[i] > degDayBase)
                    Deg_Days += (MonthlyTemp[i] - degDayBase) * Weather.DaysInMonth(i, currentYear);
            }
            return (int) Deg_Days;
        }

        public int CalculateChillingDays(IMonthlyWeather[] annualClimate, Random autoRand)
        // Chilling days is the number of days since November 1 with mean temperatures <= 5 C.  
        // Murray et al. 1996. J Applied Ecology 26: 693-700.

        {
            NormalRandomVar randVar = new NormalRandomVar(0, 1);
            
            double chillDayBase = 5.00;      

            // Because we only have a single year's weather, it is necessary to use current year
            // November and December as a substitute for the same months in the previous year.  This should generally
            // be a safe assumption as these months should be almost entirely chilling days.
            double lastMonthAvgTemp = (annualClimate[10].AvgMinTemp + annualClimate[10].AvgMaxTemp) / 2.0;
            
            int chillDays = 0;
            //int currentYear = 0;
            int dayCnt = 0; 

            int[] months = new int[8]{10, 11, 0, 1, 2, 3, 4, 5};

            //Only look at 8 months assuming thermal time to budburst must occur before July 1.
            for (int i = 0; i < 8; i++) 
           
            {
            
                int month = months[i];
            
                // First look backwards at each month mid-point.  November looks 
                // back to October.
                if (month == 10)   
                    dayCnt = (DaysInMonth(month, 3) + DaysInMonth(9, 3)) / 2;
                else
                    dayCnt = (DaysInMonth(month, 3) + DaysInMonth(month-1, 3)) / 2;  

                double MonthlyAvgTemp = (annualClimate[month].AvgMinTemp + annualClimate[month].AvgMaxTemp) / 2.0; 

                //Now interpolate between days:
                double degreeIncrement = (double) (MonthlyAvgTemp - lastMonthAvgTemp) / (double) dayCnt;
                double TDay = MonthlyAvgTemp;  //start from warmer month
                double TDayRandom = TDay + (annualClimate[month].StdDevTemp * (randVar.GenerateNumber(autoRand) * 2 - 1));
                   
                for(int day = 1; day <= dayCnt; day++)
                {
                    if(month != 10 || dayCnt > (DaysInMonth(9, 3) / 2))  // Account for starting at November 1.
                        if(TDayRandom < chillDayBase)
                            chillDays++;
                    TDay += degreeIncrement;  //work backwards to find last frost day.
                    TDayRandom = TDay + (annualClimate[month].StdDevTemp * (randVar.GenerateNumber(autoRand)* 2 - 1));
                }

                lastMonthAvgTemp = MonthlyAvgTemp;
            }

            Console.WriteLine("#Chill Days={0}.", chillDays);
            return chillDays;


        }

        public double MeanAnnualTemp(int currentYear)
        {
            double MAT = 0.0;
            //Calc monthly temperatures (mean +/- normally distributed
            //random number times  standard deviation) and 
            //sum degree days for consecutve months.
            for (int i = 0; i < 12; i++) //12 months in year
            {    
                int daysInMonth = Weather.DaysInMonth(i, currentYear);
                MAT += daysInMonth * MonthlyTemp[i];
            }
            MAT /= 365.0;
            return MAT;
        }

        public double TotalAnnualPrecip()
        {
            //Main loop for yearly water balance calculation by month   */
            double TAP = 0.0;
            for (int i = 0; i < 12; i++) 
            {
                TAP += MonthlyPrecip[i];
            }
            return TAP;
        }

        
        private static int CalculateBeginGrowingSeason(IMonthlyWeather[] annualClimate, Random autoRand)
        //Calculate Begin Growing Degree Day (Last Frost; Minimum = 0 degrees C):
        {
            NormalRandomVar randVar = new NormalRandomVar(0, 1);
            double lastMonthMinTemp = annualClimate[0].AvgMinTemp;
            int dayCnt = 15;  //the middle of February
            int beginGrowingSeason = -1;
            
            for (int i = 1; i < 7; i++)  //Begin looking in February (1).  Should be safe for at least 100 years.
            {
                
                int totalDays = (DaysInMonth(i, 3) + DaysInMonth(i-1, 3)) / 2;
                double MonthlyMinTemp = annualClimate[i].AvgMinTemp;// + (monthlyTempSD[i] * randVar.GenerateNumber());

                //Now interpolate between days:
                double degreeIncrement = System.Math.Abs(MonthlyMinTemp - lastMonthMinTemp) / (double) totalDays;
                double Tnight = MonthlyMinTemp;  //start from warmer month
                double TnightRandom = Tnight + (annualClimate[i].StdDevTemp * (randVar.GenerateNumber(autoRand)* 2 - 1));
                   
                for(int day = 1; day <= totalDays; day++)
                {
                    if(TnightRandom <= 0)
                        beginGrowingSeason = (dayCnt + day);
                    Tnight += degreeIncrement;  //work backwards to find last frost day.
                    TnightRandom = Tnight + (annualClimate[i].StdDevTemp * (randVar.GenerateNumber(autoRand)* 2 - 1));
                }

                lastMonthMinTemp = MonthlyMinTemp;
                dayCnt += totalDays;  //new monthly mid-point
            }
            return beginGrowingSeason;
        }

        private static int CalculateEndGrowingSeason(IMonthlyWeather[] annualClimate, Random autoRand)
        //Calculate End Growing Degree Day (First frost; Minimum = 0 degrees C):
        {
            NormalRandomVar randVar = new NormalRandomVar(0, 1);
            
            //Defaults for the middle of July:
            double lastMonthTemp = annualClimate[6].AvgMinTemp;
            int dayCnt = 198;  
            //int endGrowingSeason = 198;
            
            for (int i = 7; i < 12; i++)  //Begin looking in August.  Should be safe for at least 100 years.
            {
                int totalDays = (DaysInMonth(i, 3) + DaysInMonth(i-1, 3)) / 2;
                double MonthlyMinTemp = annualClimate[i].AvgMinTemp;

                //Now interpolate between days:
                double degreeIncrement = System.Math.Abs(lastMonthTemp - MonthlyMinTemp) / (double) totalDays;
                double Tnight = lastMonthTemp;  //start from warmer month

                    //double randomT = (2 * annualClimate[i].StdDevTemp * randVar.GenerateNumber(autoRand));
                    //Console.WriteLine("Night Temp random offset = {0}.", randomT);
                double TnightRandom = Tnight + (annualClimate[i].StdDevTemp * (randVar.GenerateNumber(autoRand)* 2 - 1));
                   
                for(int day = 1; day <= totalDays; day++)
                {
                    if(TnightRandom <= 0)
                        return (dayCnt + day);
                    Tnight -= degreeIncrement;  //work forwards to find first frost day.
                    TnightRandom = Tnight + (annualClimate[i].StdDevTemp * (randVar.GenerateNumber(autoRand)* 2 - 1));
                    //Console.WriteLine("Tnight = {0}.", TnightRandom);
                }

                lastMonthTemp = MonthlyMinTemp;
                dayCnt += totalDays;  //new monthly mid-point
            }
            return -1;
        }
        
        public static int DaysInMonth(int month, int currentYear)
        //This will return the number of days in a month given the month number where
        //January is 1.

        {
            switch(month+1) 
            {
                //Thirty days hath September, April, June && November
                case 9:  case 4:  case 6: case 11: return 30;
                //...all the rest have 31...
                case 1: case 3: case 5: case 7:
                case 8: case 10: case 12: return 31;
                //...save February, etc.
                case 2: if (currentYear%4 == 0)
                  return 28;
                else
                  return 29;
            }
            return 0;   
        }
        
        public static double LatitudeCorrection(int month, double latitude)
        {
            double latitudeCorrection = 0;
            int  latIndex = 0;
            double[,] latCorrect = new double[27, 13] 
                {
                    {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                    {0, .93, .89, 1.03, 1.06, 1.15, 1.14, 1.17, 1.12, 1.02, .99, .91, .91},
                    {0, .92, .88, 1.03, 1.06, 1.15, 1.15, 1.17, 1.12, 1.02, .99, .91, .91},
                    {0, .92, .88, 1.03, 1.07, 1.16, 1.15, 1.18, 1.13, 1.02, .99, .90, .90},
                    {0, .91, .88, 1.03, 1.07, 1.16, 1.16, 1.18, 1.13, 1.02, .98, .90, .90},
                    {0, .91, .87, 1.03, 1.07, 1.17, 1.16, 1.19, 1.13, 1.03, .98, .90, .89},
                    {0, .90, .87, 1.03, 1.08, 1.18, 1.17, 1.20, 1.14, 1.03, .98, .89, .88},
                    {0, .90, .87, 1.03, 1.08, 1.18, 1.18, 1.20, 1.14, 1.03, .98, .89, .88},
                    {0, .89, .86, 1.03, 1.08, 1.19, 1.19, 1.21, 1.15, 1.03, .98, .88, .87},
                    {0, .88, .86, 1.03, 1.09, 1.19, 1.20, 1.22, 1.15, 1.03, .97, .88, .86},
                    {0, .88, .85, 1.03, 1.09, 1.20, 1.20, 1.22, 1.16, 1.03, .97, .87, .86},
                    {0, .87, .85, 1.03, 1.09, 1.21, 1.21, 1.23, 1.16, 1.03, .97, .86, .85},
                    {0, .87, .85, 1.03, 1.10, 1.21, 1.22, 1.24, 1.16, 1.03, .97, .86, .84},
                    {0, .86, .84, 1.03, 1.10, 1.22, 1.23, 1.25, 1.17, 1.03, .97, .85, .83},
                    {0, .85, .84, 1.03, 1.10, 1.23, 1.24, 1.25, 1.17, 1.04, .96, .84, .83},
                    {0, .85, .84, 1.03, 1.11, 1.23, 1.24, 1.26, 1.18, 1.04, .96, .84, .82},
                    {0, .84, .83, 1.03, 1.11, 1.24, 1.25, 1.27, 1.18, 1.04, .96, .83, .81},
                    {0, .83, .83, 1.03, 1.11, 1.25, 1.26, 1.27, 1.19, 1.04, .96, .82, .80},
                    {0, .82, .83, 1.03, 1.12, 1.26, 1.27, 1.28, 1.19, 1.04, .95, .82, .79},
                    {0, .81, .82, 1.02, 1.12, 1.26, 1.28, 1.29, 1.20, 1.04, .95, .81, .77},
                    {0, .81, .82, 1.02, 1.13, 1.27, 1.29, 1.30, 1.20, 1.04, .95, .80, .76},
                    {0, .80, .81, 1.02, 1.13, 1.28, 1.29, 1.31, 1.21, 1.04, .94, .79, .75},
                    {0, .79, .81, 1.02, 1.13, 1.29, 1.31, 1.32, 1.22, 1.04, .94, .79, .74},
                    {0, .77, .80, 1.02, 1.14, 1.30, 1.32, 1.32, 1.22, 1.04, .93, .78, .73},
                    {0, .76, .80, 1.02, 1.14, 1.31, 1.33, 1.34, 1.23, 1.05, .93, .77, .72},
                    {0, .75, .79, 1.02, 1.14, 1.32, 1.34, 1.35, 1.24, 1.05, .93, .76, .71},
                    {0, .74, .78, 1.02, 1.15, 1.33, 1.36, 1.37, 1.25, 1.06, .92, .76, .70}};
                    
            latIndex = (int) (latitude + 0.5) - 24;
            if (latIndex < 1) 
                throw new System.ApplicationException("Error: Lat index too small.");
            if (latIndex > 26)
                latIndex = 26;

            latitudeCorrection = latCorrect[latIndex, month];
            return latitudeCorrection;
        }


        public string Write()
        {
            string s = String.Format(            
                " Weather:  Number GDD = {0}."
                + " Chilling Period = {5}."
                + " Begin Grow Season = {1}." 
                + " End Grow Season = {2}." 
                + " TotalAnnualPpt = {3:000.0}." 
                + " MeanJanuaryLowTemp = {4:0.0}.",
                this.GrowingDegreeDays, 
                    this.BeginGrowing, 
                    this.EndGrowing, 
                    TotalAnnualPrecip(), 
                    this.monthlyMinTemp[0], 
                    this.chillingDays
                    );
            return s;
        }
    }
}
