﻿﻿using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using IODataPlatform.Models;
using IODataPlatform.Models.DBModels;
using IODataPlatform.Models.ExcelModels;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Expression = System.Linq.Expressions.Expression;

namespace IODataPlatform.Utilities;

/// <summary>
/// 公式构建器类
/// 提供基于条件的动态公式计算功能，支持多种控制系统的字段值计算
/// 使用表达式树和Lambda表达式实现高性能的条件匹配和值计算
/// 支持复杂的字符串操作、数值计算和逻辑判断，广泛用于IO数据的智能填充
/// </summary>
public partial class FormulaBuilder {
   
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
    /// <exception cref="Exception">当公式计算出现错误时抛出异常</exception>
    public string FindMatchingFormulaReturnValueAsyncByExpression<T>(T targetTypeInstance,ControlSystem controlSystem, string fieldDisplayName, List<formular> formulars, List<formular_Index> formularIndexes, List<formular_index_condition> FormulaIndexConditions) where T : class
    {
        try
        {
            Type type = typeof(T);           
            // 先找到 FormularId
            var fieldIdToGet = formulars
                .Where(q => q.TargetType == controlSystem.ToString() && q.FieldName == fieldDisplayName)
                .FirstOrDefault()?.Id;// 找到字段 Id
            if (fieldIdToGet == null) return "Err";
            var values = formularIndexes.Where(f => f.FormulaId == fieldIdToGet).OrderBy(x => x.Index).ToList(); // 找到结果     
            foreach (var value in values)
            {
                ParameterExpression paramTarget = System.Linq.Expressions.Expression.Parameter(typeof(T), "target");
                Expression finalExpression = null;

                var conditions = FormulaIndexConditions
                    .Where(f => f.IndexId == value.Id)
                    .OrderBy(c => c.Index)
                    .ToList();
                foreach (var condition in conditions)
                {
                    var propertyInfo = typeof(T).GetProperties().FirstOrDefault(a => a.Name == condition.PropertyName);

                    if (propertyInfo == null)
                    {
                        continue; // 或抛出异常
                    }
                    var propertyValue = propertyInfo.GetValue(targetTypeInstance) ?? "";
                    ConstantExpression propertyConstantValue = Expression.Constant(propertyValue);
                    ConstantExpression constantValue = Expression.Constant(Convert.ChangeType(condition.FieldValue, propertyInfo.PropertyType), propertyInfo.PropertyType);

                    Expression conditionExpression = condition.FieldOperator switch
                    {
                        FieldOperator.等于 => Expression.Equal(propertyConstantValue, constantValue),
                        FieldOperator.不等于 => Expression.NotEqual(propertyConstantValue, constantValue),
                        FieldOperator.包含 when propertyInfo.PropertyType == typeof(string) =>
                            Expression.Call(propertyConstantValue, "Contains", null, constantValue),
                        FieldOperator.不包含 when propertyInfo.PropertyType == typeof(string) =>
                            Expression.Not(Expression.Call(propertyConstantValue, "Contains", null, constantValue)),
                        FieldOperator.起始于 when propertyInfo.PropertyType == typeof(string) =>
                            Expression.Call(propertyConstantValue, "StartsWith", null, constantValue),
                        FieldOperator.终止于 when propertyInfo.PropertyType == typeof(string) =>
                            Expression.Call(propertyConstantValue, "EndsWith", null, constantValue),
                        // 添加其他运算符的支持
                        _ => throw new NotSupportedException($"不支持的运算符: {condition.FieldOperator}")
                    };


                    finalExpression = finalExpression == null
                        ? conditionExpression
                        : condition.ConditionOperator switch
                        {
                            ConditionOperator.并且 => Expression.AndAlso(finalExpression, conditionExpression),
                            ConditionOperator.或 => Expression.OrElse(finalExpression, conditionExpression),
                            ConditionOperator.无 => finalExpression,
                            _ => throw new NotSupportedException($"不支持的连接运算符: {condition.ConditionOperator}")
                        };
                }

                if (finalExpression != null)
                {
                    var conditionDelegate = Expression.Lambda<Func<T, bool>>(finalExpression, paramTarget).Compile();
                    if (conditionDelegate(targetTypeInstance))
                    {
                        return value.ReturnValue;
                    }
                }
            }
            return "Err";
        }
        catch (Exception ex)
        {        
            throw new Exception($"获取{fieldDisplayName}错误");
        }
       
    }

