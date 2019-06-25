# Tool-Pest-Calculator

Running the Probability of Establishment (Pest) Calculator 

The Pest calculator is designed expressly for creating probability of establishment 
values for input into the LANDIS-II forest landscape simulation model.  The associated executable must be placed within the c:\program files\landis-ii\5.1\bin
folder.  The 5.1 version of LANDIS-II must be installed.


The Pest calculator is largely derived from the LINKAGES model (Pastor and Post 1988).  The Pest calculator estimates four different modifiers that are assumed to 
alter the probability of establishment:
1.  Temperature.  Botkin's 1972 GDD envelop determines the temperature modifier.  The Degree Day Base is 5.56 (C) or 42 (F).
2.  Soil Moisture.  The method of Thorthwaite and Mather (1957) as modified by Pastor and 
Post (1984) is used to calculate the number of days with soil moisture below the 
wilting point.  This value is compared to the fraction of growing days with drought
conditions that each species can tolerate:  

	SoilMoistureModifier  = Sqrt(MaxDrought - DryDays / MaxDrought)
	
	where MaxDrought is the maximum fraction of growing degree days with soil
moisture below the wilting point for each species and DryDays is the number of 
days with soil moisture below the wilting point for the current year.
3.  Minimum January Temperature.  If the simulated mean monthly temperature is below 
the species threshold, then this modifier is zero.
4.  Nitrogen tolerance.  For each ecoregion, a base mean soil nitrogen (Mg/ha) 
determines the Nitrogen availability modifier according to the equations 
provided by Aber et al. 1979 (Can. J. For. Res.).

At each user-determined time step, a random climate is produced based on the 
monthly climatic inputs (see the sample input file for units; standard deviations are calculated from inter-annual data).  This random climate is used to determine the four modifiers above.  The four modifiers are combined using Liebig's 1898 (?) Law of the Minimum whereby the lowest modifier determines the probability of establishment for that time step for that species.

If multiple time steps are indicated, the PestSppEcoregionTable will produce the average, median, or minimum for the multiple climates.  The choice between Average, Median, or Minimum is indicated by the SummaryTableAnalysis variable near the top of the input file.  The file, PestSppEcoregionTable.txt was formatted to be copied directly into the LANDIS-II biomass succession input file.

In addition, the Pest Calculator produces a log file with data about the climate
and soils at each time step and for each ecoregion.  Also listed within the log file are the four modifiers for each species, given the climate and soil parameters generated.  When using the Pest calculator, look through this file carefully!   If a species appears to have a consistently lower than expected value, you will be able to determine which modifier is causing the low value and therefore check the most relevant parameters.

Note:  If you are simulating climate change, the Pest calculator will need to be run separately for each climatic time step.  For example, if you are simulating climate change, a new Pest calculator input file will need to be created for every new climate.

Note:  The climate inputs were designed to be maximally compatible with PnET-II input files, thereby reducing the reformatting necessary to run both models.

Note:  Min and Max GDD refer to the minimum and maximum bounds for a temperature envelope defined by growing degree days (Botkins 1973).  Together they define a parabolic curve with the optimal conditions (modifier = 1.0) at the center.  For North America, existing species distributions were used to estimate these parameters.

Note:  Wilting point is defined as the amount of water in soil below which water potential exceeds (falls below) -1500 J/kg.   At this point, it is assumed that the water is no longer accessible to plants.

Note:  A maximum of 150 ecoregions currently allowed.

References:

Aber, J. D.; Botkin, D. B., and Melillo, J. M. Predicting the effects of different harvesting regimes on productivity and yield in northern hardwoods. Canadian Journal of Forest Research. 1979; 9:10-14.

Botkin, D. B.; Janak, J. F., and Wallis, J. R. Some ecological consequences of a computer model of forest growth. Journal of Ecology. 1973; 60:849-872.

Pastor, J. and Post, W. M.  Development of a linked forest productivity-soil process model.  Oak Ridge, Tennessee, USA: Oak Ridge National Laboratory; 1986; ORNL/TM-9519. 
