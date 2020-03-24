using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SQL_Format
{
	public class SQLTranslatorSelect : SQLTranslator
	{
		public override string GetCaption()
		{
			return "Select";
		}

		public override void SetupOptionsContent(Control Parent, EventHandler changedHandler)
		{
			{
				Label label = new Label();
				label.Margin = new Padding(10, 3, 0, 0);
				label.Text = "Alias: ";
				label.MinimumSize = new Size(0, 17);
				label.AutoSize = false;
				label.AutoSize = true;
				Parent.Controls.Add(label);

				TextBox tbAlias = new TextBox();
				tbAlias.Text = "t";
				tbAlias.Name = "option_alias";
				tbAlias.TextChanged += changedHandler;
				Parent.Controls.Add(tbAlias);
			}
			{
				CheckBox checkBox = new CheckBox();
				checkBox.Text = "Inline";
				checkBox.Name = "option_inline";
				checkBox.CheckedChanged += changedHandler;
				Parent.Controls.Add(checkBox);
			}
            {
                CheckBox checkBox = new CheckBox();
                checkBox.Text = "Forward Comma";
                checkBox.Name = "option_forward_comma";
                checkBox.CheckedChanged += changedHandler;
                Parent.Controls.Add(checkBox);
            }
        }

        public override string Translate(TableDefinition tableDefinition, object options)
		{
			string optionAllias = null;
			if (options is Control)
			{
				var r = ((Control)options).Controls.Find("option_alias", true);
				if (r.Length > 0)
				{
					optionAllias = (r[0] as TextBox).Text;
				}
			}
			if (!String.IsNullOrEmpty(optionAllias)) optionAllias = optionAllias + '.';

			bool bOptionInline = false;
			if (options is Control)
			{
				var r = ((Control)options).Controls.Find("option_inline", true);
				if (r.Length > 0)
				{
					bOptionInline = (r[0] as CheckBox).Checked;
				}
			}
            bool option_forward_comma = false;
            if (options is Control)
            {
                var r = ((Control)options).Controls.Find("option_forward_comma", true);
                if (r.Length > 0)
                {
                    option_forward_comma = (r[0] as CheckBox).Checked;
                }
            }
            string sColumnSeparator = Environment.NewLine;
			if (bOptionInline) sColumnSeparator = " ";

			StringBuilder result = new StringBuilder();
			foreach(ColumnDefinition columnDefinition in tableDefinition.ColumnDefinitions)
			{
				string ident = TSQLHelper.Identifier2Value(columnDefinition.ColumnIdentifier);
                if (!option_forward_comma)
				    result.Append($"{optionAllias}{ident},{sColumnSeparator}");
                else result.Append($"{sColumnSeparator}, {optionAllias}{ident}");
            }

			return result.ToString();
		}
	}
}
