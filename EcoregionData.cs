//  Copyright 2006 University of Wisconsin
//  Authors:  Robert M. Scheller
//  License:  Available at  
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using System.Collections.Generic;

namespace Landis.PestCalc
{
    /// <summary>
    /// Ecoregion data.
    /// </summary>
    public interface IEcoregionData
    {

        int Number{get;set;}
        int Index{get; set;}
        double FieldCapacity{get;set;}
        double WiltingPoint{get;set;}
        double Latitude{get; set;}
        double Longitude{get; set;}
        double BaseSoilN{get;set;}

    }
}



namespace Landis.PestCalc
{

    public class EcoregionData
    : IEcoregionData
    {

        private int number;
        private int index;
        private double fieldCapacity;
        private double wiltingPoint;
        private double latitude;
        private double longitude;
        private double baseSoilN;

        public int Number
        {
            get {
                return number;
            }
            set {
                number = value;
            }
        }
        
        public int Index
        {
            get {
                return index;
            }
            set {
                index = value;
            }
        }
        
        public double FieldCapacity
        {
            get {
                return fieldCapacity;
            }
            set {
                fieldCapacity = value;
            }
        }
        public double WiltingPoint
        {
            get {
                return wiltingPoint;
            }
            set {
                wiltingPoint = value;
            }
        }
        
        public double Latitude
        {
            get {
                return latitude;
            }
            set {
                latitude = value;
            }
        }

        public double Longitude
        {
            get {
                return longitude;
            }
            set {
                longitude = value;
            }
        }

        public double BaseSoilN
        {
            get {
                return baseSoilN;
            }
            set {
                baseSoilN = value;
            }
        }
        public EcoregionData(
                            int number,
                            int index,
                            double fieldCapacity,
                            double wiltingPoint,
                            double latitude,
                            double longitude,
                            double baseSoilN
                            )
        {
            this.number = number;
            this.index = index;
            this.fieldCapacity = fieldCapacity;
            this.wiltingPoint = wiltingPoint;
            this.latitude = latitude;
            this.longitude = longitude;
            this.baseSoilN = baseSoilN;
        }
        
        public EcoregionData()
        {
        }
        
    }
}
