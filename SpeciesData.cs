//  Copyright 2008 University of Wisconsin, Conservation Biology Institute
//  Authors:  Robert M. Scheller
//  License:  Available at  
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using System.Collections.Generic;

namespace Landis.PestCalc
{
    /// <summary>
    /// Weather parameters for each month.
    /// </summary>
    public interface ISpeciesData
    {

        string Name{get;set;}
        double AllowableDrought{get;set;}

        int MinGDDstar{get;set;}    // See Sykes, Prentice, and Cramer.  1996.  JBiogeog 23: 203-233.
        int B{get; set;}            // See Sykes, Prentice, and Cramer.  1996.  JBiogeog 23: 203-233.
        double K{get; set;}         // See Sykes, Prentice, and Cramer.  1996.  JBiogeog 23: 203-233.
        
        int MinGDD{get;set;}
        int MaxGDD{get;set;}
        int MinJanTemp{get;set;}
        int MaxJanTemp{get; set;}
        int MaxJulyTemp{get; set;}

        int NTolerance{get; set;}
    }
}



namespace Landis.PestCalc
{

    public class SpeciesData
    : ISpeciesData
    {

        private string name;
        private double allowableDrought;
        
        private int minGDDstar;
        private int b;
        private double k;
        
        private int minGDD;
        private int maxGDD;
        private int minJanTemp;
        private int maxJanTemp;
        private int maxJulyTemp;
        
        private int nTolerance;

        public string Name
        {
            get {
                return name;
            }
            set {
                name = value;
            }
        }

        public double AllowableDrought
        {
            get {
                return allowableDrought;
            }
            set {
                allowableDrought = value;
            }
        }
        public int MinGDD
        {
            get {
                return minGDD;
            }
            set {
                minGDD = value;
            }
        }
        public int MaxGDD
        {
            get {
                return maxGDD;
            }
            set {
                maxGDD = value;
            }
        }
        public int MinGDDstar
        {
            get {
                return minGDDstar;
            }
            set {
                minGDDstar = value;
            }
        }
        public int B
        {
            get {
                return b;
            }
            set {
                b = value;
            }
        }

        public double K
        {
            get {
                return k;
            }
            set {
                k = value;
            }
        }
        public int MinJanTemp
        {
            get {
                return minJanTemp;
            }
            set {
                minJanTemp = value;
            }
        }
        public int MaxJanTemp
        {
            get {
                return maxJanTemp;
            }
            set {
                maxJanTemp = value;
            }
        }
        public int MaxJulyTemp
        {
            get {
                return maxJulyTemp;
            }
            set {
                maxJulyTemp = value;
            }
        }

        public int NTolerance
        {
            get {
                return nTolerance;
            }
            set {
                nTolerance = value;
            }
        }
        public SpeciesData(
                            string name,
                            double allowableDrought,
                            //int minGDDstar,
                            //int b,
                            //double k,
                            int minGDD,
                            int maxGDD,
                            int minJanTemp,
                            int maxJanTemp,
                            int maxJulyTemp,
                            int nTolerance
                            )
        {
            this.name = name;
            this.allowableDrought = allowableDrought;
            //this.minGDDstar = minGDDstar;
            //this.b = b;
            //this.k = k;
            this.minGDD     = minGDD;
            this.maxGDD     = maxGDD;
            this.minJanTemp = minJanTemp;
            this.maxJanTemp = maxJanTemp;
            this.maxJulyTemp = maxJulyTemp;
            this.nTolerance = nTolerance;
        }
        
        public SpeciesData()
        {
        }
        
    }
}
