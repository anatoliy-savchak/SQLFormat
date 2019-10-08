using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SQL_Format
{
	public class SQLTranslatorXmlSelect : SQLTranslator
	{
		public override string GetCaption()
		{
			return "Xml Select";
		}

		public override void SetupOptionsContent(Control Parent, EventHandler changedHandler)
		{
			{
				Label label = new Label();
				label.Margin = new Padding(10, 3, 0, 0);
				label.Text = "Source: ";
				label.MinimumSize = new Size(0, 17);
				label.AutoSize = false;
				label.AutoSize = true;
				Parent.Controls.Add(label);

				TextBox tbAlias = new TextBox();
				tbAlias.Text = "x";
				tbAlias.Name = "option_source";
				tbAlias.TextChanged += changedHandler;
				Parent.Controls.Add(tbAlias);
			}

			{
				Label label = new Label();
				label.Margin = new Padding(10, 3, 0, 0);
				label.Text = "Row Alias: ";
				label.MinimumSize = new Size(0, 17);
				label.AutoSize = false;
				label.AutoSize = true;
				Parent.Controls.Add(label);

				TextBox tbAlias = new TextBox();
				tbAlias.Text = "n";
				tbAlias.Name = "option_alias_row";
				tbAlias.TextChanged += changedHandler;
				Parent.Controls.Add(tbAlias);
			}

			{
				CheckBox checkBox = new CheckBox();
				checkBox.Text = "Safe";
				checkBox.Name = "option_safe";
				checkBox.CheckedChanged += changedHandler;
				Parent.Controls.Add(checkBox);
			}

			{
				CheckBox checkBox = new CheckBox();
				checkBox.Text = "C Columns";
				checkBox.Name = "option_ccolumns";
				checkBox.CheckedChanged += changedHandler;
				Parent.Controls.Add(checkBox);
			}
		}

		public override string Translate(TableDefinition tableDefinition, object options)
		{
			string optionSource0 = null;
			if (options is Control)
			{
				var r = ((Control)options).Controls.Find("option_source", true);
				if (r.Length > 0)
				{
					optionSource0 = (r[0] as TextBox).Text;
				}
			}
			string optionRowAlias0 = null;
			if (options is Control)
			{
				var r = ((Control)options).Controls.Find("option_alias_row", true);
				if (r.Length > 0)
				{
					optionRowAlias0 = (r[0] as TextBox).Text;
				}
			}

			bool optionsSafe = false;
			if (options is Control)
			{
				var r = ((Control)options).Controls.Find("option_safe", true);
				if (r.Length > 0)
				{
					optionsSafe = (r[0] as CheckBox).Checked;
				}
			}

			bool option_ccolumns = false;
			if (options is Control)
			{
				var r = ((Control)options).Controls.Find("option_ccolumns", true);
				if (r.Length > 0)
				{
					option_ccolumns = (r[0] as CheckBox).Checked;
				}
			}

			StringBuilder result = new StringBuilder();
			string sep = null;
			result.Append($"select{Environment.NewLine}");
			// rows
			{
				sep = null;
				int iter = -1;
				foreach (ColumnDefinition columnDefinition in tableDefinition.ColumnDefinitions)
				{
					iter++;
					string ident = TSQLHelper.Identifier2Value(columnDefinition.ColumnIdentifier);
					string typ = TSQLHelper.Column2TypeStr(columnDefinition);
					string attName = ident;
					if (option_ccolumns) {
						attName = $"c{iter}";
					}
					if (optionsSafe)
					{
						result.Append($"\t{sep}{ident} = case when {optionRowAlias0}.value('@{attName}', 'nvarchar(max)') = \'NULL\' then null else {optionRowAlias0}.value('@{attName}', '{typ}') end{Environment.NewLine}");
					}
					else
					{
						result.Append($"\t{sep}{ident} = {optionRowAlias0}.value('@{attName}', '{typ}'){Environment.NewLine}");
					}
					if (String.IsNullOrEmpty(sep)) sep = ", ";
				}
			}
			result.Append($"from @{optionSource0}.nodes('/root[1]/data[1]/row') as t({optionRowAlias0}){Environment.NewLine}");
			return result.ToString();
		}
	}
}
