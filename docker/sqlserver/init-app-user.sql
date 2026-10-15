-- 减少无关的行数输出
SET NOCOUNT ON;
GO

-- 处理实例级登录账号
USE [master];
GO

-- 数据库必须先由迁移服务创建
IF DB_ID(N'EITSTMSDb') IS NULL
BEGIN
    ;THROW 50001, N'EITSTMSDb 不存在，请先检查数据库迁移。', 1;
END;
GO

-- 初始化服务会先对密码中的单引号进行 SQL 转义
-- 先使用 nvarchar(max)，避免过长密码被静默截断
DECLARE @appPassword nvarchar(max) = N'$(APP_DB_PASSWORD_SQL)';

-- SQL Server 登录密码最多使用 128 个 UTF-16 代码单元
-- 使用 DATALENGTH，避免 LEN 忽略末尾空格
IF DATALENGTH(@appPassword) = 0
   OR DATALENGTH(@appPassword) > 256
BEGIN
    ;THROW 50002, N'应用数据库密码为空或超过允许长度。', 1;
END;

-- 同名主体必须是 SQL 登录账号，不能是其他类型的主体
IF EXISTS (
    SELECT 1
    FROM sys.server_principals
    WHERE name = N'eitstms_app'
      AND type <> 'S'
)
BEGIN
    ;THROW 50003, N'eitstms_app 已存在，但不是 SQL 登录账号。', 1;
END;

-- 只创建不存在的账号，不自动重置已有账号的密码
IF NOT EXISTS (
    SELECT 1
    FROM sys.server_principals
    WHERE name = N'eitstms_app'
)
BEGIN
    -- CREATE LOGIN 的密码位置需要字符串字面量
    -- QUOTENAME 用单引号包裹密码，并转义其中的单引号
    DECLARE @createLoginSql nvarchar(max) =
        N'CREATE LOGIN [eitstms_app] WITH PASSWORD = N'
        + QUOTENAME(@appPassword, '''')
        + N', CHECK_POLICY = ON;';

    EXEC sys.sp_executesql @createLoginSql;
END;

-- 应用账号不应拥有实例最高权限
IF IS_SRVROLEMEMBER(N'sysadmin', N'eitstms_app') = 1
BEGIN
    ;THROW 50004, N'eitstms_app 不应属于 sysadmin，请人工检查权限。', 1;
END;
GO

-- 处理项目数据库内的用户及权限
USE [EITSTMSDb];
GO

-- 已有同名用户必须正确关联到目标登录账号
-- 不自动修改已有用户的身份映射
IF EXISTS (
    SELECT 1
    FROM sys.database_principals
    WHERE name = N'eitstms_app'
      AND (
          type <> 'S'
          OR sid <> SUSER_SID(N'eitstms_app')
      )
)
BEGIN
    ;THROW 50005, N'已有 eitstms_app 用户的类型或登录映射不正确。', 1;
END;

-- 用户不存在时，创建并关联登录账号
IF NOT EXISTS (
    SELECT 1
    FROM sys.database_principals
    WHERE name = N'eitstms_app'
)
BEGIN
    CREATE USER [eitstms_app]
        FOR LOGIN [eitstms_app];
END;

-- 授予读取用户表和视图的权限
IF IS_ROLEMEMBER(N'db_datareader', N'eitstms_app') = 0
BEGIN
    ALTER ROLE [db_datareader] ADD MEMBER [eitstms_app];
END;

-- 授予新增、修改、删除用户表数据的权限
IF IS_ROLEMEMBER(N'db_datawriter', N'eitstms_app') = 0
BEGIN
    ALTER ROLE [db_datawriter] ADD MEMBER [eitstms_app];
END;
GO