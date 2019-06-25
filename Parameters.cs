//  Copyright 2006 University of Wisconsin
//  Authors:  
//      Robert M. Scheller
//  Version 1.0
//  License:  Available at  
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using System.Collections.Generic;

namespace Landis.PestCalc
{

    public enum SummaryType {Average, Minimum, Median};
    /// <summary>
    /// Parameters for the extension.
    /// </summary>
    public interface IParameters
    {
        int Timestep {get;}
        SummaryType MultiyearAnalysis {get;}
        string LogFileName{get;}
        string TableLogFileName{get;}
        List<EcoregionData> EcoregionTable {get;}
        IMonthlyWeather[,] MonthlyWeatherTable  {get;}
        ISpeciesData[] SpeciesDataset {get;}
    }

    /// <summary>
    /// Parameters for the plug-in.
    /// </summary>
    public class Parameters
        : IParameters
    {
        private int timestep;
        private SummaryType multiyearAnalysis;
        private string logFileName;
        private string tableLogFileName;
        private List<EcoregionData> ecoregionTable;
        private IMonthlyWeather[,] monthlyWeatherTable;
        private ISpeciesData[] speciesDataset;

        //---------------------------------------------------------------------
        /// <summary>
        /// Timesteps for analysis.
        /// </summary>
        public int Timestep
        {
            get {
                return timestep;
            }
            set {
                timestep = value;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Analysis type for summary table.
        /// </summary>
        public SummaryType MultiyearAnalysis
        {
            get {
                return multiyearAnalysis;
            }
            set {
                multiyearAnalysis = value;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Name of log file.
        /// </summary>
        public string LogFileName
        {
            get {
                return logFileName;
            }
            set {
                if (value != null) 
                    logFileName = value;
            }
        }

        //---------------------------------------------------------------------
        public string TableLogFileName
        {
            get {
                return tableLogFileName;
            }
            set {
                if (value != null) 
                    tableLogFileName = value;
            }
        }
        
        //---------------------------------------------------------------------
        public List<EcoregionData> EcoregionTable
        {
            get {
                return ecoregionTable;
            }
            set {
                if (value != null) 
                    ecoregionTable = value;
            }
        }
        
        //---------------------------------------------------------------------
        public IMonthlyWeather[,] MonthlyWeatherTable
        {
            get {
                return monthlyWeatherTable;
            }
        }
        
        //---------------------------------------------------------------------
        public ISpeciesData[] SpeciesDataset
        {
            get {
                return speciesDataset;
            }
        }

        public Parameters()
        {
            this.ecoregionTable = new List<EcoregionData>(0);
            this.monthlyWeatherTable = new IMonthlyWeather[150,12];  //150 ecoregion max
            this.speciesDataset = new ISpeciesData[50];  //a rough guess at the maximum
        }
        
        //---------------------------------------------------------------------
        public Parameters(
                        int timestep,
                        SummaryType multiyearAnalysis,
                        string logFileName,
                        string tableLogFileName,
                        List<EcoregionData> ecoregionTable,
                        IMonthlyWeather[,] monthlyWeatherTable,
                        ISpeciesData[] speciesDataset
                          )
        {
            this.timestep = timestep;
            this.multiyearAnalysis = multiyearAnalysis;
            this.logFileName = logFileName;
            this.tableLogFileName = tableLogFileName;
            this.ecoregionTable = ecoregionTable;
            this.monthlyWeatherTable = monthlyWeatherTable;
            this.speciesDataset = speciesDataset;
        }
        //---------------------------------------------------------------------
        public bool IsComplete
        {
            get {
                foreach (object parameter in new object[]{
                        timestep,
                        multiyearAnalysis,
                        logFileName,
                        tableLogFileName,
                        ecoregionTable,
                        monthlyWeatherTable,
                        speciesDataset
                        }) {
                    if (parameter == null)
                        return false;
                }
                return true;
            }
        }

        //---------------------------------------------------------------------
        public IParameters GetComplete()
        {
            if (IsComplete)
                return new Parameters(
                        timestep,
                        multiyearAnalysis,
                        logFileName,
                        tableLogFileName,
                        ecoregionTable,
                        monthlyWeatherTable,
                        speciesDataset
                        );
            else
                return null;
        }
    }
}
