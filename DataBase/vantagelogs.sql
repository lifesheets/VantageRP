-- --------------------------------------------------------
-- Хост:                         127.0.0.1
-- Версия сервера:               10.4.12-MariaDB-log - mariadb.org binary distribution
-- Операционная система:         Win64
-- HeidiSQL Версия:              11.2.0.6213
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

-- Дамп структуры для таблица graylogs.adminlog
CREATE TABLE IF NOT EXISTS `adminlog` (
  `time` datetime NOT NULL,
  `admin` varchar(150) DEFAULT NULL,
  `action` varchar(300) NOT NULL,
  `player` varchar(150) DEFAULT NULL,
  KEY `time` (`time`) USING BTREE,
  KEY `admin` (`admin`) USING BTREE,
  KEY `action` (`action`) USING BTREE,
  KEY `player` (`player`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- Дамп данных таблицы graylogs.adminlog: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `adminlog` DISABLE KEYS */;
/*!40000 ALTER TABLE `adminlog` ENABLE KEYS */;

-- Дамп структуры для таблица graylogs.arrestlog
CREATE TABLE IF NOT EXISTS `arrestlog` (
  `time` datetime DEFAULT NULL,
  `player` varchar(150) DEFAULT NULL,
  `target` varchar(150) DEFAULT NULL,
  `reason` varchar(100) DEFAULT NULL,
  `stars` int(11) DEFAULT NULL,
  `pnick` varchar(150) DEFAULT NULL,
  `tnick` varchar(150) DEFAULT NULL,
  KEY `time` (`time`) USING BTREE,
  KEY `player` (`player`) USING BTREE,
  KEY `target` (`target`) USING BTREE,
  KEY `reason` (`reason`) USING BTREE,
  KEY `stars` (`stars`) USING BTREE,
  KEY `pnick` (`pnick`) USING BTREE,
  KEY `tnick` (`tnick`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- Дамп данных таблицы graylogs.arrestlog: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `arrestlog` DISABLE KEYS */;
/*!40000 ALTER TABLE `arrestlog` ENABLE KEYS */;

-- Дамп структуры для таблица graylogs.banlog
CREATE TABLE IF NOT EXISTS `banlog` (
  `time` datetime DEFAULT NULL,
  `admin` varchar(150) DEFAULT NULL,
  `player` varchar(150) DEFAULT NULL,
  `until` datetime DEFAULT NULL,
  `reason` varchar(100) DEFAULT NULL,
  `ishard` tinyint(4) DEFAULT NULL,
  KEY `time` (`time`) USING BTREE,
  KEY `admin` (`admin`) USING BTREE,
  KEY `player` (`player`) USING BTREE,
  KEY `until` (`until`) USING BTREE,
  KEY `reason` (`reason`) USING BTREE,
  KEY `ishard` (`ishard`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- Дамп данных таблицы graylogs.banlog: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `banlog` DISABLE KEYS */;
/*!40000 ALTER TABLE `banlog` ENABLE KEYS */;

-- Дамп структуры для таблица graylogs.casinobetlog
CREATE TABLE IF NOT EXISTS `casinobetlog` (
  `time` datetime DEFAULT NULL,
  `name` varchar(50) DEFAULT NULL,
  `uuid` int(11) DEFAULT -1,
  `red` text DEFAULT NULL,
  `zero` text DEFAULT NULL,
  `black` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- Дамп данных таблицы graylogs.casinobetlog: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `casinobetlog` DISABLE KEYS */;
/*!40000 ALTER TABLE `casinobetlog` ENABLE KEYS */;

-- Дамп структуры для таблица graylogs.casinoendlog
CREATE TABLE IF NOT EXISTS `casinoendlog` (
  `time` datetime DEFAULT NULL,
  `name` varchar(50) DEFAULT NULL,
  `uuid` int(11) DEFAULT -1,
  `state` text DEFAULT NULL,
  `type` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- Дамп данных таблицы graylogs.casinoendlog: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `casinoendlog` DISABLE KEYS */;
/*!40000 ALTER TABLE `casinoendlog` ENABLE KEYS */;

-- Дамп структуры для таблица graylogs.casinowinloselog
CREATE TABLE IF NOT EXISTS `casinowinloselog` (
  `time` datetime DEFAULT NULL,
  `name` varchar(50) DEFAULT NULL,
  `uuid` int(11) DEFAULT -1,
  `wonbet` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- Дамп данных таблицы graylogs.casinowinloselog: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `casinowinloselog` DISABLE KEYS */;
/*!40000 ALTER TABLE `casinowinloselog` ENABLE KEYS */;

-- Дамп структуры для таблица graylogs.connlog
CREATE TABLE IF NOT EXISTS `connlog` (
  `in` datetime DEFAULT NULL,
  `out` datetime DEFAULT NULL,
  `uuid` int(11) DEFAULT -1,
  `ip` varchar(50) DEFAULT NULL,
  `amount` varchar(256) DEFAULT NULL,
  `sclub` varchar(50) DEFAULT NULL,
  `hwid` varchar(256) DEFAULT NULL,
  KEY `in` (`in`) USING BTREE,
  KEY `out` (`out`) USING BTREE,
  KEY `uuid` (`uuid`) USING BTREE,
  KEY `ip` (`ip`) USING BTREE,
  KEY `sclub` (`sclub`) USING BTREE,
  KEY `hwid` (`hwid`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- Дамп данных таблицы graylogs.connlog: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `connlog` DISABLE KEYS */;
/*!40000 ALTER TABLE `connlog` ENABLE KEYS */;

-- Дамп структуры для таблица graylogs.deletelog
CREATE TABLE IF NOT EXISTS `deletelog` (
  `time` datetime DEFAULT NULL,
  `uuid` int(11) DEFAULT NULL,
  `name` varchar(50) DEFAULT NULL,
  `account` varchar(50) DEFAULT NULL,
  KEY `time` (`time`) USING BTREE,
  KEY `uuid` (`uuid`) USING BTREE,
  KEY `name` (`name`) USING BTREE,
  KEY `account` (`account`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 ROW_FORMAT=DYNAMIC;

-- Дамп данных таблицы graylogs.deletelog: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `deletelog` DISABLE KEYS */;
/*!40000 ALTER TABLE `deletelog` ENABLE KEYS */;

-- Дамп структуры для таблица graylogs.eventslog
CREATE TABLE IF NOT EXISTS `eventslog` (
  `AdminStarted` text DEFAULT NULL,
  `EventName` text DEFAULT NULL,
  `MembersLimit` text DEFAULT NULL,
  `Started` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- Дамп данных таблицы graylogs.eventslog: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `eventslog` DISABLE KEYS */;
/*!40000 ALTER TABLE `eventslog` ENABLE KEYS */;

-- Дамп структуры для таблица graylogs.idlog
CREATE TABLE IF NOT EXISTS `idlog` (
  `in` datetime DEFAULT NULL,
  `out` datetime DEFAULT NULL,
  `from` varchar(150) DEFAULT NULL,
  `uuid` int(11) DEFAULT -1,
  `id` int(11) DEFAULT -1,
  `name` varchar(50) DEFAULT NULL,
  KEY `in` (`in`) USING BTREE,
  KEY `out` (`out`) USING BTREE,
  KEY `from` (`from`) USING BTREE,
  KEY `uuid` (`uuid`) USING BTREE,
  KEY `id` (`id`) USING BTREE,
  KEY `name` (`name`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- Дамп данных таблицы graylogs.idlog: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `idlog` DISABLE KEYS */;
/*!40000 ALTER TABLE `idlog` ENABLE KEYS */;

-- Дамп структуры для таблица graylogs.itemslog
CREATE TABLE IF NOT EXISTS `itemslog` (
  `time` datetime DEFAULT NULL,
  `from` varchar(150) DEFAULT NULL,
  `to` varchar(150) DEFAULT NULL,
  `type` int(11) DEFAULT NULL,
  `amount` int(11) DEFAULT NULL,
  `data` varchar(100) DEFAULT NULL,
  KEY `time` (`time`) USING BTREE,
  KEY `from` (`from`) USING BTREE,
  KEY `to` (`to`) USING BTREE,
  KEY `type` (`type`) USING BTREE,
  KEY `amount` (`amount`) USING BTREE,
  KEY `data` (`data`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- Дамп данных таблицы graylogs.itemslog: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `itemslog` DISABLE KEYS */;
/*!40000 ALTER TABLE `itemslog` ENABLE KEYS */;

-- Дамп структуры для таблица graylogs.moneylog
CREATE TABLE IF NOT EXISTS `moneylog` (
  `time` datetime DEFAULT NULL,
  `from` varchar(150) DEFAULT NULL,
  `to` varchar(150) DEFAULT NULL,
  `amount` bigint(20) DEFAULT NULL,
  `comment` varchar(80) DEFAULT NULL,
  KEY `time` (`time`) USING BTREE,
  KEY `from` (`from`) USING BTREE,
  KEY `to` (`to`) USING BTREE,
  KEY `amount` (`amount`) USING BTREE,
  KEY `comment` (`comment`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- Дамп данных таблицы graylogs.moneylog: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `moneylog` DISABLE KEYS */;
/*!40000 ALTER TABLE `moneylog` ENABLE KEYS */;

-- Дамп структуры для таблица graylogs.namelog
CREATE TABLE IF NOT EXISTS `namelog` (
  `time` datetime DEFAULT NULL,
  `uuid` int(11) DEFAULT NULL,
  `old` varchar(50) DEFAULT NULL,
  `new` varchar(50) DEFAULT NULL,
  KEY `time` (`time`) USING BTREE,
  KEY `uuid` (`uuid`) USING BTREE,
  KEY `old` (`old`) USING BTREE,
  KEY `new` (`new`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- Дамп данных таблицы graylogs.namelog: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `namelog` DISABLE KEYS */;
/*!40000 ALTER TABLE `namelog` ENABLE KEYS */;

-- Дамп структуры для таблица graylogs.stocklog
CREATE TABLE IF NOT EXISTS `stocklog` (
  `time` datetime DEFAULT NULL,
  `frac` tinyint(4) DEFAULT NULL,
  `uuid` varchar(150) DEFAULT NULL,
  `type` varchar(100) DEFAULT NULL,
  `amount` int(11) DEFAULT NULL,
  `in` tinyint(4) DEFAULT NULL,
  KEY `time` (`time`) USING BTREE,
  KEY `frac` (`frac`) USING BTREE,
  KEY `uuid` (`uuid`) USING BTREE,
  KEY `type` (`type`) USING BTREE,
  KEY `amount` (`amount`) USING BTREE,
  KEY `in` (`in`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- Дамп данных таблицы graylogs.stocklog: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `stocklog` DISABLE KEYS */;
/*!40000 ALTER TABLE `stocklog` ENABLE KEYS */;

-- Дамп структуры для таблица graylogs.ticketlog
CREATE TABLE IF NOT EXISTS `ticketlog` (
  `time` datetime DEFAULT NULL,
  `player` varchar(150) DEFAULT NULL,
  `target` varchar(150) DEFAULT NULL,
  `sum` int(11) DEFAULT NULL,
  `reason` varchar(100) DEFAULT NULL,
  `pnick` varchar(150) DEFAULT NULL,
  `tnick` varchar(150) DEFAULT NULL,
  KEY `time` (`time`) USING BTREE,
  KEY `player` (`player`) USING BTREE,
  KEY `target` (`target`) USING BTREE,
  KEY `sum` (`sum`) USING BTREE,
  KEY `reason` (`reason`) USING BTREE,
  KEY `pnick` (`pnick`) USING BTREE,
  KEY `tnick` (`tnick`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- Дамп данных таблицы graylogs.ticketlog: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `ticketlog` DISABLE KEYS */;
/*!40000 ALTER TABLE `ticketlog` ENABLE KEYS */;

-- Дамп структуры для таблица graylogs.votelog
CREATE TABLE IF NOT EXISTS `votelog` (
  `election` text DEFAULT NULL,
  `login` text DEFAULT NULL,
  `votefor` text DEFAULT NULL,
  `time` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

-- Дамп данных таблицы graylogs.votelog: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `votelog` DISABLE KEYS */;
/*!40000 ALTER TABLE `votelog` ENABLE KEYS */;

/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
