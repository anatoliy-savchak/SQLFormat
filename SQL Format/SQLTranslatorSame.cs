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
	public class SQLTranslatorSame : SQLTranslator
	{
		public override string GetCaption()
		{
			return "Same";
		}

		public override void SetupOptionsContent(Control Parent, EventHandler changedHandler)
		{
			{
				Label label = new Label();
				label.Margin = new Padding(10, 3, 0, 0);
				label.Text = "Source Alias: ";
				label.MinimumSize = new Size(0, 17);
				label.AutoSize = false;
				label.AutoSize = true;
				Parent.Controls.Add(label);

				TextBox tbAlias = new TextBox();
				tbAlias.Text = "s";
				tbAlias.Name = "option_alias_src";
				tbAlias.TextChanged += changedHandler;
				Parent.Controls.Add(tbAlias);
			}

			{
				Label label = new Label();
				label.Margin = new Padding(10, 3, 0, 0);
				label.Text = "Target Alias: ";
				label.MinimumSize = new Size(0, 17);
				label.AutoSize = false;
				label.AutoSize = true;
				Parent.Controls.Add(label);

				TextBox tbAlias = new TextBox();
				tbAlias.Text = "t";
				tbAlias.Name = "option_alias_dest";
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

		}

		public override string Translate(TableDefinition tableDefinition, object options)
		{
			string optionAllias0 = null;
			if (options is Control)
			{
				var r = ((Control)options).Controls.Find("option_alias_src", true);
				if (r.Length > 0)
				{
					optionAllias0 = (r[0] as TextBox).Text;
				}
			}
			string optionAlliasDest0 = null;
			if (options is Control)
			{
				var r = ((Control)options).Controls.Find("option_alias_dest", true);
				if (r.Length > 0)
				{
					optionAlliasDest0 = (r[0] as TextBox).Text;
				}
			}

			bool bOptionInline = false;
			if (options is Control)
			{
				var r = ((Control)options).Controls.Find("option_inline", true);
				if (r.Length > 0)
				{
					bOptionInline = (r[0] as CheckBox).Checked;
				}
			}
			string sColumnSeparator = Environment.NewLine;
			if (bOptionInline) sColumnSeparator = " ";

			bool bOptionExplicitNames = false;
			if (options is Control)
			{
				var r = ((Control)options).Controls.Find("option_explicit_pname", true);
				if (r.Length > 0)
				{
					bOptionExplicitNames = (r[0] as CheckBox).Checked;
				}
			}

			string optionAllias = optionAllias0;
			string optionAlliasDest = optionAlliasDest0;
			if (!String.IsNullOrEmpty(optionAllias)) optionAllias = optionAllias + '.';
			if (!String.IsNullOrEmpty(optionAlliasDest)) optionAlliasDest = optionAlliasDest + '.';
			string columnIdent = "\t\t";
			if (bOptionInline) columnIdent = null;

			StringBuilder result = new StringBuilder();
			string sep = null;
			// line1
			{
				result.Append($"/*same*/ exists({Environment.NewLine}");
				result.Append($"\tselect{sColumnSeparator}");
				sep = null;
				foreach (ColumnDefinition columnDefinition in tableDefinition.ColumnDefinitions)
				{
					string ident = TSQLHelper.Identifier2Value(columnDefinition.ColumnIdentifier);
					result.Append($"{columnIdent}{sep}{optionAlliasDest}{ident}{sColumnSeparator}");
					if (String.IsNullOrEmpty(sep)) sep = ", ";
				}
			}
			//line2
			{
				if (bOptionInline) result.Append($"{Environment.NewLine}");
				result.Append($"\tintersect{Environment.NewLine}");
				result.Append($"\tselect{sColumnSeparator}");
				sep = null;
				foreach (ColumnDefinition columnDefinition in tableDefinition.ColumnDefinitions)
				{
					string ident = TSQLHelper.Identifier2Value(columnDefinition.ColumnIdentifier);
					result.Append($"{columnIdent}{sep}{optionAllias}{ident}{sColumnSeparator}");
					if (String.IsNullOrEmpty(sep)) sep = ", ";
				}
				result.Append($"{Environment.NewLine}");
			}
			result.Append($"){Environment.NewLine}");
			return result.ToString();
		}
	}
}
