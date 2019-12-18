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
	public class SQLTranslatorInsert : SQLTranslator
	{
		public override string GetCaption()
		{
			return "Insert";
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
				checkBox.Text = "Explicit projection names";
				checkBox.AutoSize = false;
				checkBox.AutoSize = true;
				checkBox.Name = "option_explicit_pname";
				checkBox.Checked = true;
				checkBox.CheckedChanged += changedHandler;
				Parent.Controls.Add(checkBox);
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
				checkBox.Text = "values";
				checkBox.Name = "option_values";
				checkBox.CheckedChanged += changedHandler;
				Parent.Controls.Add(checkBox);
			}

			{
				CheckBox checkBox = new CheckBox();
				checkBox.Text = "include default columns";
				checkBox.Name = "option_default";
				checkBox.Checked = true;
				checkBox.CheckedChanged += changedHandler;
				Parent.Controls.Add(checkBox);
			}

			{
				CheckBox checkBox = new CheckBox();
				checkBox.Text = "not already exists";
				checkBox.Name = "option_notalready";
				checkBox.Checked = false;
				checkBox.CheckedChanged += changedHandler;
				Parent.Controls.Add(checkBox);
			}
			
		}

		public override string TranslateExt(CreateTableStatement createTableStatement, object options)
		{
			TableDefinition tableDefinition = createTableStatement.Definition;
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
			if (bOptionInline) sColumnSeparator = null;

			bool bOptionExplicitNames = false;
			if (options is Control)
			{
				var r = ((Control)options).Controls.Find("option_explicit_pname", true);
				if (r.Length > 0)
				{
					bOptionExplicitNames = (r[0] as CheckBox).Checked;
				}
			}

			bool bOptionValues = false;
			if (options is Control)
			{
				var r = ((Control)options).Controls.Find("option_values", true);
				if (r.Length > 0)
				{
					bOptionValues = (r[0] as CheckBox).Checked;
				}
			}

			bool bOptionDefault = true;
			if (options is Control)
			{
				var r = ((Control)options).Controls.Find("option_default", true);
				if (r.Length > 0)
				{
					bOptionDefault = (r[0] as CheckBox).Checked;
				}
			}

			bool bNotAlready = false;
			if (options is Control)
			{
				var r = ((Control)options).Controls.Find("option_notalready", true);
				if (r.Length > 0)
				{
					bNotAlready = (r[0] as CheckBox).Checked;
				}
			}

			string keywordSep = bOptionInline ? " " : Environment.NewLine;
			string optionAllias = optionAllias0;
			string optionAlliasDest = optionAlliasDest0;
			string tableName = TSQLHelper.Identifiers2Value(createTableStatement.SchemaObjectName.Identifiers);
			if (!String.IsNullOrEmpty(optionAllias)) optionAllias = optionAllias + '.';
			if (!String.IsNullOrEmpty(optionAlliasDest)) optionAlliasDest = optionAlliasDest + '.';
			string columnIdent = "\t";
			if (bOptionInline) columnIdent = null;

			StringBuilder result = new StringBuilder();
			string sep = null;
			// insert into
			{
				result.Append($"insert into {tableName}({sColumnSeparator}");
				sep = null;
				foreach (ColumnDefinition columnDefinition in tableDefinition.ColumnDefinitions)
				{
					if (!bOptionDefault && TSQLHelper.ColumnIsDefault(columnDefinition)) continue;
					string ident = TSQLHelper.Identifier2Value(columnDefinition.ColumnIdentifier);
					//if (!bOptionInline)
						result.Append($"{columnIdent}{sep}{ident}{sColumnSeparator}");
					//else result.Append($"{sep}{ident}{sColumnSeparator}");
					if (String.IsNullOrEmpty(sep)) sep = ", ";
				}
				result.Append($"){Environment.NewLine}");
			}

			if (bOptionValues)
			{
				result.Append($"values(");
				sep = null;
				foreach (ColumnDefinition columnDefinition in tableDefinition.ColumnDefinitions)
				{
					if (!bOptionDefault && TSQLHelper.ColumnIsDefault(columnDefinition)) continue;
					string ident = TSQLHelper.Identifier2Value(columnDefinition.ColumnIdentifier);
					if (bOptionExplicitNames)
					{
						result.Append($"{columnIdent}{sep}{optionAllias}{ident} /*{ident}*/{sColumnSeparator}");
					}
					else result.Append($"{columnIdent}{sep}{optionAllias}{ident}{sColumnSeparator}");
					if (String.IsNullOrEmpty(sep)) sep = ", ";
				}
				result.Append($"){Environment.NewLine}");
			}
			else
			// select
			{
				result.Append($"select{keywordSep}");
				sep = null;
				foreach (ColumnDefinition columnDefinition in tableDefinition.ColumnDefinitions)
				{
					if (!bOptionDefault && TSQLHelper.ColumnIsDefault(columnDefinition)) continue;
					string ident = TSQLHelper.Identifier2Value(columnDefinition.ColumnIdentifier);
					if (bOptionExplicitNames)
					{
						result.Append($"{columnIdent}{sep}{ident} = {optionAllias}{ident}{sColumnSeparator}");
					} else
					{
						result.Append($"{columnIdent}{sep}{optionAllias}{ident}{sColumnSeparator}");
					}
					if (String.IsNullOrEmpty(sep)) sep = ", ";
				}
				
				result.Append($"{keywordSep}from {optionAllias0}{Environment.NewLine}");
			}
			

			if (bNotAlready)
			{
				result.Append($"where not exists(select 1 from {tableName} t with (nolock) where");
				int i = 0;
				foreach (ColumnDefinition columnDefinition in tableDefinition.ColumnDefinitions)
				{
					i++;
					if (!bOptionDefault && TSQLHelper.ColumnIsDefault(columnDefinition)) continue;
					string ident = TSQLHelper.Identifier2Value(columnDefinition.ColumnIdentifier);
					if (i > 1)
						result.Append($" and");
					result.Append($" t.{ident} = {optionAllias}{ident}");
				}
				result.Append($")");
			}

			result.Append($";");
			return result.ToString();
		}
	}
}
