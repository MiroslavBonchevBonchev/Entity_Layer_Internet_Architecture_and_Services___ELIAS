CREATE DEFINER=`root`@`%` PROCEDURE `Insert_temperature`( IN _date DATE, IN _temp_c INT )
BEGIN
	INSERT INTO new_table VALUES( NULL, _date, _temp_c );
END