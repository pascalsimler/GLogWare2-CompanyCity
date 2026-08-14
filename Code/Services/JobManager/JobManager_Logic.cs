using Gudel.GLogWare.Entities;
using Gudel.GLogWare.Logging;
using Microsoft.EntityFrameworkCore;

namespace Gudel.GLogWare.Services.JobManager;

public partial class JobManager
{
    private async Task DoWork()
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        await ProcessWaitingJobs();
        await CreateJobsForOutputOrders();
        await SearchPlaceInStore("Cruchot");
        
        _logger.LogInformation(LogMessages.LeaveMethod);
    }

    private async Task ProcessWaitingJobs()
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        var waitingJobs = _db.Jobs
            .Include(j => j.ActualPlaceRecord)
            .Where(j => j.Status == nameof(JobStatusIdentifiers.WAIT_ON_JOBMANAGER))
            .OrderBy(j => j.ModifiedAt);

        foreach (Job job in waitingJobs)
        {
            _logger.LogInformation($"JobId=[{job.Jobid}]");
            _logger.LogInformation($"ActualPlace=[{job.ActualPlace}]");
            _logger.LogInformation($"ActualPlaceRecord.PlaceType=[{job.ActualPlaceRecord.PlaceType}]");
            switch (job.ActualPlaceRecord.PlaceType)
            {
                case nameof(PlaceTypeIdentifiers.GANTRY_PICK):
                    _logger.LogInformation($"CRUCHOTAGE !!!");
                    break;

                default:
                    break;
            }
            _logger.LogInformation($"----------------------------------");
        }

        _logger.LogInformation(LogMessages.LeaveMethod);
    }

    private async Task CreateJobsForOutputOrders()
    {
        _logger.LogInformation(LogMessages.EnterMethod);

        _logger.LogInformation(LogMessages.LeaveMethod);
    }

    private async Task SearchPlaceInStore(string iJobId, string? iRequestedPlace = null)
    {
        _logger.LogInformation(LogMessages.EnterMethod);
        _logger.LogInformation($"iJobId=[{iJobId}]");
        _logger.LogInformation($"iRequestedPlace=[{iRequestedPlace}]");

        string oPlaceFound = string.Empty;

        var q = _db.Places
            .Where(p => 
                p.Area == "GANTRY" &&
                p.Bridge == "OP7100BR" &&
                p.Name == ((iRequestedPlace == null) ? p.Name : iRequestedPlace) &&
                (!p.Skus.Any() || p.Skus.Max(s => s.Article) == "Cruchot") 
            )
            .OrderByDescending( p => p.Skus.Count() )
            .ThenBy(p => p.Zone)
            .ThenBy(p=>p.Distance)
            .Select(p => new
            {
                p.Name, p.XCell, p.YCell, 
                SkuCount = p.Skus.Count(),
                Article = p.Skus.Max(s => s.Article),
                p.Zone,
                p.Distance
            })
            .Take(50)
        ;
        foreach (var r in q)
        {
            _logger.LogInformation($"Name=[{r.Name}], XCell=[{r.XCell}], YCell=[{r.YCell}],  Article=[{r.Article}], SkuCount=[{r.SkuCount}], Zome=[{r.Zone}], Distance=[{r.Distance}]");
        }

//                    pl.PLACE,
//                    pl.BRIDGE,
//                    MAX(pl.DISTANCE) DISTANCE,
//                    MAX(pl.ZONE) ZONE,
//                    COUNT(s.SKUID) CNTSKU,
//                    MAX(s.CAI) CAI,
//                    MAX(s.LPC) LPC

        //                FROM PLACE pl, SKU s

        //                WHERE s.PLACE(+) = pl.PLACE

        //                AND pl.AREA = 'KOM' AND NVL(pl.LOCKED, '0') = '0'

        //                AND pl.BRIDGE = DECODE(sDefaultBridge, 0, sDefaultBridge, pl.Bridge)

        //                AND pl.PLACE = NVL(iRequestedPlace, pl.PLACE)

        //                GROUP BY pl.PLACE, pl.BRIDGE




        //retCode:= pack_global.RetCodeNOK;
        //oRetMsg:= 'No place could be found !!';

        //        SELECT

        //            COUNT(*),
        //			MAX(s.CAI),
        //			MAX(s.LPC),
        //			MAX(pl.BRIDGE)

        //        INTO
        //            sSkuCount,
        //            sCai,
        //            sLpc,
        //            sDefaultBridge

        //        FROM SKU s, PLACE pl

        //        WHERE pl.PLACE = s.PLACE

        //        AND s.JOBID = iJobId;
        //        TRACE.LOG(fctName, 7, 'I', 'TS_S', 'sSkuCount=[' || TO_CHAR(sSkuCount) || ']');
        //        TRACE.LOG(fctName, 7, 'I', 'TS_S', 'CAI=[' || sCai || ']');
        //        TRACE.LOG(fctName, 7, 'I', 'TS_S', 'CAI=[' || sLpc || ']');
        //        TRACE.LOG(fctName, 7, 'I', 'TS_S', 'sDefaultBridge=[' || sDefaultBridge || ']');
        //        IF sSkuCount = 0 THEN
        //            oRetMsg := 'Nothing to search a place for !';
        //        GOTO ENDE;
        //        END IF;

        //    artRT:= getArticleRT_BasedOnCaiAndLpc(sCai, sLpc);
        //        IF artRT.CAI IS NULL THEN

        //            oRetMsg:= 'No article record found !';
        //        GOTO ENDE;
        //        END IF;

        //    sSingleBridge:= 0;
        //        FOR Rec IN(
        //            SELECT MFR_ELEMENTNR

        //            FROM MFR_ELEMENT

        //            WHERE ELEMENT_TYP = 'GANTRY'
        //        )

        //        LOOP
        //            IF pack_Gantry.WorkWithSingleBridge(Rec.MFR_ELEMENTNR) = 1 THEN
        //                sSingleBridge := 1;
        //        TRACE.LOG(fctName, 7, 'I', 'TS_S', 'Working with single bridge: [' || Rec.MFR_ELEMENTNR || ']');
        //        EXIT;
        //        END IF;
        //        END LOOP;

        //        FOR Rec IN(
        //            SELECT

        //                PLACE,
        //                BRIDGE,
        //                DISTANCE,
        //                ZONE,
        //                CNTSKU,
        //                CAI,
        //                LPC

        //            FROM
        //            (
        //                SELECT

        //                    pl.PLACE,
        //                    pl.BRIDGE,
        //                    MAX(pl.DISTANCE) DISTANCE,
        //                    MAX(pl.ZONE) ZONE,
        //                    COUNT(s.SKUID) CNTSKU,
        //                    MAX(s.CAI) CAI,
        //                    MAX(s.LPC) LPC

        //                FROM PLACE pl, SKU s

        //                WHERE s.PLACE(+) = pl.PLACE

        //                AND pl.AREA = 'KOM' AND NVL(pl.LOCKED, '0') = '0'

        //                AND pl.BRIDGE = DECODE(sDefaultBridge, 0, sDefaultBridge, pl.Bridge)

        //                AND pl.PLACE = NVL(iRequestedPlace, pl.PLACE)

        //                GROUP BY pl.PLACE, pl.BRIDGE
        //            ) t

        //            WHERE NVL(CAI, sCai) = sCAI AND NVL(LPC, sLpc) = sLpc

        //             AND CNTSKU + sSkuCount <= artRT.STACK_AMOUNT

        //            ORDER BY DECODE(BRIDGE, sDefaultBridge, 1, 2), CNTSKU DESC, ZONE, DISTANCE
        //        )

        //        LOOP
        //            oPlaceFound := Rec.PLACE;
        //    retCode:= pack_global.RetCodeOK;
        //        GOTO ENDE;
        //        END LOOP;


        //<< ENDE >>
        //        TRACE.LOG(fctName, 7, 'I', 'TS_S', 'oPlaceFound=[' || oPlaceFound || ']');
        //        IF retCode = pack_global.RetCodeOK THEN

        //            TRACE.LOG(fctName, 7, 'I', 'TS_S', 'OK');
        //        ELSE

        //            TRACE.LOG(fctName, 1, 'E', 'TS_S', 'oRetMsg=[' || oRetMsg || ']');
        //        END IF;
        //        TRACE.LOG(fctName, 7, 'I', 'TS_L', 'Leave ...');
        //        RETURN retCode;
        //        END;


        _logger.LogInformation(LogMessages.LeaveMethod);
    }
}
