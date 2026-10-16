IF OBJECT_ID('dbo.ViewStatisticFillLevel', 'V') IS NOT NULL
    DROP VIEW dbo.ViewStatisticFillLevel
GO

CREATE VIEW ViewStatisticFillLevel AS
SELECT
    Element                                         Area, 
    FORMAT(Day, 'yyyy.MM.dd')                       Day,    
    Hour                                            Hour,
    
    Counter01                                       CountTotal, 
    
    Counter02                                       CountAvailable,
    ROUND(100*Counter02/Counter01,0)                PercentAvailable,

    Counter03                                       CountBlocked,
    ROUND(100*Counter03/Counter01*100,0)            PercentBlocked,
    
    Counter04                                       CountFree,
    ROUND(100*Counter04/Counter01*100,0)            PercentFree,
   
    Counter01+Counter04                             CountOccupied, 
    ROUND(100*(Counter01-Counter04)/Counter01,0)    PercentOccupied
FROM [Statistics]
WHERE Category='FillLevel'
GO