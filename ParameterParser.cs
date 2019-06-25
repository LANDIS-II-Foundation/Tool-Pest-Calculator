//  Copyright 2006 University of Wisconsin
//  Authors:  
//      Robert M. Scheller
//  Version 1.0
//  License:  Available at  
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;
using Landis.Util;
using System.Collections.Generic;
using System.Text;

namespace Landis.PestCalc
{
    /// <summary>
    /// A parser that reads the tool parameters from text input.
    /// </summary>
    public class ParameterParser
        : Landis.TextParser<IParameters>
    {
        
        private List<EcoregionData> ecos = new List<EcoregionData>(0);

        //---------------------------------------------------------------------
        public override string LandisDataValue
        {
            get {
                return "Pest Calc";
            }
        }

        //---------------------------------------------------------------------
        public ParameterParser()
        {
             RegisterForInputValues();
        }

        //---------------------------------------------------------------------

        protected override IParameters Parse()
        {

            ReadLandisDataVar();

            Parameters parameters = new Parameters();

            InputVar<int> timestep = new InputVar<int>("Timestep");
            ReadVar(timestep);
            parameters.Timestep = timestep.Value;

            InputVar<SummaryType> st = new InputVar<SummaryType>("SummaryTableAnalysis");
            ReadVar(st);
            parameters.MultiyearAnalysis = st.Value;

            InputVar<string> logFile = new InputVar<string>("LogFile");
            ReadVar(logFile);
            parameters.LogFileName = logFile.Value;
            
            InputVar<string> tlf = new InputVar<string>("TableLogFile");
            ReadVar(tlf);
            parameters.TableLogFileName = tlf.Value;

            //---------------------------------------------------------------------
            //Read in the species data:
            InputVar<string>   sppname = new InputVar<string>("Spp Name");
            InputVar<double>   allowdr = new InputVar<double>("Allowable Drought");
            InputVar<int>   mingdd = new InputVar<int>("Minimum Growing Degree Days");
            InputVar<int>   maxgdd = new InputVar<int>("Maximum Growing Degree Days");
            InputVar<int>   mjt = new InputVar<int>("Minimum January Temp");
            InputVar<int>   mingddstar = new InputVar<int>("Minimum GDD*");
            InputVar<int>   b = new InputVar<int>("Sykes: b");
            InputVar<double>   k = new InputVar<double>("Sykes: k");
            InputVar<int>   ntol = new InputVar<int>("Nitrogen Tolerance");

            const string nextTableName = "EcoregionTable";

            int sppCnt = 0;
            while (! AtEndOfInput && CurrentName != nextTableName)
            {
                StringReader currentLine = new StringReader(CurrentLine);

                ISpeciesData sppdata = new SpeciesData();            
                parameters.SpeciesDataset[sppCnt] = sppdata;

                ReadValue(sppname, currentLine);
                sppdata.Name = sppname.Value;
                
                ReadValue(allowdr, currentLine);
                sppdata.AllowableDrought = allowdr.Value;
                
                ReadValue(mingdd, currentLine);
                sppdata.MinGDD = mingdd.Value;
                
                ReadValue(maxgdd, currentLine);
                sppdata.MaxGDD = maxgdd.Value;
                
                ReadValue(mjt, currentLine);
                sppdata.MinJanTemp = mjt.Value;

                ReadValue(mingddstar, currentLine);
                sppdata.MinGDDstar = mingddstar.Value;

                ReadValue(b, currentLine);
                sppdata.B = b.Value;

                ReadValue(k, currentLine);
                sppdata.K = k.Value;

                ReadValue(ntol, currentLine);
                sppdata.NTolerance = ntol.Value;

                CheckNoDataAfter("the " + ntol.Name + " column",
                                 currentLine);

                sppCnt++;
                GetNextLine();
                
            }

            //---------------------------------------------------------------------
            //Read in the ecoregion data:
            
            ReadName(nextTableName);

            InputVar<int>   econ = new InputVar<int>("Ecoregion number");
            InputVar<double>   fc = new InputVar<double>("Field Capacity");
            InputVar<double>   wp = new InputVar<double>("Wilting Point");
            InputVar<double>  lat = new InputVar<double>("Latitude");
            InputVar<double> longi = new InputVar<double>("Longitude");
            InputVar<double> avn = new InputVar<double>("Available Nitrogen");

            const string nextTableName2 = "MonthlyClimateData";

            int ecoCnt = 0;
            while (! AtEndOfInput && CurrentName != nextTableName2)
            {
                StringReader currentLine = new StringReader(CurrentLine);

                EcoregionData ecoData = new EcoregionData();
                
                ecoData.Index = ecoCnt;

                ReadValue(econ, currentLine);
                ecoData.Number = econ.Value;

                ReadValue(fc, currentLine);
                ecoData.FieldCapacity = fc.Value;

                ReadValue(wp, currentLine);
                ecoData.WiltingPoint = wp.Value;
                
                ReadValue(lat, currentLine);
                ecoData.Latitude = lat.Value;
                
                ReadValue(longi, currentLine);
                ecoData.Longitude = longi.Value;

                ReadValue(avn, currentLine);
                ecoData.BaseSoilN = avn.Value;

                CheckNoDataAfter("the " + avn.Name + " column",
                                 currentLine);

                ecos.Add(ecoData);

                ecoCnt++;
                GetNextLine();
                
            }
            
            parameters.EcoregionTable = ecos;


            //TO DO :  READ IN ECO TABLE
            //CREATE AN int[] of ecoregion numbers
            //CREATE METHOD to retrieve ecoindex.
            
            //---------------------------------------------------------------------
            //Read in climate data:

            ReadName(nextTableName2);

            InputVar<int>    econum     = new InputVar<int>("The Ecoregion");
            InputVar<int>    month      = new InputVar<int>("The Month");
            InputVar<double> avgMinTemp = new InputVar<double>("Monthly Value");
            InputVar<double> avgMaxTemp = new InputVar<double>("Monthly Value");
            InputVar<double> stdDevTemp = new InputVar<double>("Monthly Value");
            InputVar<double> avgPpt     = new InputVar<double>("Monthly Value");
            InputVar<double> stdDevPpt  = new InputVar<double>("Monthly Value");

            while (! AtEndOfInput)
            {
                StringReader currentLine = new StringReader(CurrentLine);

                ReadValue(econum, currentLine);
                int eco = econum.Value.Actual;
                
                ReadValue(month, currentLine);
                int mo = month.Value.Actual;

                IMonthlyWeather moClimate = new MonthlyWeather();            

                parameters.MonthlyWeatherTable[GetEcoIndex(eco), mo-1] = moClimate;
                
                ReadValue(avgMinTemp, currentLine);
                moClimate.AvgMinTemp = avgMinTemp.Value;

                ReadValue(avgMaxTemp, currentLine);
                moClimate.AvgMaxTemp = avgMaxTemp.Value;

                ReadValue(stdDevTemp, currentLine);
                moClimate.StdDevTemp = stdDevTemp.Value;
                
                ReadValue(avgPpt, currentLine);
                moClimate.AvgPpt = avgPpt.Value;
                
                ReadValue(stdDevPpt, currentLine);
                moClimate.StdDevPpt = stdDevPpt.Value;
                
                CheckNoDataAfter("the " + stdDevPpt.Name + " column",
                                 currentLine);

                GetNextLine();
                
            }
            
            return parameters.GetComplete();
        }

        private int GetEcoIndex(int ecoNumber)
        {
            int index = -1;
            foreach(EcoregionData ecoData in ecos)
                if (ecoData.Number == ecoNumber)
                    return ecoData.Index;
            if (index == -1)
                throw new InputValueException(ecoNumber.ToString(),
                    "The ecoregion number {0} was defined in the ecoregion table",
                    ecoNumber);
            
            return index;
        }
        
        // -----------------------------------------------------------
        /// <summary>
        // Functions for validating enumerator types:
        /// </summary>
        public static SummaryType SummaryTypeParse(string word)
        {
            if (word == "Average")
                return SummaryType.Average;
            else if (word == "Minimum")
                return SummaryType.Minimum;
            else if (word == "Median")
                return SummaryType.Median;
            throw new System.FormatException("Valid types:  Average, Minimum, Median");
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Registers the appropriate method for reading input values.
        /// </summary>
        public static void RegisterForInputValues()
        {
            Type.SetDescription<SummaryType>("Summary Type for Output Table");
            InputValues.Register<SummaryType>(SummaryTypeParse);
        }

    }
}
