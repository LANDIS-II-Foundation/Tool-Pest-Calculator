//  Copyright 2006 University of Wisconsin
//  Authors:  Robert M. Scheller
//  License:  Available at  
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using System.Collections.Generic;

namespace Landis.PestCalc
{
    /// <summary>
    /// Weather parameters for each month.
    /// </summary>
    public interface IMonthlyWeather
    {

        double AvgMinTemp{get;set;}
        double AvgMaxTemp{get;set;}
        double StdDevTemp{get;set;}
        double AvgPpt{get;set;}
        double StdDevPpt{get;set;}

    }
}



namespace Landis.PestCalc
{

    public class MonthlyWeather
    : IMonthlyWeather
    {

        private double avgMinTemp;
        private double avgMaxTemp;
        private double stdDevTemp;
        private double avgPpt;
        private double stdDevPpt;

        public double AvgMinTemp
        {
            get {
                return avgMinTemp;
            }
            set {
                avgMinTemp = value;
            }
        }

        public double AvgMaxTemp
        {
            get {
                return avgMaxTemp;
            }
            set {
                avgMaxTemp = value;
            }
        }
        public double StdDevTemp
        {
            get {
                return stdDevTemp;
            }
            set {
                stdDevTemp = value;
            }
        }
        public double AvgPpt
        {
            get {
                return avgPpt;
            }
            set {
                avgPpt = value;
            }
        }
        public double StdDevPpt
        {
            get {
                return stdDevPpt;
            }
            set {
                stdDevPpt = value;
            }
        }

        public MonthlyWeather(
                            double avgMinTemp,
                            double avgMaxTemp,
                            double stdDevTemp,
                            double avgPpt,
                            double stdDevPpt
                            )
        {
            this.avgMinTemp = avgMinTemp;
            this.avgMaxTemp = avgMaxTemp;
            this.stdDevTemp = stdDevTemp;
            this.avgPpt = avgPpt;
            this.stdDevPpt = stdDevPpt;
        }
        
        public MonthlyWeather()
        {
            this.avgMinTemp = -99.0;
            this.avgMaxTemp = -99.0;
            this.stdDevTemp = -99.0;
            this.avgPpt = -99.0;
            this.stdDevPpt = -99.0;
            
        }
        
    }
}
