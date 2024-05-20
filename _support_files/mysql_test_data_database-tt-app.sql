-- --------------------------------------------------------
-- Host:                         127.0.0.1
-- Server version:               8.4.0 - MySQL Community Server - GPL
-- Server OS:                    Linux
-- HeidiSQL Version:             12.6.0.6765
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;


-- Dumping database structure for ELIAS_QS_WorkDB
DROP DATABASE IF EXISTS `ELIAS_QS_WorkDB`;
CREATE DATABASE IF NOT EXISTS `ELIAS_QS_WorkDB` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `ELIAS_QS_WorkDB`;

-- Dumping structure for procedure ELIAS_QS_WorkDB.Get_all_temperatures
DROP PROCEDURE IF EXISTS `Get_all_temperatures`;
DELIMITER //
CREATE PROCEDURE `Get_all_temperatures`()
BEGIN
	SELECT date, temp_c FROM new_table;
END//
DELIMITER ;

-- Dumping structure for procedure ELIAS_QS_WorkDB.Get_date_temperature
DROP PROCEDURE IF EXISTS `Get_date_temperature`;
DELIMITER //
CREATE PROCEDURE `Get_date_temperature`( IN _date DATE )
BEGIN
	select date, temp_c from new_table where date = _date;
END//
DELIMITER ;

-- Dumping structure for procedure ELIAS_QS_WorkDB.Insert_temperature
DROP PROCEDURE IF EXISTS `Insert_temperature`;
DELIMITER //
CREATE PROCEDURE `Insert_temperature`( IN _date DATE, IN _temp_c INT )
BEGIN
	INSERT INTO new_table VALUES( NULL, _date, _temp_c );
END//
DELIMITER ;

-- Dumping structure for table ELIAS_QS_WorkDB.new_table
DROP TABLE IF EXISTS `new_table`;
CREATE TABLE IF NOT EXISTS `new_table` (
  `Id` int unsigned NOT NULL AUTO_INCREMENT,
  `date` date NOT NULL,
  `temp_c` int NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Dumping data for table ELIAS_QS_WorkDB.new_table: ~0 rows (approximately)
INSERT INTO `new_table` (`Id`, `date`, `temp_c`) VALUES
	(9, '2024-04-17', 17);
INSERT INTO `new_table` (`Id`, `date`, `temp_c`) VALUES
	(1, '2024-04-18', 18);
INSERT INTO `new_table` (`Id`, `date`, `temp_c`) VALUES
	(2, '2024-04-19', 19);
INSERT INTO `new_table` (`Id`, `date`, `temp_c`) VALUES
	(3, '2024-04-20', 20);
INSERT INTO `new_table` (`Id`, `date`, `temp_c`) VALUES
	(4, '2024-04-21', 21);
INSERT INTO `new_table` (`Id`, `date`, `temp_c`) VALUES
	(5, '2024-04-22', 22);
INSERT INTO `new_table` (`Id`, `date`, `temp_c`) VALUES
	(6, '2024-04-23', 23);
INSERT INTO `new_table` (`Id`, `date`, `temp_c`) VALUES
	(7, '2024-04-24', 24);
INSERT INTO `new_table` (`Id`, `date`, `temp_c`) VALUES
	(8, '2024-04-25', 25);

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
