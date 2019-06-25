//  Copyright 2006 University of Wisconsin
//  Authors:  Robert M. Scheller
//  License:  Available at  
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Landis.PestCalc
{
    public static class PestCalc
    {
    
        public static int Main(string[] args)
        {
            try {

                Assembly assembly = Assembly.GetExecutingAssembly();
                VersionRelease versionRelease = new VersionRelease(assembly);
                Console.WriteLine("Probability of Establishment Calculator {0}", versionRelease);
                Console.WriteLine("Copyright 2004-2006 University of Wisconsin");
                Console.WriteLine();
    
                if (args.Length > 1) {
                    Console.WriteLine("Error: No parameters need be specified at this point.");
                    Console.WriteLine("Error: Argument(s) on command line:");
                    StringBuilder argsList = new StringBuilder();
                    argsList.Append(" ");
                    for (int i = 1; i < args.Length; ++i)
                        argsList.AppendFormat(" {0}", args[i]);
                    Console.WriteLine(argsList.ToString());
                    return 1;
                }

                string appDir = Application.Directory;
                string inputFile = args[0];

                ParameterParser parser = new ParameterParser();
                IParameters parameters = Data.Load<IParameters>(inputFile, parser);
                
                ISpeciesData[] SpeciesDataset = parameters.SpeciesDataset;

                UI.WriteLine("Opening Pest Calculator log file \"{0}\" ...", parameters.LogFileName);
                StreamWriter log = Data.CreateTextFile(parameters.LogFileName);
                log.AutoFlush = true;

                UI.WriteLine("Opening Pest Calculator output file \"{0}\" ...", parameters.TableLogFileName);
                StreamWriter log2 = Data.CreateTextFile(parameters.TableLogFileName);
                log2.AutoFlush = true;

                Console.WriteLine("Begin calculating species establishment probabilities.");

                double[,,] establishProbs = new double[parameters.EcoregionTable.Count, 50, parameters.Timestep];

                foreach(EcoregionData ecoData in parameters.EcoregionTable)
                {
                
                    IMonthlyWeather[] ecoClimate = new IMonthlyWeather[12];
                        for(int mo = 0; mo < 12; mo++)
                            ecoClimate[mo] = parameters.MonthlyWeatherTable[ecoData.Index,mo];
                    
                    for(int t = 1; t <= parameters.Timestep; t++)
                    {

                        //Initialize identical weather and soils for all species.
                        Weather annualWeather = new Weather(ecoData.Latitude, ecoData.Longitude);
                        annualWeather.InitializeWeather(ecoClimate, t);
                        
                        Soils soils = new Soils(ecoData.FieldCapacity, ecoData.WiltingPoint);
                        soils.InitializeSoilMoisture(annualWeather, 1968);
                    
                        log.WriteLine(annualWeather.Write());
                        log.WriteLine(soils.Write());

                        for (int spp = 0; spp < 50; spp++)
                        {
                            if (parameters.SpeciesDataset[spp] == null)
                            {
                                break;
                            }
                            
                            ISpeciesData sppData = SpeciesDataset[spp];
                            
                            double tempMultiplier = annualWeather.BotkinDegreeDayMultiplier(sppData.MaxGDD, sppData.MinGDD);
                            double chillDayMultiplier = annualWeather.SykesMinimumGDDMultiplier(sppData);
                            double soilMultiplier = soils.SoilMoistureMultiplier(annualWeather, sppData.AllowableDrought);
                            double nitrogenMultiplier = soils.SoilNitrogenMultiplier(ecoData.BaseSoilN, sppData.NTolerance);
                            double minJanTempMultiplier = annualWeather.MinJanuaryTempModifier(sppData.MinJanTemp);
                            
                            log.Write("Spp = {0},", sppData.Name);
                            log.Write(" TempMultiplier(Botkin) = {0:0.00},", tempMultiplier);
                            log.Write(" MinJanTempMultiplier = {0:0.00},", minJanTempMultiplier);
                            log.Write(" SoilMultiplier = {0:0.00}.", soilMultiplier);
                            log.Write(" ChillDayMultiplier = {0:0.00}.", chillDayMultiplier);
                            log.WriteLine(" NMultiplier = {0:0.00}.", nitrogenMultiplier);
                            
                            // Liebig's Law of the Minimum is applied to the four multipliers for each year:
                            double minMultiplier = System.Math.Min(tempMultiplier, soilMultiplier);
                            minMultiplier = System.Math.Min(nitrogenMultiplier, minMultiplier);
                            minMultiplier = System.Math.Min(minJanTempMultiplier, minMultiplier);

                            establishProbs[ecoData.Index, spp, t-1] = minMultiplier;

                        }
                    
                        annualWeather = null;
                        soils = null;


                    }   // end timesteps             
                } //end ecoregions

                //Write header information with ecoregion numbers:
                foreach(EcoregionData ecoData in parameters.EcoregionTable)
                    log2.Write("{0}   ", ecoData.Number);
                log2.WriteLine("");


                for (int spp = 0; spp < 50; spp++)
                {
                    if (parameters.SpeciesDataset[spp] == null)
                    {
                        break;
                    }

                    log2.Write("{0}", parameters.SpeciesDataset[spp].Name);


                    foreach(EcoregionData ecoData in parameters.EcoregionTable)
                    {
                        double outputValue = 0.0;
                        if(parameters.Timestep == 1)
                            outputValue = establishProbs[ecoData.Index, spp, 0];
                        
                        if(parameters.MultiyearAnalysis == SummaryType.Average && parameters.Timestep > 1)
                        {
                            for(int t = 0; t < parameters.Timestep; t++)
                                outputValue += establishProbs[ecoData.Index, spp, t];
                            outputValue /= parameters.Timestep;
                        }
                        if(parameters.MultiyearAnalysis == SummaryType.Minimum && parameters.Timestep > 1)
                        {
                            for(int t = 0; t < parameters.Timestep; t++)
                                outputValue = System.Math.Min(establishProbs[ecoData.Index, spp, t], outputValue);
                        }    
                        if(parameters.MultiyearAnalysis == SummaryType.Median && parameters.Timestep > 1)
                        {
                            int aLength = parameters.Timestep;
                            double[] probs = new double[aLength];
                            for(int t = 0; t < parameters.Timestep; t++)
                                probs[t] = establishProbs[ecoData.Index, spp, t];

                            Array.Sort(probs);
                            int index = (int) System.Math.Floor((double) aLength / 2.0);
                            
                            if(aLength % 2 == 1)
                            {
                                outputValue = probs[index]; 
                            }
                            if(aLength % 2 == 0)
                            {
                                outputValue = (double) (probs[index] + probs[index - 1]) / 2.0;
                            }
                        }    
                        log2.Write("  {0:0.000}", outputValue);
                    }
                    log2.WriteLine("");
                }

                return 0;
            }
            catch (ApplicationException exc) {
                Console.WriteLine(exc.Message);
                return 1;
            }
            catch (Exception exc) {
                Console.WriteLine("Internal error occurred within the program:");
                Console.WriteLine("  {0}", exc.Message);
                if (exc.InnerException != null) {
                    Console.WriteLine("  {0}", exc.InnerException.Message);
                }
                Console.WriteLine();
                Console.WriteLine("Stack trace:");
                Console.WriteLine(exc.StackTrace);
                return 1;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Compares weights
        /// </summary>
        private static int CompareProbs(double x,
                                        double y)
        {
            if (x < y)
                return -1;
            else if (x > y)
                return 1;
            else
                return 0;
        }
    }
}
