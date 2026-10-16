IF OBJECT_ID('dbo.ViewStatisticBridgePerHourConsolidated', 'V') IS NOT NULL
    DROP VIEW dbo.ViewStatisticBridgePerHourConsolidated
GO

CREATE VIEW ViewStatisticBridgePerHourConsolidated AS
SELECT
	Day,                       
    RIGHT('00'+CAST(Hour AS VARCHAR(2)), 2) +
    '->' +
    RIGHT('00'+CAST(Hour+1 AS VARCHAR(2)), 2)   Hour,
	
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
GROUP BY Day, Hour
GO

