//  Copyright 2006 University of Wisconsin
//  Authors:  Robert M. Scheller
//  License:  Available at  
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using System.Collections.Generic;
using System.IO;
using System;

namespace Landis.PestCalc
{

    public class Soils
    {
        private int dryDays;
        private double actualET;
        private double annualPotentialET;
        private double fieldCapacity;
        private double fieldWiltingPoint;

        static Soils()
        {
        }
        
        public Soils(double fc, double wp)
        {
            fieldCapacity = fc;
            fieldWiltingPoint = wp;
            
        }        
        
        public int DryDays
        {
            get {
                return dryDays;
            }
        }
        public double AET
        {
            get {
                return actualET;
            }
        }
        public double AnnualPET
        {
            get {
                return annualPotentialET;
            }
        }
        public double FieldCapacity
        {
            get {
                return fieldCapacity;
            }
        }
        public double FieldWiltingPoint
        {
            get {
                return fieldWiltingPoint;
            }
        }
        public double SoilNitrogenMultiplier(double baseSoilN, int Ntolerance)
        {
            //Calc species soil N growth multiplier (Mitchell and Chandler.
            //1939. Black Rock Forest Bull. 11, Aber et al. 1979. Can. J. For.
            //Res. 9:10 - 14.
            double a, b, c, d, e;
            double soilNitrogenMultiplier = 0.0;
            
            double availableN = baseSoilN * 0.015; //conversion of base soil N from LINKAGES
            
            availableN += 0.005;
            double availMC = -170.0 + 4.0 * (availableN * 1000.0);
        
            if(Ntolerance == 1)  //Intolerant to low nitrogen
            {
                a = 2.99;
                b = 207.43;
                c = 0.00175;
                d = -1.7;
                e = 1.0;
            } else if (Ntolerance == 2) //Mid-tolerant of low nitrogen
            {
                a = 2.94;
                b = 117.52;
                c = 0.00234;
                d = -0.5;
                e = 0.5;
            } else if (Ntolerance == 3) //Tolerant of low nitrogen
            {
                a = 2.79;
                b = 219.77;
                c = 0.00179;
                d = -0.3;
                e = 0.6;
            } else
                throw new System.ApplicationException("Error: Incorrect N tolerance value .");
            
         

            //concNinLeaves = percent N in green leaves
            double concNinLeaves = a * (1.0 - System.Math.Pow(10.0, ((-1.0 * c) * (availMC + b))));
        
            soilNitrogenMultiplier = d + (e * concNinLeaves); //(3) Aber 1979
            
            soilNitrogenMultiplier = Math.Min(1.0, soilNitrogenMultiplier);
            soilNitrogenMultiplier = Math.Max(0.0, soilNitrogenMultiplier);
            
            return soilNitrogenMultiplier;
        }


        public double SoilMoistureMultiplier(Weather weather, double sppAllowableDrought)
        //Calc soil moisture multipliers based on Degree_Day (supplied by calc_temperature()),
        //dryDays (supplied by MOIST).

        {
            double growDays = 0.0;
            double maxDrought;
            double Soil_Moist_GF = 0.0;

            growDays = weather.EndGrowing - weather.BeginGrowing + 1.0;
            if (growDays < 2.0)
                throw new System.ApplicationException("Error: Too few growing days .");

            //Calc species soil moisture multipliers
            maxDrought = sppAllowableDrought * growDays;
            if (maxDrought < this.dryDays) 
            {
                Soil_Moist_GF = 0.0;
            }
            else
            {
                Soil_Moist_GF = System.Math.Sqrt((double)(maxDrought - this.dryDays)/maxDrought);
            }
            return Soil_Moist_GF;
        }


