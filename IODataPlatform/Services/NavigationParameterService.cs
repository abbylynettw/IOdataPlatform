﻿namespace IODataPlatform.Services
{
    /// <summary>
    /// 导航参数服务类
    /// 提供在页面间传递参数的功能，支持任意类型的参数存储和检索
    /// 使用字典结构存储键值对，支持泛型参数的安全访问
    /// </summary>
    public class NavigationParameterService
    {
        /// <summary>存储导航参数的字典，键为参数名，值为参数对象</summary>
        private readonly Dictionary<string, object> parameters = new Dictionary<string, object>();

        /// <summary>
        /// 设置导航参数
        /// 如果参数已存在则更新其值，否则添加新参数
        /// </summary>
        /// <param name="key">参数键名</param>
        /// <param name="value">参数值，可以是任意类型</param>
        public void SetParameter(string key, object value)
        {
            if (parameters.ContainsKey(key))
            {
                parameters[key] = value;
            }
            else
            {
                parameters.Add(key, value);
            }
        }

        /// <summary>
        /// 获取指定类型的导航参数
        /// 如果参数不存在或类型不匹配，则返回类型的默认值
        /// </summary>
        /// <typeparam name="T">期望的参数类型</typeparam>
        /// <param name="key">参数键名</param>
        /// <returns>指定类型的参数值，或类型的默认值</returns>
        public T GetParameter<T>(string key)
        {
            if (parameters.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }

            return default;
        }

        /// <summary>
        /// 清除所有已存储的导航参数
        /// 通常在页面切换后调用以防止内存泄漏
        /// </summary>
        public void ClearParameters()
        {
            parameters.Clear();
        }
    }

}
