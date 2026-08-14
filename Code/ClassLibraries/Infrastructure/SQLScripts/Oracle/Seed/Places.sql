DELETE Places WHERE Area = 'GANTRY';

DECLARE
    i NUMBER := 1;
    j NUMBER;
BEGIN

    WHILE i <= 22 LOOP
        j := 1;
        WHILE j <= 14 LOOP
            INSERT INTO Places (
                Name,
                Area,
                G,
                XCell,
                YCell,
                XPos,
                YPos,
                Zone,
                Distance
            )
            VALUES (
                'GANTRY-' ||
                LPAD(i, 2, '0') || '.' ||
                LPAD(j, 2, '0'),
                'GANTRY',
                '1',
                i,
                j,
                0,
                0,
                0,
                0
            );
            j := j + 1;
        END LOOP;
        i := i + 1;
    END LOOP;


    ---------------------------------------------------------
    -- Remove odd/even places from the honeycomb
    ---------------------------------------------------------

    DELETE Places
    WHERE Area = 'GANTRY'
    AND MOD(TO_NUMBER(YCell), 2) = 0
    AND TO_NUMBER(XCell) IN (2, 4, 6, 8, 10, 13, 15, 17, 19, 21);

    DELETE Places
    WHERE Area = 'GANTRY'
    AND MOD(TO_NUMBER(YCell), 2) = 1
    AND TO_NUMBER(XCell) IN (1, 3, 5, 7, 9, 11, 12, 14, 16, 18, 20, 22);


    ---------------------------------------------------------
    -- Remove places used by infeed conveyors
    ---------------------------------------------------------

    DELETE Places
    WHERE Area = 'GANTRY'
    AND TO_NUMBER(XCell) IN (7, 8, 15, 16)
    AND TO_NUMBER(YCell) < 4;


    ---------------------------------------------------------
    -- Remove places used by outfeed conveyors
    ---------------------------------------------------------

    DELETE Places
    WHERE Area = 'GANTRY'
    AND TO_NUMBER(XCell) IN (4, 5, 6, 13, 14, 15)
    AND TO_NUMBER(YCell) > 9;


    ---------------------------------------------------------
    -- Place position in millimeters
    ---------------------------------------------------------

    UPDATE Places SET
        XPos =
            CASE TO_NUMBER(XCell)
                WHEN 1  THEN 0
                WHEN 2  THEN 823
                WHEN 3  THEN 1946
                WHEN 4  THEN 2769
                WHEN 5  THEN 3592
                WHEN 6  THEN 4415
                WHEN 7  THEN 5450
                WHEN 8  THEN 6273
                WHEN 9  THEN 7096
                WHEN 10 THEN 7919
                WHEN 11 THEN 8742
                WHEN 12 THEN 9942
                WHEN 13 THEN 10915
                WHEN 14 THEN 11738
                WHEN 15 THEN 12561
                WHEN 16 THEN 13384
                WHEN 17 THEN 14207
                WHEN 18 THEN 15030
                WHEN 19 THEN 15853
                WHEN 20 THEN 16676
                WHEN 21 THEN 17799
                WHEN 22 THEN 18622
            END,

        YPos =
            CASE TO_NUMBER(YCell)
                WHEN 1  THEN -475
                WHEN 2  THEN 0
                WHEN 3  THEN 475
                WHEN 4  THEN 950
                WHEN 5  THEN 1425
                WHEN 6  THEN 1900
                WHEN 7  THEN 2375
                WHEN 8  THEN 2850
                WHEN 9  THEN 3325
                WHEN 10 THEN 3800
                WHEN 11 THEN 4275
                WHEN 12 THEN 4750
                WHEN 13 THEN 5225
                WHEN 14 THEN 5700
            END
    WHERE Area = 'GANTRY';


    ---------------------------------------------------------
    -- Gantry bridge assignment
    ---------------------------------------------------------

    UPDATE Places SET 
        Bridge = '3100'
    WHERE Area = 'GANTRY'
    AND TO_NUMBER(XCell) < 12;

    UPDATE Places SET 
        Bridge = '3200'
    WHERE Area = 'GANTRY'
    AND TO_NUMBER(XCell) >= 12;


    ---------------------------------------------------------
    -- Zones
    ---------------------------------------------------------

    UPDATE Places SET 
        Zone = 20
    WHERE Area = 'GANTRY'
    AND TO_NUMBER(XCell) IN (3, 4, 6, 7, 9, 10, 13, 14, 16, 17, 19, 20);

    UPDATE Places SET 
        Zone = 40
    WHERE Area = 'GANTRY' 
    AND TO_NUMBER(XCell) IN (5, 8, 15, 18);

    UPDATE Places SET 
        Zone = 40
    WHERE Area = 'GANTRY'
    AND TO_NUMBER(XCell) IN (6, 7, 8, 9, 17, 18, 19, 20)
    AND TO_NUMBER(YCell) <= 4;

    UPDATE Places SET 
        Zone = 40
    WHERE Area = 'GANTRY'
    AND TO_NUMBER(XCell) IN (3, 7, 16)
    AND TO_NUMBER(YCell) > 9;

    UPDATE Places SET 
        Zone = 60
    WHERE Area = 'GANTRY'
    AND TO_NUMBER(XCell) IN (11, 12);

    UPDATE Places SET 
        Zone = 60
    WHERE Area = 'GANTRY'
    AND TO_NUMBER(YCell) IN (8, 9);

    UPDATE Places SET 
        Zone = 80
    WHERE Area = 'GANTRY'
    AND TO_NUMBER(XCell) IN (1, 2, 21, 22);


    ---------------------------------------------------------
    -- Travel distance
    ---------------------------------------------------------

    UPDATE Places
    SET Distance =
            POWER(TO_NUMBER(XCell) - 7, 2) +
            POWER(TO_NUMBER(YCell) - 4, 2)
    WHERE Area = 'GANTRY'
    AND TO_NUMBER(XCell) < 12;

    UPDATE Places SET 
        Distance =
            POWER(TO_NUMBER(XCell) - 18, 2) +
            POWER(TO_NUMBER(YCell) - 4, 2)
    WHERE Area = 'GANTRY'
    AND TO_NUMBER(XCell) >= 12;

END;
