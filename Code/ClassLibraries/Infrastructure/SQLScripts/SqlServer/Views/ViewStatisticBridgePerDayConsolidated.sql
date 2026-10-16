IF OBJECT_ID('dbo.ViewStatisticBridgePerDayConsolidated', 'V') IS NOT NULL
    DROP VIEW dbo.ViewStatisticBridgePerDayConsolidated
GO

CREATE VIEW ViewStatisticBridgePerDayConsolidated AS
SELECT
	Day,
	
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
GROUP BY Day
GO

