﻿namespace IODataPlatform.Models.Configs
{
    /// <summary>
    /// 其他平台集成配置类
    /// 用于配置与外部系统（如禅道项目管理系统）的集成信息
    /// </summary>
    public class OtherPlatFormConfig
    {
        /// <summary>禅道项目管理系统的URL地址</summary>
        public string ZentaoUrl { get; set; } = string.Empty;
    }
}
