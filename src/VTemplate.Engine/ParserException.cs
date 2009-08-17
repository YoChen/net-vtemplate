﻿/* ***********************************************
 * Author		:  kingthy
 * Email		:  kingthy@gmail.com
 * Description	:  ParseException
 *
 * ***********************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;

namespace VTemplate.Engine
{
    /// <summary>
    /// 解析模版时的错误
    /// </summary>
    public class ParserException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message">描述信息</param>
        public ParserException(string message)
            : base(message)
        {
            this.HaveLineAndColumnNumber = false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Point">行号列号(x = 列号, y = 行号)</param>
        /// <param name="text">模版文本数据</param>
        /// <param name="message">描述信息</param>
        public ParserException(Point p, string text, string message)
            : this(p.Y, p.X, text, message)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="line">所在行号</param>
        /// <param name="column">所在列</param>
        /// <param name="text">模版文本数据</param>
        /// <param name="message">描述信息</param>
        public ParserException(int line, int column, string text, string message)
            : base(string.Format("在解析(行{0}:列{1})的模版文本字符\"{2}\"时,发生错误:{3}", line, column, text, message))
        {
            this.HaveLineAndColumnNumber = true;   
        }

        /// <summary>
        /// 是否包含行号与列号
        /// </summary>
        public bool HaveLineAndColumnNumber { get; private set; }
    }
}