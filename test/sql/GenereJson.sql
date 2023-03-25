--===============================
--Create Load Center Test to Json
--===============================

SELECT TOP(200)
  'LC_T' + CAST (ROW_NUMBER() OVER(ORDER BY a.object_id ASC) AS VARCHAR) as code,
  'Load Center Test ' + CAST (ROW_NUMBER() OVER(ORDER BY a.object_id ASC) AS VARCHAR) as name ,
  'TL1'   as tensionLevelCode , 
    '01AAP-85' as nodeCode
FROM sys.columns AS a CROSS JOIN sys.columns AS b
 FOR JSON PATH

 --Create Load Center Test to Json
SELECT TOP(200)
  '"LC_T' + CAST (ROW_NUMBER() OVER(ORDER BY a.object_id ASC) AS VARCHAR) + '",' as code
FROM sys.columns AS a CROSS JOIN sys.columns AS b

--==========================
--Create Load Center For SQL
--==========================

-- Create [SystemElements]
INSERT INTO [Master].[SystemElements]
SELECT TOP(200)
  'LC_T' + CAST (ROW_NUMBER() OVER(ORDER BY a.object_id ASC) AS VARCHAR) ,
  'ECenCarg',
  'Load Center Test ' + CAST (ROW_NUMBER() OVER(ORDER BY a.object_id ASC) AS VARCHAR)  
FROM sys.columns AS a CROSS JOIN sys.columns AS b


-- Create [LoadCenters]
INSERT INTO [Master].[LoadCenters]
SELECT TOP(200)
  'LC_T' + CAST (ROW_NUMBER() OVER(ORDER BY a.object_id ASC) AS VARCHAR) ,
  'Load Center Test ' + CAST (ROW_NUMBER() OVER(ORDER BY a.object_id ASC) AS VARCHAR)  ,
  'Compay_T1',
  '01AAP-85',
   'CARG',	
   null,
   'ZONA2',
   null
FROM sys.columns AS a CROSS JOIN sys.columns AS b


