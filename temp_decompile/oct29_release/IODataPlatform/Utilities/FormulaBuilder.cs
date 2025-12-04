using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using NCalc;

namespace IODataPlatform.Utilities;

/// <summary>
/// 公式构建器类
/// 提供基于条件的动态公式计算功能，支持多种控制系统的字段值计算
/// 使用表达式树和Lambda表达式实现高性能的条件匹配和值计算
/// 支持复杂的字符串操作、数值计算和逻辑判断，广泛用于IO数据的智能填充
/// </summary>
public class FormulaBuilder
{
	/// <summary>
	/// 通过表达式引擎查找匹配的公式并返回计算结果
	/// 使用表达式树构建复杂的条件判断逻辑，支持多种数据类型和操作符
	/// 包含完整的异常处理机制，确保公式计算的稳定性和可靠性
	/// </summary>
	/// <typeparam name="T">目标对象类型，必须是类类型</typeparam>
	/// <param name="targetTypeInstance">目标对象实例，用于条件匹配</param>
	/// <param name="controlSystem">控制系统类型</param>
	/// <param name="fieldDisplayName">字段的显示名称</param>
	/// <param name="formulars">公式定义列表</param>
	/// <param name="formularIndexes">公式索引配置列表</param>
	/// <param name="FormulaIndexConditions">公式条件配置列表</param>
	/// <returns>返回匹配公式的计算结果，失败时返回"Err"</returns>
	/// <exception cref="T:System.Exception">当公式计算出现错误时抛出异常</exception>
	public string FindMatchingFormulaReturnValueAsyncByExpression<T>(T targetTypeInstance, ControlSystem controlSystem, string fieldDisplayName, List<formular> formulars, List<formular_Index> formularIndexes, List<formular_index_condition> FormulaIndexConditions) where T : class
	{
		try
		{
			Type typeFromHandle = typeof(T);
			int? fieldIdToGet = formulars.Where((formular q) => q.TargetType == controlSystem.ToString() && q.FieldName == fieldDisplayName).FirstOrDefault()?.Id;
			if (!fieldIdToGet.HasValue)
			{
				return "Err";
			}
			List<formular_Index> list = (from x in formularIndexes
				where x.FormulaId == fieldIdToGet
				orderby x.Index
				select x).ToList();
			foreach (formular_Index value in list)
			{
				ParameterExpression parameterExpression = System.Linq.Expressions.Expression.Parameter(typeof(T), "target");
				System.Linq.Expressions.Expression expression = null;
				List<formular_index_condition> list2 = (from c in FormulaIndexConditions
					where c.IndexId == value.Id
					orderby c.Index
					select c).ToList();
				foreach (formular_index_condition condition in list2)
				{
					PropertyInfo propertyInfo = typeof(T).GetProperties().FirstOrDefault((PropertyInfo a) => a.Name == condition.PropertyName);
					if (propertyInfo == null)
					{
						continue;
					}
					object value2 = propertyInfo.GetValue(targetTypeInstance) ?? "";
					ConstantExpression constantExpression = System.Linq.Expressions.Expression.Constant(value2);
					ConstantExpression constantExpression2 = System.Linq.Expressions.Expression.Constant(Convert.ChangeType(condition.FieldValue, propertyInfo.PropertyType), propertyInfo.PropertyType);
					System.Linq.Expressions.Expression expression2;
					switch (condition.FieldOperator)
					{
					case FieldOperator.等于:
						expression2 = System.Linq.Expressions.Expression.Equal(constantExpression, constantExpression2);
						break;
					case FieldOperator.不等于:
						expression2 = System.Linq.Expressions.Expression.NotEqual(constantExpression, constantExpression2);
						break;
					case FieldOperator.包含:
						if (propertyInfo.PropertyType == typeof(string))
						{
							expression2 = System.Linq.Expressions.Expression.Call(constantExpression, "Contains", null, constantExpression2);
							break;
						}
						goto default;
					case FieldOperator.不包含:
						if (propertyInfo.PropertyType == typeof(string))
						{
							expression2 = System.Linq.Expressions.Expression.Not(System.Linq.Expressions.Expression.Call(constantExpression, "Contains", null, constantExpression2));
							break;
						}
						goto default;
					case FieldOperator.起始于:
						if (propertyInfo.PropertyType == typeof(string))
						{
							expression2 = System.Linq.Expressions.Expression.Call(constantExpression, "StartsWith", null, constantExpression2);
							break;
						}
						goto default;
					case FieldOperator.终止于:
						if (propertyInfo.PropertyType == typeof(string))
						{
							expression2 = System.Linq.Expressions.Expression.Call(constantExpression, "EndsWith", null, constantExpression2);
							break;
						}
						goto default;
					default:
						throw new NotSupportedException($"不支持的运算符: {condition.FieldOperator}");
					}
					System.Linq.Expressions.Expression expression3 = expression2;
					System.Linq.Expressions.Expression expression4 = ((expression != null) ? (condition.ConditionOperator switch
					{
						ConditionOperator.并且 => System.Linq.Expressions.Expression.AndAlso(expression, expression3), 
						ConditionOperator.或 => System.Linq.Expressions.Expression.OrElse(expression, expression3), 
						ConditionOperator.无 => expression, 
						_ => throw new NotSupportedException($"不支持的连接运算符: {condition.ConditionOperator}"), 
					}) : expression3);
					expression = expression4;
				}
				if (expression != null)
				{
					Func<T, bool> func = System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(expression, new ParameterExpression[1] { parameterExpression }).Compile();
					if (func(targetTypeInstance))
					{
						return value.ReturnValue;
					}
				}
			}
			return "Err";
		}
		catch (Exception)
		{
			throw new Exception("获取" + fieldDisplayName + "错误");
		}
	}

