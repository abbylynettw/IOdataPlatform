using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;

namespace IODataPlatform.Utilities;

/// <summary>
/// 通用扩展方法集合
/// 提供数据类型转换、DataTable操作、对象映射等核心功能
/// 支持DisplayAttribute特性的自动映射，广泛用于Excel导入导出和数据绑定
/// 实现复杂类型的智能转换，包括枚举、可空类型、基础数据类型等
/// </summary>
public static class Extensions
{
	/// <summary>
	/// 将DataTable转换为指定类型的对象集合
	/// 使用DisplayAttribute特性进行属性名映射，支持多种数据类型的智能转换
	/// 包含完整的类型安全处理和异常容错机制
	/// </summary>
	/// <typeparam name="T">目标对象类型，必须具有无参构造函数</typeparam>
	/// <param name="table">源DataTable对象</param>
	/// <returns>返回转换后的对象集合</returns>
	/// <exception cref="T:System.NotImplementedException">当遇到未支持的数据类型时抛出</exception>
	public static IEnumerable<T> StringTableToIEnumerableByDiplay<T>(this DataTable table) where T : new()
	{
		var properties = (from x in typeof(T).GetProperties()
			where x.CanWrite
			select new
			{
				Property = x,
				DisplayName = (x.GetCustomAttribute<DisplayAttribute>()?.Name ?? x.Name)
			} into x
			where table.Columns.Contains(x.DisplayName)
			select x).ToList();
		return table.Rows.Cast<DataRow>().Select(delegate(DataRow row)
		{
			T val = new T();
			foreach (var item in properties)
			{
				try
				{
					string text = $"{row[item.DisplayName]}";
					if (string.IsNullOrEmpty(text))
					{
						if (item.Property.PropertyType == typeof(int) || item.Property.PropertyType == typeof(int?))
						{
							item.Property.SetValue(val, 0);
						}
						else
						{
							item.Property.SetValue(val, null);
						}
					}
					else if (item.Property.PropertyType == typeof(string))
					{
						item.Property.SetValue(val, text);
					}
					else if (item.Property.PropertyType == typeof(bool))
					{
						try
						{
							item.Property.SetValue(val, bool.Parse(text));
						}
						catch
						{
							item.Property.SetValue(val, false);
						}
					}
					else if (item.Property.PropertyType == typeof(int))
					{
						try
						{
							item.Property.SetValue(val, int.Parse(text));
						}
						catch
						{
							item.Property.SetValue(val, 0);
						}
					}
					else if (item.Property.PropertyType == typeof(int?))
					{
						try
						{
							item.Property.SetValue(val, int.Parse(text));
						}
						catch
						{
							item.Property.SetValue(val, null);
						}
					}
					else if (item.Property.PropertyType == typeof(float))
					{
						try
						{
							item.Property.SetValue(val, float.Parse(text));
						}
						catch
						{
							item.Property.SetValue(val, 0);
						}
					}
					else if (item.Property.PropertyType == typeof(float?))
					{
						try
						{
							item.Property.SetValue(val, float.Parse(text));
						}
						catch
						{
							item.Property.SetValue(val, null);
						}
					}
					else if (item.Property.PropertyType == typeof(decimal))
					{
						try
						{
							item.Property.SetValue(val, decimal.Parse(text));
						}
						catch
						{
							item.Property.SetValue(val, 0);
						}
					}
					else if (item.Property.PropertyType == typeof(decimal?))
					{
						try
						{
							item.Property.SetValue(val, decimal.Parse(text));
						}
						catch
						{
							item.Property.SetValue(val, null);
						}
					}
					else if (item.Property.PropertyType == typeof(double))
					{
						try
						{
							item.Property.SetValue(val, double.Parse(text));
						}
						catch
						{
							item.Property.SetValue(val, 0);
						}
					}
					else if (item.Property.PropertyType == typeof(double?))
					{
						try
						{
							item.Property.SetValue(val, double.Parse(text));
						}
						catch
						{
							item.Property.SetValue(val, null);
						}
					}
					else if (item.Property.PropertyType == typeof(DateTime?))
					{
						try
						{
							item.Property.SetValue(val, DateTime.Parse(text));
						}
						catch
						{
							item.Property.SetValue(val, null);
						}
					}
					else if (item.Property.PropertyType == typeof(TagType))
					{
						if (int.TryParse(text, out var result))
						{
							item.Property.SetValue(val, (TagType)result);
						}
						else
						{
							item.Property.SetValue(val, TagType.Normal);
						}
					}
					else if (item.Property.PropertyType == typeof(Xt2NetType))
					{
						PropertyInfo property = item.Property;
						object obj11 = val;
						Xt2NetType xt2NetType = ((!(text == "Net1") && text == "Net2") ? Xt2NetType.Net2 : Xt2NetType.Net1);
						property.SetValue(obj11, xt2NetType);
					}
					else if (item.Property.PropertyType == typeof(盘箱柜类别))
					{
						PropertyInfo property2 = item.Property;
						object obj12 = val;
						property2.SetValue(obj12, text switch
						{
							"盘台" => 盘箱柜类别.盘台, 
							"阀箱" => 盘箱柜类别.阀箱, 
							"机柜" => 盘箱柜类别.机柜, 
							_ => throw new NotImplementedException(), 
						});
					}
					else if (!(item.Property.PropertyType == typeof(config_cable_systemNumber)))
					{
						throw new NotImplementedException();
					}
				}
				catch (NotImplementedException)
				{
					throw new Exception("开发人员注意：需要匹配更多类型");
				}
				catch (Exception)
				{
				}
			}
			return val;
		});
	}

