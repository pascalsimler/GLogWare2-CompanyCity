IF OBJECT_ID('dbo.ViewStatisticBridgePerMonth', 'V') IS NOT NULL
    DROP VIEW dbo.ViewStatisticBridgePerMonth
GO

CREATE VIEW ViewStatisticBridgePerMonth AS
SELECT
	Element                                     BridgeNr,
    YEAR(Day)                                   Year,                       
	MONTH(Day)                                  Month,
    SUM(Counter01)                              CountStacksIn, 
    SUM(Counter02)                              CountStacksOut,  
    SUM(Counter03)                              CountStacksRelocated,  
    SUM(Counter01+Counter02+Counter03)          CountStacksTotal,

    SUM(Counter01)                              CountTiresIn, 
    SUM(Counter02)                              CountTiresOut, 
    SUM(Counter03)                              CountTiresRelocated,
    SUM(Counter01+Counter02+Counter03)          CountTiresTotal
FROM [Statistics]
WHERE Category='Bridge'
GROUP BY Element, YEAR(Day), MONTH(Day)
GO

