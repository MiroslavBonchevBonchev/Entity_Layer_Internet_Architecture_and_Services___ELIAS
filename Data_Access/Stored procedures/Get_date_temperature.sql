CREATE DEFINER=`root`@`%` PROCEDURE `Get_date_temperature`( IN _date DATE )
BEGIN
	select date, temp_c from new_table where date = _date;
END