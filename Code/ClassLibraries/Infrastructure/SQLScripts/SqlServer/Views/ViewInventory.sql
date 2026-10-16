IF OBJECT_ID('dbo.ViewInventory', 'V') IS NOT NULL
    DROP VIEW dbo.ViewInventory
GO

CREATE VIEW ViewInventory AS
SELECT
	p.Name, COUNT(s.SkuId) TireCount
FROM PLACES p
LEFT OUTER JOIN skus s ON s.Place = p.Name
WHERE p.Area='GANTRY'
GROUP BY p.Name
GO

