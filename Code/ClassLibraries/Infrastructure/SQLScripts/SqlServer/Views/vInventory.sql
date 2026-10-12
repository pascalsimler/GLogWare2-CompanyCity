create or alter view vInventory as

select
	p.Name, count(s.SkuId) TireCount
from PLACES p
left outer join skus s on s.Place = p.Name
where p.Area='GANTRY'
group by p.Name