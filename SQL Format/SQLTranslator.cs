using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace SQL_Format
{
	public abstract class SQLTranslator
	{
		public virtual string Translate(TableDefinition tableDefinition, object options) { return null; }
		public abstract string GetCaption();
		public abstract void SetupOptionsContent(Control Parent, EventHandler changedHandler);

        public virtual string TranslateExt2(CreateTableStatement createTableStatement, object options, string content, TSqlScript sqlScript)
        {
			if (createTableStatement != null)
				return TranslateExt(createTableStatement, options);
			return "";
        }

        public virtual string TranslateExt(CreateTableStatement createTableStatement, object options)
		{
			return Translate(createTableStatement.Definition, options);
		}

        public virtual string TranslateText(string text, object options)
        {
			return null;
        }

        public bool? GetOptionBool(string name, object options)
        {
			bool? result = null;
			if (options is Control)
			{
				var r = ((Control)options).Controls.Find(name, true);
				if (r.Length > 0)
				{
					result = (r[0] as CheckBox).Checked;
				}
			}
			return result;
		}

		public bool GetOptionBoolDef(string name, object options, bool def)
        {
			bool? result = GetOptionBool(name, options);
			if (!result.HasValue)
				return def;
			return result.Value;
		}

		public string GetOptionString(string name, object options)
		{
			string result = null;
			if (options is Control)
			{
				var r = ((Control)options).Controls.Find(name, true);
				if (r.Length > 0)
				{
					result = (r[0] as TextBox).Text;
				}
			}
			return result;
		}

		public object AddOptionTextBox(string caption, string name, string defaultValue, Control parent, EventHandler changedHandler)
		{
            Label label = new Label
            {
                Margin = new Padding(10, 3, 0, 0),
                Text = $"{caption}: ",
                MinimumSize = new Size(0, 17),
                AutoSize = false
            };
            label.AutoSize = true;
            parent.Controls.Add(label);

            TextBox tbAlias = new TextBox
            {
                Text = defaultValue,
                Name = name
            };
            tbAlias.TextChanged += changedHandler;
			tbAlias.Tag = label;
            parent.Controls.Add(tbAlias);

			return tbAlias;
        }

        public object AddOptionCheckBox(string caption, string name, bool defaultValue, Control parent, EventHandler changedHandler)
        {

            CheckBox tbAlias = new CheckBox
            {
                Text = caption,
				Checked = defaultValue,
                Name = name
            };
            tbAlias.CheckedChanged += changedHandler;
            parent.Controls.Add(tbAlias);

            return tbAlias;
        }
    }
}
