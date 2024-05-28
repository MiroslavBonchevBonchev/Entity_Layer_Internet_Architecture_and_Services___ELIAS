CREATE DEFINER=`root`@`%` PROCEDURE `Get_all_temperatures`()
BEGIN
	SELECT date, temp_c FROM new_table;
END