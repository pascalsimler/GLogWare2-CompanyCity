IF OBJECT_ID('dbo.ViewStatisticBridgePerWeekConsolidated', 'V') IS NOT NULL
    DROP VIEW dbo.ViewStatisticBridgePerWeekConsolidated
GO

CREATE VIEW ViewStatisticBridgePerWeekConsolidated AS
SELECT
    YEAR(Day)                                   Year,                       
	DATEPART(WEEK, Day)                         Week,
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
GROUP BY YEAR(Day), DATEPART(WEEK, Day)
GO