	public string? FindMatchingFormulaReturnValueAsyncByExpressionNew<T>(T targetTypeInstance, ControlSystem controlSystem, string fieldDisplayName, List<formular> formulars, List<formular_Index> formularIndexes, List<formular_index_condition> FormulaIndexConditions) where T : class
	{
		try
		{
			Type typeFromHandle = typeof(T);
			int? fieldIdToGet = formulars.FirstOrDefault((formular q) => q.TargetType == controlSystem.ToString() && q.FieldName == fieldDisplayName)?.Id;
			if (!fieldIdToGet.HasValue)
			{
				return "Err";
			}
			List<formular_Index> list = (from x in formularIndexes
				where x.FormulaId == fieldIdToGet
				orderby x.Index
				select x).ToList();
			foreach (formular_Index item in list)
			{
				if (IsConditionMatched(item.Id, targetTypeInstance, FormulaIndexConditions))
				{
					if (item.ReturnValue.Contains("CH"))
					{
						int channel = (int)typeFromHandle.GetProperty("Channel")?.GetValue(targetTypeInstance);
						string expression = item.ReturnValue.Replace("CH", channel.ToString());
						string text = CalculateChExpression(expression, channel);
						return text.ToString();
					}
					return item.ReturnValue;
				}
			}
			return "Err";
		}
		catch (Exception)
		{
			throw new Exception("获取" + fieldDisplayName + "错误");
		}
	}

	public string CalculateChExpression(string expression, int channel)
	{
		if (expression.Contains("CH"))
		{
			expression = expression.Replace("CH", channel.ToString());
		}
		if (expression.Contains("+") || expression.Contains("*") || expression.Contains("-") || expression.Contains("\\"))
		{
			return CalculateExpression(expression).ToString();
		}
		return expression;
	}

	private object CalculateExpression(string expressionString)
	{
		NCalc.Expression expression = new NCalc.Expression(expressionString);
		return expression.Evaluate();
	}

	private bool IsConditionMatched<T>(int indexId, T targetTypeInstance, List<formular_index_condition> FormulaIndexConditions) where T : class
	{
		ParameterExpression parameterExpression = System.Linq.Expressions.Expression.Parameter(typeof(T), "target");
		System.Linq.Expressions.Expression expression = null;
		List<formular_index_condition> list = (from c in FormulaIndexConditions
			where c.IndexId == indexId
			orderby c.Index
			select c).ToList();
		foreach (formular_index_condition condition in list)
		{
			PropertyInfo propertyInfo = typeof(T).GetProperties().FirstOrDefault((PropertyInfo a) => a.Name == condition.PropertyName);
			if (propertyInfo == null)
			{
				continue;
			}
			object value = propertyInfo.GetValue(targetTypeInstance) ?? "";
			ConstantExpression constantExpression = System.Linq.Expressions.Expression.Constant(value);
			ConstantExpression constantExpression2 = System.Linq.Expressions.Expression.Constant(Convert.ChangeType(condition.FieldValue, propertyInfo.PropertyType), propertyInfo.PropertyType);
			System.Linq.Expressions.Expression expression2;
			switch (condition.FieldOperator)
			{
			case FieldOperator.等于:
				expression2 = System.Linq.Expressions.Expression.Equal(constantExpression, constantExpression2);
				break;
			case FieldOperator.不等于:
				expression2 = System.Linq.Expressions.Expression.NotEqual(constantExpression, constantExpression2);
				break;
			case FieldOperator.包含:
				if (propertyInfo.PropertyType == typeof(string))
				{
					expression2 = System.Linq.Expressions.Expression.Call(constantExpression, "Contains", null, constantExpression2);
					break;
				}
				goto default;
			case FieldOperator.不包含:
				if (propertyInfo.PropertyType == typeof(string))
				{
					expression2 = System.Linq.Expressions.Expression.Not(System.Linq.Expressions.Expression.Call(constantExpression, "Contains", null, constantExpression2));
					break;
				}
				goto default;
			case FieldOperator.起始于:
				if (propertyInfo.PropertyType == typeof(string))
				{
					expression2 = System.Linq.Expressions.Expression.Call(constantExpression, "StartsWith", null, constantExpression2);
					break;
				}
				goto default;
			case FieldOperator.终止于:
				if (propertyInfo.PropertyType == typeof(string))
				{
					expression2 = System.Linq.Expressions.Expression.Call(constantExpression, "EndsWith", null, constantExpression2);
					break;
				}
				goto default;
			default:
				throw new NotSupportedException($"Unsupported operator: {condition.FieldOperator}");
			}
			System.Linq.Expressions.Expression expression3 = expression2;
			System.Linq.Expressions.Expression expression4 = ((expression != null) ? (condition.ConditionOperator switch
			{
				ConditionOperator.并且 => System.Linq.Expressions.Expression.AndAlso(expression, expression3), 
				ConditionOperator.或 => System.Linq.Expressions.Expression.OrElse(expression, expression3), 
				ConditionOperator.无 => expression, 
				_ => throw new NotSupportedException($"Unsupported conjunction operator: {condition.ConditionOperator}"), 
			}) : expression3);
			expression = expression4;
		}
		if (expression != null)
		{
			Func<T, bool> func = System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(expression, new ParameterExpression[1] { parameterExpression }).Compile();
			return func(targetTypeInstance);
		}
		return false;
	}
}
