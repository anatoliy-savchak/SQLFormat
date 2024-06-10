using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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

			{
				CheckBox checkBox = new CheckBox();
				checkBox.Text = "insert";
				checkBox.Name = "option_insert";
				checkBox.CheckedChanged += changedHandler;
				Parent.Controls.Add(checkBox);
			}

			{
				CheckBox checkBox = new CheckBox();
				checkBox.Text = "identity ins";
				checkBox.Name = "option_identity_insert";
				checkBox.CheckedChanged += changedHandler;
				Parent.Controls.Add(checkBox);
			}

			{
				CheckBox checkBox = new CheckBox();
				checkBox.Text = "table name in path";
				checkBox.Name = "option_tab_path";
				checkBox.CheckedChanged += changedHandler;
				checkBox.AutoSize = true;
				Parent.Controls.Add(checkBox);
			}

			{
				CheckBox checkBox = new CheckBox();
				checkBox.Text = "output";
				checkBox.Name = "option_output";
				checkBox.CheckedChanged += changedHandler;
				checkBox.AutoSize = true;
				Parent.Controls.Add(checkBox);
			}

            {
                CheckBox checkBox = new CheckBox();
                checkBox.Text = "json";
                checkBox.Name = "option_json";
                checkBox.CheckedChanged += changedHandler;
                checkBox.AutoSize = true;
                Parent.Controls.Add(checkBox);
            }
        }

        public override string TranslateExt(CreateTableStatement createTableStatement, object options)
		{
			TableDefinition tableDefinition = createTableStatement.Definition;
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

			bool option_json = GetOptionBoolDef("option_json", options, false);

            bool option_insert = GetOptionBoolDef("option_insert", options, false);
			bool option_identity_insert = GetOptionBoolDef("option_identity_insert", options, false);
			bool option_tab_path = GetOptionBoolDef("option_tab_path", options, false);
			bool option_output = GetOptionBoolDef("option_output", options, false);

			string tableName = TSQLHelper.Identifiers2Value(createTableStatement.SchemaObjectName.Identifiers);
			string tableName0 = TSQLHelper.Identifiers2ValueLast(createTableStatement.SchemaObjectName.Identifiers);

			StringBuilder result = new StringBuilder();
			string sep = null;

			if (option_identity_insert)
			{
				result.Append($"set identity_insert {tableName} on;{Environment.NewLine}{Environment.NewLine}");
			}
			// insert into
			if (option_insert)
			{
				string sColumnSeparator = Environment.NewLine;
				bool bOptionDefault = true;
				string columnIdent = "\t";

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
				if (option_output)
					result.Append($"output '{tableName0}' as [Table_{tableName0}], inserted.*{Environment.NewLine}");
			}


			result.Append($"select{Environment.NewLine}");
			List<string> jsonWith = new List<string>();
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
					if (!option_json)
					{
						if (optionsSafe)
						{
							result.Append($"\t{sep}{ident} = case when {optionRowAlias0}.value('@{attName}', 'nvarchar(max)') = \'NULL\' then null else {optionRowAlias0}.value('@{attName}', '{typ}') end{Environment.NewLine}");
						}
						else
						{
							result.Append($"\t{sep}{ident} = {optionRowAlias0}.value('@{attName}', '{typ}'){Environment.NewLine}");
						}
					}
					else
					{
                        result.Append($"\t{sep}{ident} = {optionRowAlias0}.{attName}{Environment.NewLine}");
                        jsonWith.Add($"{sep}{attName} {typ} '$.{attName}'");
                    }
					if (String.IsNullOrEmpty(sep)) sep = ", ";
				}
			}
			if (!option_json)
			{
				string path = "/root[1]/data[1]/row";
				if (option_tab_path) path = $"/root[1]/{tableName0}/row";
				result.Append($"from @{optionSource0}.nodes('{path}') as t({optionRowAlias0}){Environment.NewLine}");
			}
            else
			{
                result.Append($"from openjson(@json) with ({Environment.NewLine}");
				foreach (var ss in jsonWith)
					result.Append($"\t{ss}{Environment.NewLine}");
                result.Append($") {optionRowAlias0}{Environment.NewLine}");
            }

			if (option_identity_insert)
			{
				result.Append($"{Environment.NewLine}set identity_insert {tableName} off;");
			}

			return result.ToString();
		}
	}
}