        public void InitializeSoilMoisture(Weather weather, int currentYear)
        //Calc fraction of growing season with unfavorable soil moisture
        //for growth (Dry_Days_in_Grow_Seas) used in SoilMoistureMultiplier to determine soil
        //moisture growth multipliers and Actual EvapoTranspiration (AET) used
        //in DECOMP to determine decay rates.
        //
        //Simulates method of Thorthwaite and Mather (1957) as modified
        //by Pastor and Post (1984).
        //
        //mean_temp = average monthly temperatures (jan-dec)  centigrade
        //mean_rain = average monthly rainfall     (jan-dec)  centimeters
        //var_rain  = standard deviation of average monthly rainfall
        //field_cap = centimeters of water the soil can hold at field capacity
        //field_dry = centimeters of water below which tree growth stops
        //            (-15 bars)
        //beg_grow_seas = year day on which the growing season begins
        //end_grow_seas = year day on which the growing season ends
        //latitude = latitude of region (degrees north)

        {

            double  xFieldCap,            //
            waterAvail,           //
            accPotWaterLoss,      //
            tempEfficiency,                   //
            aExponentET,        //
            julianDay,           //
            oldWaterAvail,      //
            monthlyRain,         //
            potWaterLoss,       //
            potentialET,               //
            tempFac,             //
            xAccPotWaterLoss, //
            changeSoilMoisture, //
            oldJulianDay,       //
            dryDayInterp;       //

            //Initialize water content of soil in January to Field_Cap (mm)
            xFieldCap = 10.0 * fieldCapacity;
            waterAvail = fieldCapacity;
    
            //Initialize Thornwaithe parameters:
            //
            //TE = temperature efficiency
            //aExponentET = exponent of evapotranspiration function
            //pot_et = potential evapotranspiration
            //aet = actual evapotranspiration
            //acc_pot_water_loss = accumulated potential water loss
    
            actualET = 0.0;
            accPotWaterLoss = 0.0;
            tempEfficiency = 0.0;
  
            for (int i = 0; i < 12; i++) 
            {
                tempFac = 0.2 * weather.MonthlyTemp[i];
      
                if (tempFac > 0.0)
                    tempEfficiency += System.Math.Pow(tempFac, 1.514);
            }
    
            aExponentET = 0.675 * System.Math.Pow(tempEfficiency, 3) - 
                            77.1 * (tempEfficiency * tempEfficiency) +
                            17920.0 * tempEfficiency + 492390.0;
            aExponentET *= (0.000001);
    
            //Initialize the number of dry days and current day of year
            dryDays = 0;
            julianDay = 15.0;
    
    
            for (int i = 0; i < 12; i++) 
            {
                double daysInMonth = Weather.DaysInMonth(i, currentYear);
                oldWaterAvail = waterAvail;
                monthlyRain = weather.MonthlyPrecip[i];
                tempFac = 10.0 * weather.MonthlyTemp[i];
                
                //Calc potential evapotranspiriation (potentialET) Thornwaite and Mather,
                //1957.  Climatology 10:83 - 311.
                if (tempFac > 0.0) 
                {

                    potentialET = 1.6 * (System.Math.Pow((tempFac / tempEfficiency), aExponentET)) * 
                            Weather.LatitudeCorrection(i, weather.Latitude);
                } 
                else 
                {
                    potentialET = 0.0;
                }
                
                annualPotentialET += potentialET;
      
                //Calc potential water loss this month
                potWaterLoss = monthlyRain - potentialET;
      
                //If monthlyRain doesn't satisfy potentialET, add this month's potential
                //water loss to accumulated water loss from soil
                if (potWaterLoss < 0.0) 
                {
                    accPotWaterLoss += potWaterLoss;
                    xAccPotWaterLoss = accPotWaterLoss * 10;
      
                    //Calc water retained in soil given so much accumulated potential
                    //water loss Pastor and Post. 1984.  Can. J. For. Res. 14:466:467.
      
                    waterAvail = fieldCapacity * 
                                 System.Math.Exp((.000461 - 1.10559 / xFieldCap) * (-1.0 * xAccPotWaterLoss));
      
                    if (waterAvail < 0.0)
                        waterAvail = 0.0;
      
                    //changeSoilMoisture - during this month
                    changeSoilMoisture = waterAvail - oldWaterAvail;
      
                    //Calc actual evapotranspiration (AET) if soil water is drawn down
                    actualET += (monthlyRain - changeSoilMoisture);
                } 

                //If monthlyRain satisfies potentialET, don't draw down soil water
                else 
                {
                    waterAvail = oldWaterAvail + potWaterLoss;
                    if (waterAvail >= fieldCapacity)
                        waterAvail = fieldCapacity;
                    changeSoilMoisture = waterAvail - oldWaterAvail;
 
                    //If soil partially recharged, reduce accumulated potential
                    //water loss accordingly
                    accPotWaterLoss += changeSoilMoisture;
      
                    //If soil completely recharged, reset accumulated potential
                    //water loss to zero
                    if (waterAvail >= fieldCapacity)
                        accPotWaterLoss = 0.0;
      
                    //If soil water is not drawn upon, add potentialET to AET
                    actualET += potentialET;
                }
      
                oldJulianDay = julianDay;
                julianDay += daysInMonth;
                dryDayInterp = 0.0;
    
                //Increment number of dry days, truncate
                //at end of growing season
                if ((julianDay > weather.BeginGrowing) && (oldJulianDay < weather.EndGrowing)) 
                {
                    if ((oldWaterAvail >= fieldWiltingPoint)  && (waterAvail >= fieldWiltingPoint))
                    {
                    }
                    else if ((oldWaterAvail > fieldWiltingPoint) && (waterAvail < fieldWiltingPoint)) 
                    {
                        dryDayInterp = daysInMonth * (fieldWiltingPoint - waterAvail) / 
                                        (oldWaterAvail - waterAvail);
                        if ((oldJulianDay < weather.BeginGrowing) && (julianDay > weather.BeginGrowing))
                            if ((julianDay - weather.BeginGrowing) < dryDayInterp)
                                dryDayInterp = julianDay - weather.BeginGrowing;
    
                        if ((oldJulianDay < weather.EndGrowing) && (julianDay > weather.EndGrowing))
                            dryDayInterp = weather.EndGrowing - julianDay + dryDayInterp;
    
                        if (dryDayInterp < 0.0)
                            dryDayInterp = 0.0;
    
                    } 
                    else if ((oldWaterAvail < fieldWiltingPoint) && (waterAvail > fieldWiltingPoint)) 
                    {
                        dryDayInterp = daysInMonth * (fieldWiltingPoint - oldWaterAvail) / 
                                        (waterAvail - oldWaterAvail);
          
                        if ((oldJulianDay < weather.BeginGrowing) && (julianDay > weather.BeginGrowing))
                            dryDayInterp = oldJulianDay + dryDayInterp - weather.BeginGrowing;
    
                        if (dryDayInterp < 0.0)
                            dryDayInterp = 0.0;
    
                        if ((oldJulianDay < weather.EndGrowing) && (julianDay > weather.EndGrowing))
                            if ((weather.EndGrowing - oldJulianDay) < dryDayInterp)
                                dryDayInterp = weather.EndGrowing - oldJulianDay;
                    } 
                    else 
                    {
                        dryDayInterp = daysInMonth;
          
                        if ((oldJulianDay < weather.BeginGrowing) && (julianDay > weather.BeginGrowing))
                            dryDayInterp = julianDay - weather.BeginGrowing;
    
                        if ((oldJulianDay < weather.EndGrowing) && (julianDay > weather.EndGrowing))
                            dryDayInterp = weather.EndGrowing - oldJulianDay;
                    }
      
                    dryDays += (int) dryDayInterp;
                }
            }  //END MONTHLY CALCULATIONS
  
            //Convert AET from cm to mm
            //actualET *= 10.0;

            //Calculate AET multiplier
            //(used to be done in decomp)
            //float aetMf = min((double)AET,600.0);
            //AET_Mult = (-1. * aetMf) / (-1200. + aetMf);
        }
        
        public string Write()
        {
            string s = String.Format(            
                " Soils:  Dry Days = {0}." + 
                " Actual ET = {1:0.0}." + 
                " Potential ET = {2:0.0}." + 
                " Field Capcity = {3}." + 
                " Field Wilting Point = {4}.", 
                this.DryDays, this.AET, this.AnnualPET, this.FieldCapacity, this.FieldWiltingPoint);
            return s;
        }

    }
}
