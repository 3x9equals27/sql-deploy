DECLARE @v_script_name VARCHAR = '00000000_0001_provision_table.sql';
BEGIN
--
IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_CATALOG = DB_NAME() AND TABLE_SCHEMA = SCHEMA_NAME() 
				   AND TABLE_NAME = 'provision')
				 ) BEGIN
	-- DROP TABLE provision;
	CREATE TABLE provision(
	  script_name NVARCHAR(200) PRIMARY KEY
	, provisioned DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
	);
	
	INSERT INTO provision(script_name) VALUES(@v_script_name);
END
--    
END

