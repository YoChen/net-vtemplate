﻿/* ***********************************************
 * Author		:  kingthy
 * Email		:  kingthy@gmail.com
 * Description	:  ParseHelper
 *
 * ***********************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace VTemplate.Engine
{
    /// <summary>
    /// 解析器的帮助类
    /// </summary>
    internal static class ParserHelper
    {
        /// <summary>
        /// 解析元素的属性列表
        /// </summary>
        /// <param name="element"></param>
        /// <param name="match"></param>
        internal static void ParseElementAttributes(IAttributesElement element, Match match)
        {
            //处理属性
            CaptureCollection attrNames = match.Groups["attrname"].Captures;
            CaptureCollection attrVals = match.Groups["attrval"].Captures;

            for (int i = 0; i < attrNames.Count; i++)
            {
                string attrName = attrNames[i].Value;
                string attrVal = HttpUtility.HtmlDecode(attrVals[i].ToString());

                //加入属性列表
                if (!string.IsNullOrEmpty(attrName))
                    element.Attributes.Add(attrName, attrVal);
            }
        }

        /// <summary>
        /// 构建文本节点元素
        /// </summary>
        /// <param name="ownerTemplate">宿主模版</param>
        /// <param name="container">标签的容器</param>
        /// <param name="text"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        internal static void CreateTextNode(Template ownerTemplate, Tag container, string text, int offset, int length)
        {
            if (length > 0)
            {
                container.InnerElements.Add(new TextNode(ownerTemplate, text.Substring(offset, length)));
            }
        }

        /// <summary>
        /// 从匹配项中建构建变量实例
        /// </summary>
        /// <param name="ownerTemplate"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        internal static Variable CreateVariable(Template ownerTemplate, Match match)
        {
            string prefix = match.Groups["prefix"].Success ? match.Groups["prefix"].Value : null;
            string name = match.Groups["name"].Value;

            ownerTemplate = Utility.GetVariableTemplateByPrefix(ownerTemplate, prefix);
            if (ownerTemplate == null) throw new ParserException(string.Format("变量的宿主模版#{0}不存在", prefix));

            Variable variable = Utility.GetVariableOrAddNew(ownerTemplate, name);
            return variable;
        }

        /// <summary>
        /// 构建变量的字段列表
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        internal static VariableExpression CreateVariableExpression(Variable variable, Match match)
        {
            //解析变量字段列表
            string prefix = match.Groups["prefix"].Success ? match.Groups["prefix"].Value : null;
            VariableExpression field = new VariableExpression(prefix, variable);
            CaptureCollection fields = match.Groups["field"].Captures;
            CaptureCollection methods = match.Groups["method"].Captures;
            VariableExpression item = field;
            for (var i = 0; i < fields.Count; i++)
            {
                string fieldName = fields[i].Value;
                VariableExpression exp = new VariableExpression(item, fieldName, !string.IsNullOrEmpty(methods[i].Value));
                item = exp;
            }

            return field;
        }

        /// <summary>
        /// 从变量表达式文本(如:name.age)中构建变量表达式
        /// </summary>
        /// <param name="expressionText"></param>
        /// <returns></returns>
        internal static VariableExpression CreateVariableExpression(Template ownerTemplate, string expressionText)
        {
            if (string.IsNullOrEmpty(expressionText)) return null;

            Match match = ParserRegex.VarExpRegex.Match(expressionText);
            if (match.Success)
            {
                Variable variable = CreateVariable(ownerTemplate, match);
                return CreateVariableExpression(variable, match);
            }
            else
            {
                //非变量表达式
                return null;
            }
        }
        /// <summary>
        /// 从表达式文本中构造表达式.如果表达式是以$字符开头.并且不是以$$字符开头.则认为是变量表达式.否则则认为为常量表达式
        /// </summary>
        /// <param name="ownerTemplate"></param>
        /// <param name="expressionText"></param>
        /// <returns></returns>
        internal static IExpression CreateExpression(Template ownerTemplate, string expressionText)
        {
            if (string.IsNullOrEmpty(expressionText)) return new ConstantExpression(expressionText);

            if (expressionText.StartsWith("$"))
            {
                expressionText = expressionText.Remove(0, 1);
                if (expressionText.StartsWith("$"))
                {
                    //$$字符开头.则认为是常量表达式
                    return new ConstantExpression(expressionText);
                }
                else
                {
                    //变量表达式
                    return CreateVariableExpression(ownerTemplate, expressionText);
                }
            }
            else
            {
                //常量表达式
                return new ConstantExpression(expressionText);
            }
        }

        /// <summary>
        /// 构建变量元素
        /// </summary>
        /// <param name="ownerTemplate">宿主模版</param>
        /// <param name="container">标签的容器</param>
        /// <param name="match"></param>
        internal static VariableTag CreateVariableTag(Template ownerTemplate, Tag container, Match match)
        {
            Variable variable = CreateVariable(ownerTemplate, match);
            VariableExpression varExp = CreateVariableExpression(variable, match);

            VariableTag tag = new VariableTag(ownerTemplate, varExp);
            //解析属性列表
            ParseElementAttributes(tag, match);
            container.InnerElements.Add(tag);

            return tag;
        }

        /// <summary>
        /// 构建标签元素
        /// </summary>
        /// <param name="ownerTemplate">宿主模版</param>
        /// <param name="match"></param>
        /// <param name="isClosedTag">是否是自闭合标签</param>
        /// <returns></returns>
        internal static Tag CreateTag(Template ownerTemplate, Match match,out bool isClosedTag)
        {
            string tagName = match.Groups["tagname"].Value;
            isClosedTag = match.Groups["closed"].Success;

            Tag tag = TagFactory.FromTagName(ownerTemplate, tagName);
            if (tag == null) throw new ParserException(string.Format("不能识别的元素标签\"{0}\"", tagName));

            ParseElementAttributes(tag, match);

            return tag;
        }
    }
}
