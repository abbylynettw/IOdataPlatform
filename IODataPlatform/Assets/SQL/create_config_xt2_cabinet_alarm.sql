-- XT2控制系统机柜报警清单配置表
-- 存储从Excel导入的机柜报警信息，用于后续导出机柜报警表时关联查询

-- 如果表已存在则删除
IF OBJECT_ID('config_xt2_cabinet_alarm', 'U') IS NOT NULL
    DROP TABLE config_xt2_cabinet_alarm;
GO

-- 创建表
CREATE TABLE config_xt2_cabinet_alarm (
    Id INT PRIMARY KEY IDENTITY(1,1),       -- 主键，自增
    SerialNumber INT DEFAULT 0,             -- 序号
    CabinetName NVARCHAR(50) NOT NULL,      -- 机柜名称
    CabinetType NVARCHAR(50),               -- 机柜类型
    Room NVARCHAR(50),                      -- 房间号
    Temperature INT DEFAULT 0,              -- 温度（AI点）数量
    ComprehensiveAlarm INT DEFAULT 0,       -- 综合报警（DI点）数量
    PowerAFault INT DEFAULT 0,              -- 机柜电源A故障数量
    PowerBAlarmFault INT DEFAULT 0,         -- 机柜电源报警B故障数量
    DoorOpen INT DEFAULT 0,                 -- 机柜门开数量
    HighTemperatureAlarm INT DEFAULT 0,     -- 机柜温度高报警数量
    FanFault INT DEFAULT 0,                 -- 风扇故障数量
    NetworkFault INT DEFAULT 0,             -- 网络故障数量
    RTUBoardPosition NVARCHAR(50),          -- RTU板卡位置（格式：1-0-04）
    RTUCabinetNumber NVARCHAR(50)           -- RTU板卡所在机柜号
);
GO

-- 创建索引
CREATE INDEX idx_xt2_cabinet_alarm_cabinet ON config_xt2_cabinet_alarm(CabinetName);
GO