	/// <summary>
	/// 将对象集合转换为DataTable，使用DisplayAttribute特性作为列名
	/// 支持异步操作，自动处理可空类型和DBNull值转换
	/// 广泛用于Excel导出和数据绑定场景
	/// </summary>
	/// <typeparam name="T">源对象类型</typeparam>
	/// <param name="data">要转换的对象集合</param>
	/// <returns>返回包含转换结果的DataTable</returns>
	public static async Task<DataTable> ToTableByDisplayAttributeAsync<T>(this IEnumerable<T> data)
	{
		return await Task.Run(delegate
		{
			DataTable dataTable = new DataTable();
			List<PropertyInfo> props = typeof(T).GetProperties().ToList();
			Dictionary<string, string> mappingDic = props.ToDictionary((PropertyInfo x) => x.Name, (PropertyInfo x) => x.GetCustomAttribute<DisplayAttribute>()?.Name ?? x.Name);
			props.ForEach(delegate(PropertyInfo x)
			{
				dataTable.Columns.Add(mappingDic[x.Name], Nullable.GetUnderlyingType(x.PropertyType) ?? x.PropertyType);
			});
			data.ToList().ForEach(delegate(T x)
			{
				DataRow row = dataTable.NewRow();
				props.ForEach(delegate(PropertyInfo prop)
				{
					row[mappingDic[prop.Name]] = prop.GetValue(x) ?? DBNull.Value;
				});
				dataTable.Rows.Add(row);
			});
			return dataTable;
		});
	}

	/// <summary>
	/// 将对象集合转换为DataTable，直接使用属性名作为列名
	/// 支持异步操作，适用于不需要DisplayAttribute映射的简单转换场景
	/// </summary>
	/// <typeparam name="T">源对象类型</typeparam>
	/// <param name="data">要转换的对象集合</param>
	/// <returns>返回包含转换结果的DataTable</returns>
	public static async Task<DataTable> ToTableByPropertyNameAsync<T>(this IEnumerable<T> data)
	{
		return await Task.Run(delegate
		{
			DataTable dataTable = new DataTable();
			List<PropertyInfo> props = typeof(T).GetProperties().ToList();
			props.ForEach(delegate(PropertyInfo x)
			{
				dataTable.Columns.Add(x.Name);
			});
			data.ToList().ForEach(delegate(T x)
			{
				DataRow row = dataTable.NewRow();
				props.ForEach(delegate(PropertyInfo prop)
				{
					row[prop.Name] = prop.GetValue(x);
				});
				dataTable.Rows.Add(row);
			});
			return dataTable;
		});
	}

	/// <summary>将字符串转为指定类型，仅支持有限的类型</summary>
	public static object To(this string text, Type type)
	{
		if (type == typeof(string))
		{
			return text;
		}
		if (type == typeof(int))
		{
			return text.ToInt32();
		}
		if (type == typeof(int?))
		{
			return text.ToNullableInt32();
		}
		if (type == typeof(float))
		{
			return text.ToSingle();
		}
		if (type == typeof(float?))
		{
			return text.ToNullableSingle();
		}
		if (type == typeof(double))
		{
			return text.ToDouble();
		}
		if (type == typeof(double?))
		{
			return text.ToNullableDouble();
		}
		if (type == typeof(DateTime))
		{
			return text.ToDateTime();
		}
		if (type == typeof(DateTime?))
		{
			return text.ToNullableDateTime();
		}
		throw new Exception("不支持的类型转换");
	}

	/// <summary>将字符串转为指定类型，仅支持有限的类型</summary>
	public static T To<T>(this string text)
	{
		return (T)text.To(typeof(T));
	}

	/// <summary>将字符串转为int，无法识别的转为0</summary>
	public static int ToInt32(this string text)
	{
		try
		{
			return int.Parse(text);
		}
		catch
		{
			return 0;
		}
	}

	/// <summary>将字符串转为int?，无法识别的转为null</summary>
	public static int? ToNullableInt32(this string text)
	{
		try
		{
			return int.Parse(text);
		}
		catch
		{
			return null;
		}
	}

	/// <summary>将字符串转为float，无法识别的转为0</summary>
	public static float ToSingle(this string text)
	{
		try
		{
			return float.Parse(text);
		}
		catch
		{
			return 0f;
		}
	}

	/// <summary>将字符串转为float?，无法识别的转为null</summary>
	public static float? ToNullableSingle(this string text)
	{
		try
		{
			return float.Parse(text);
		}
		catch
		{
			return null;
		}
	}

	/// <summary>将字符串转为double，无法识别的转为0</summary>
	public static double ToDouble(this string text)
	{
		try
		{
			return double.Parse(text);
		}
		catch
		{
			return 0.0;
		}
	}

	/// <summary>将字符串转为double?，无法识别的转为null</summary>
	public static double? ToNullableDouble(this string text)
	{
		try
		{
			return double.Parse(text);
		}
		catch
		{
			return null;
		}
	}

	/// <summary>将字符串转为DateTime，无法识别的转为default</summary>
	public static DateTime ToDateTime(this string text)
	{
		try
		{
			return DateTime.Parse(text);
		}
		catch
		{
			return default(DateTime);
		}
	}

	/// <summary>将字符串转为DateTime?，无法识别的转为null</summary>
	public static DateTime? ToNullableDateTime(this string text)
	{
		try
		{
			return DateTime.Parse(text);
		}
		catch
		{
			return null;
		}
	}
}
