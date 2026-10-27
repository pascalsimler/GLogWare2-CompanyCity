IF OBJECT_ID('dbo.ViewBridgeUtilization', 'V') IS NOT NULL
    DROP VIEW dbo.ViewBridgeUtilization     
GO

CREATE VIEW ViewBridgeUtilization AS

select 
	day, hour,
	CAST([OP31003BR]/36 as decimal(5,1)) B11,
	CAST([OP32003BR]/36 as decimal(5,1)) B12, 
	CAST([OP33003BR]/36 as decimal(5,1)) B13, 
	CAST([OP34003BR]/36 as decimal(5,1)) B14, 
	CAST([OP35003BR]/36 as decimal(5,1)) B15, 
	CAST([OP41003BR]/36 as decimal(5,1)) B21,
	CAST([OP42003BR]/36 as decimal(5,1)) B22, 
	CAST([OP43003BR]/36 as decimal(5,1)) B23, 
	CAST([OP44003BR]/36 as decimal(5,1)) B24, 
	CAST([OP45003BR]/36 as decimal(5,1)) B25 
from
(
select 
	bridge, 
	cast(starttime as date) day, 
	datepart(hour, starttime) hour, 
	duration 
	--SUm(duration) duration, ROUND(SUM(duration)/36, 1) utilisation
from cruchot
--where CAST(StartTime AS date) = CAST('2026-09-25' as date)
--group by  bridge, datepart(hour, starttime)
) t
PIVOT
(
   SUM(duration)
   FOR bridge IN (
	[OP31003BR], [OP32003BR], [OP33003BR], [OP34003BR], [OP35003BR], 
	[OP41003BR], [OP42003BR], [OP43003BR], [OP44003BR], [OP45003BR]
   )
) t1
-- order by day, hour

GO

