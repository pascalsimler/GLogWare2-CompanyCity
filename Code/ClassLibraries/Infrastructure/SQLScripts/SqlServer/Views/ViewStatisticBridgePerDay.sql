IF OBJECT_ID('dbo.ViewStatisticBridgePerDay', 'V') IS NOT NULL
    DROP VIEW dbo.ViewStatisticBridgePerDay
GO

CREATE VIEW ViewStatisticBridgePerDay AS
SELECT
	Element                                     BridgeNr,
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
GROUP BY Element, Day
GO