    public string? FindMatchingFormulaReturnValueAsyncByExpressionNew<T>(T targetTypeInstance, ControlSystem controlSystem, string fieldDisplayName, List<formular> formulars, List<formular_Index> formularIndexes, List<formular_index_condition> FormulaIndexConditions) where T : class
    {
        try
        {
            Type type = typeof(T);
            // Find the FormulaId
            var fieldIdToGet = formulars.FirstOrDefault(q => q.TargetType == controlSystem.ToString() && q.FieldName == fieldDisplayName)?.Id;
            if (fieldIdToGet == null) return "Err";

            var values = formularIndexes.Where(f => f.FormulaId == fieldIdToGet).OrderBy(x => x.Index).ToList();
            foreach (formular_Index value in values)
            {
                if (IsConditionMatched(value.Id, targetTypeInstance, FormulaIndexConditions))
                {
                    if (value.ReturnValue.Contains("CH"))
                    {
                        var chValue = (int)type.GetProperty("Channel")?.GetValue(targetTypeInstance);
                        string replacedExpression = value.ReturnValue.Replace("CH", chValue.ToString());
                        // 使用其他表达式计算库来计算表达式
                        var result = CalculateChExpression(replacedExpression, chValue);
                        return result.ToString();
                    }
                    return value.ReturnValue;
                }
            }
            return "Err";
        }
        catch (Exception ex)
        {
            throw new Exception($"获取{fieldDisplayName}错误");
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
            // 处理算术表达式
            return CalculateExpression(expression).ToString();
        }       
        else
        {
            // 处理其他情况，如纯数字
            return expression;
        }
    }    
    private object CalculateExpression(string expressionString)
    {
         NCalc.Expression expression = new NCalc.Expression(expressionString);
        return expression.Evaluate();
    }
    private bool IsConditionMatched<T>(int indexId, T targetTypeInstance, List<formular_index_condition> FormulaIndexConditions) where T : class
    {
        ParameterExpression paramTarget = Expression.Parameter(typeof(T), "target");
        Expression finalExpression = null;

        var conditions = FormulaIndexConditions.Where(f => f.IndexId == indexId).OrderBy(c => c.Index).ToList();
        foreach (var condition in conditions)
        {
            var propertyInfo = typeof(T).GetProperties().FirstOrDefault(a => a.Name == condition.PropertyName);
            if (propertyInfo == null)
            {
                continue; // Or throw an exception
            }
            var propertyValue = propertyInfo.GetValue(targetTypeInstance) ?? "";
            ConstantExpression propertyConstantValue = Expression.Constant(propertyValue);
            ConstantExpression constantValue = Expression.Constant(Convert.ChangeType(condition.FieldValue, propertyInfo.PropertyType), propertyInfo.PropertyType);

            Expression conditionExpression = condition.FieldOperator switch
            {
                FieldOperator.等于 => Expression.Equal(propertyConstantValue, constantValue),
                FieldOperator.不等于 => Expression.NotEqual(propertyConstantValue, constantValue),
                FieldOperator.包含 when propertyInfo.PropertyType == typeof(string) => Expression.Call(propertyConstantValue, "Contains", null, constantValue),
                FieldOperator.不包含 when propertyInfo.PropertyType == typeof(string) => Expression.Not(Expression.Call(propertyConstantValue, "Contains", null, constantValue)),
                FieldOperator.起始于 when propertyInfo.PropertyType == typeof(string) => Expression.Call(propertyConstantValue, "StartsWith", null, constantValue),
                FieldOperator.终止于 when propertyInfo.PropertyType == typeof(string) => Expression.Call(propertyConstantValue, "EndsWith", null, constantValue),
                // Add other operators as needed
                _ => throw new NotSupportedException($"Unsupported operator: {condition.FieldOperator}")
            };

            finalExpression = finalExpression == null
                ? conditionExpression
                : condition.ConditionOperator switch
                {
                    ConditionOperator.并且 => Expression.AndAlso(finalExpression, conditionExpression),
                    ConditionOperator.或 => Expression.OrElse(finalExpression, conditionExpression),
                    ConditionOperator.无 => finalExpression,
                    _ => throw new NotSupportedException($"Unsupported conjunction operator: {condition.ConditionOperator}")
                };
        }

        if (finalExpression != null)
        {
            var conditionDelegate = Expression.Lambda<Func<T, bool>>(finalExpression, paramTarget).Compile();
            return conditionDelegate(targetTypeInstance);
        }
        return false;
    }

   

}