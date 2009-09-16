﻿/* ***********************************************
 * Author		:  kingthy
 * Email		:  kingthy@gmail.com
 * DateTime		:  2009-9-11 11:15:41
 * Description	:  PropertyTag
 *
 * ***********************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace VTemplate.Engine
{
    /// <summary>
    /// 属性或字段获取标签.如: &lt;vt:property var="time" field="Now" type="System.DateTime" /&gt;
    /// </summary>
    public class PropertyTag : Tag
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ownerTemplate"></param>
        internal PropertyTag(Template ownerTemplate)
            : base(ownerTemplate)
        {
        }

        #region 重写Tag的方法
        /// <summary>
        /// 返回标签的名称
        /// </summary>
        public override string TagName
        {
            get { return "property"; }
        }
        /// <summary>
        /// 返回此标签是否是单一标签.即是不需要配对的结束标签
        /// </summary>
        internal override bool IsSingleTag
        {
            get { return false; }
        }
        #endregion

        #region 属性定义
        /// <summary>
        /// 调用的属性或字段
        /// </summary>
        public string Field { get; protected set; }

        /// <summary>
        /// 包含属性或字段的类型
        /// </summary>
        public IExpression Type { get; protected set; }

        /// <summary>
        /// 存储表达式结果的变量
        /// </summary>
        public Variable Variable { get; protected set; }
        #endregion

        #region 添加标签属性时的触发函数.用于设置自身的某些属性值
        /// <summary>
        /// 添加标签属性时的触发函数.用于设置自身的某些属性值
        /// </summary>
        /// <param name="name"></param>
        /// <param name="item"></param>
        protected override void OnAddingAttribute(string name, Attribute item)
        {
            switch (name)
            {
                case "field":
                    this.Field = item.Value.Trim();
                    break;
                case "type":
                    this.Type = ParserHelper.CreateExpression(this.OwnerTemplate, item.Value.Trim());
                    break;
                case "var":
                    this.Variable = Utility.GetVariableOrAddNew(this.OwnerTemplate, item.Value);
                    break;
            }
        }
        #endregion

        #region 呈现本元素的数据
        /// <summary>
        /// 呈现本元素的数据
        /// </summary>
        /// <param name="writer"></param>
        public override void Render(System.IO.TextWriter writer)
        {
            //如果类型定义的是变量表达式则获取表达式的值,否则建立类型
            object container = this.Type is VariableExpression ? this.Type.GetValue() : Utility.CreateType(this.Type.GetValue().ToString());
            bool exits;
            this.Variable.Value = container == null ? null : Utility.GetPropertyValue(container, this.Field, out exits);
            base.Render(writer);
        }
        #endregion

        #region 开始解析标签数据
        /// <summary>
        /// 开始解析标签数据
        /// </summary>
        /// <param name="ownerTemplate">宿主模版</param>
        /// <param name="container">标签的容器</param>
        /// <param name="tagStack">标签堆栈</param>
        /// <param name="text"></param>
        /// <param name="match"></param>
        /// <param name="isClosedTag">是否闭合标签</param>
        /// <returns>如果需要继续处理EndTag则返回true.否则请返回false</returns>
        internal override bool ProcessBeginTag(Template ownerTemplate, Tag container, Stack<Tag> tagStack, string text, ref Match match, bool isClosedTag)
        {
            if (this.Variable == null) throw new ParserException(string.Format("{0}标签中缺少var属性", this.TagName));
            if (string.IsNullOrEmpty(this.Field)) throw new ParserException(string.Format("{0}标签中缺少field属性", this.TagName));
            if (this.Type == null) throw new ParserException(string.Format("{0}标签中缺少type属性", this.TagName));

            return base.ProcessBeginTag(ownerTemplate, container, tagStack, text, ref match, isClosedTag);
        }
        #endregion

        #region 克隆当前元素到新的宿主模版
        /// <summary>
        /// 克隆当前元素到新的宿主模版
        /// </summary>
        /// <param name="ownerTemplate"></param>
        /// <returns></returns>
        internal override Element Clone(Template ownerTemplate)
        {
            PropertyTag tag = new PropertyTag(ownerTemplate);
            this.CopyTo(tag);
            tag.Field = this.Field;
            tag.Type = (IExpression)this.Type.Clone(ownerTemplate);
            tag.Variable = this.Variable == null ? null : Utility.GetVariableOrAddNew(ownerTemplate, this.Variable.Name);
            return tag;
        }
        #endregion
    }
}