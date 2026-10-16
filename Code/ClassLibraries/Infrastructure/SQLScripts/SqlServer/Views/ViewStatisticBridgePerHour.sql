IF OBJECT_ID('dbo.ViewStatisticBridgePerHour', 'V') IS NOT NULL
    DROP VIEW dbo.ViewStatisticBridgePerHour
GO

CREATE VIEW ViewStatisticBridgePerHour AS
SELECT
	Element                                     BridgeNr,
    Day,                       
    RIGHT('00'+CAST(Hour AS VARCHAR(2)), 2) +
    '->' +
    RIGHT('00'+CAST(Hour+1 AS VARCHAR(2)), 2)   Hour,
	
    Counter01                                   CountStacksIn, 
    Counter02                                   CountStacksOut,  
    Counter03                                   CountStacksRelocated,  
    Counter01+Counter02+Counter03               CountStacksTotal,

    Counter01                                   CountTiresIn, 
    Counter02                                   CountTiresOut, 
    Counter03                                   CountTiresRelocated,
    Counter01+Counter02+Counter03               CountTiresTotal
FROM [Statistics]
WHERE Category='Bridge'
GO

