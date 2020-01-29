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
	public class SQLTranslatorMerge : SQLTranslator
	{
		public override string GetCaption()
		{
			return "Merge";
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
				checkBox.Text = "insert only";
				checkBox.Name = "option_insertonly";
				checkBox.Checked = false;
				checkBox.CheckedChanged += changedHandler;
				Parent.Controls.Add(checkBox);
			}

			{
				CheckBox checkBox = new CheckBox();
				checkBox.Text = "use star in CTE";
				checkBox.Name = "use_star";
				checkBox.Checked = false;
				checkBox.CheckedChanged += changedHandler;
				Parent.Controls.Add(checkBox);
			}
			{

				CheckBox checkBox = new CheckBox();
				checkBox.Text = "use intesect";
				checkBox.Name = "use_intesect";
				checkBox.Checked = false;
				checkBox.CheckedChanged += changedHandler;
				Parent.Controls.Add(checkBox);
			}
            {

                CheckBox checkBox = new CheckBox();
                checkBox.Text = "use output";
                checkBox.Name = "use_output";
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

			bool option_insertonly = false;
			if (options is Control)
			{
				var r = ((Control)options).Controls.Find("option_insertonly", true);
				if (r.Length > 0)
				{
					option_insertonly = (r[0] as CheckBox).Checked;
				}
			}

			bool use_star = false;
			if (options is Control)
			{
				var r = ((Control)options).Controls.Find("use_star", true);
				if (r.Length > 0)
				{
					use_star = (r[0] as CheckBox).Checked;
				}
			}

			bool use_intesect = false;
			if (options is Control)
			{
				var r = ((Control)options).Controls.Find("use_intesect", true);
				if (r.Length > 0)
				{
					use_intesect = (r[0] as CheckBox).Checked;
				}
			}

            bool use_output = false;
            if (options is Control)
            {
                var r = ((Control)options).Controls.Find("use_output", true);
                if (r.Length > 0)
                {
                    use_output = (r[0] as CheckBox).Checked;
                }
            }

            string optionAllias = optionAllias0;
			string optionAlliasDest = optionAlliasDest0;
			if (!String.IsNullOrEmpty(optionAllias)) optionAllias = optionAllias + '.';
			if (!String.IsNullOrEmpty(optionAlliasDest)) optionAlliasDest = optionAlliasDest + '.';
			string tableName = TSQLHelper.Identifiers2Value(createTableStatement.SchemaObjectName.Identifiers);
			

			string firstColumnIdent = null;

			StringBuilder result = new StringBuilder();
			string sep = null;

            if (use_output)
            {
                result.Append($"declare @mergeOutput table([action] nvarchar(10)");
                sep = ", ";
                foreach (ColumnDefinition columnDefinition in tableDefinition.ColumnDefinitions)
                {
                    string ident = TSQLHelper.Identifier2Value(columnDefinition.ColumnIdentifier);
                    if (!TSQLHelper.ColumnIsPrimaryKey(columnDefinition, tableDefinition, true)) continue;
                    string ctype = TSQLHelper.Column2TypeStr(columnDefinition);
                    result.Append($"{sep}{ident} {ctype}");
                    if (String.IsNullOrEmpty(sep))
                    {
                        sep = ", ";
                    }
                }
                result.Append($");{Environment.NewLine}");

            }

            // with src as
            {
				result.Append($";with src as ({Environment.NewLine}");
				if (!use_star)
				{
					result.Append($"\tselect{Environment.NewLine}");
					sep = null;
					foreach (ColumnDefinition columnDefinition in tableDefinition.ColumnDefinitions)
					{
						string ident = TSQLHelper.Identifier2Value(columnDefinition.ColumnIdentifier);
						if (String.IsNullOrEmpty(firstColumnIdent)) firstColumnIdent = ident;
						result.Append($"\t\t{sep}{ident} = {optionAllias}{ident}{Environment.NewLine}");
						if (String.IsNullOrEmpty(sep)) sep = ", ";
					}
				} else
				{
					result.Append($"\tselect *{Environment.NewLine}");
				}
				result.Append($"\tfrom {tableName} {optionAllias0}{Environment.NewLine}");
				result.Append($"){Environment.NewLine}");
			}
			// , targ as
			{
				result.Append($", targ as ({Environment.NewLine}");
				if (!use_star)
				{

					result.Append($"\tselect{Environment.NewLine}");
					sep = null;
					foreach (ColumnDefinition columnDefinition in tableDefinition.ColumnDefinitions)
					{
						string ident = TSQLHelper.Identifier2Value(columnDefinition.ColumnIdentifier);
						result.Append($"\t\t{sep}{optionAlliasDest}{ident}{Environment.NewLine}");
						if (String.IsNullOrEmpty(sep)) sep = ", ";
					}
				}
				else
				{
					result.Append($"\tselect *{Environment.NewLine}");
				}
				result.Append($"\tfrom {tableName} {optionAlliasDest0}{Environment.NewLine}");
				result.Append($"){Environment.NewLine}");
			}
			//merge
			{
				result.Append($"merge targ as {optionAlliasDest0}{Environment.NewLine}");
				result.Append($"using src as {optionAllias0}{Environment.NewLine}");
				sep = null;
                bool onFound = false;
				foreach (ColumnDefinition columnDefinition in tableDefinition.ColumnDefinitions)
				{
					string ident = TSQLHelper.Identifier2Value(columnDefinition.ColumnIdentifier);
					if (!TSQLHelper.ColumnIsPrimaryKey(columnDefinition, tableDefinition)) continue;
                    onFound = true;
					if (String.IsNullOrEmpty(sep)) {
						sep = ", ";
						result.Append($"on (");
					} else
					{
						result.Append($" and ");
					}
					result.Append($"{optionAlliasDest}{ident} = {optionAllias}{ident}");
				}
                if (!onFound)
                    foreach (ColumnDefinition columnDefinition in tableDefinition.ColumnDefinitions)
                    {
                        string ident = TSQLHelper.Identifier2Value(columnDefinition.ColumnIdentifier);
                        if (!TSQLHelper.ColumnIsPrimaryKey(columnDefinition, tableDefinition, true)) continue;
                        onFound = true;
                        if (String.IsNullOrEmpty(sep))
                        {
                            sep = ", ";
                            result.Append($"on (");
                        }
                        else
                        {
                            result.Append($" and ");
                        }
                        result.Append($"{optionAlliasDest}{ident} = {optionAllias}{ident}");
                    }


                if (String.IsNullOrEmpty(sep))
				{
					result.Append($"on ({optionAlliasDest}{firstColumnIdent} = {optionAllias}{firstColumnIdent}){Environment.NewLine}");
				} else
				{
					result.Append($"){Environment.NewLine}");
				}
			}
			//insert
			{
				result.Append($"when not matched by target then{Environment.NewLine}");
				result.Append($"\tinsert({Environment.NewLine}");
				sep = null;
				foreach (ColumnDefinition columnDefinition in tableDefinition.ColumnDefinitions)
				{
					if (TSQLHelper.ColumnIsIdentity(columnDefinition)) continue;
					string ident = TSQLHelper.Identifier2Value(columnDefinition.ColumnIdentifier);
                    result.Append($"\t\t{sep}{ident}{Environment.NewLine}");
					if (String.IsNullOrEmpty(sep)) sep = ", ";
				}
				result.Append($"\t){Environment.NewLine}");
				result.Append($"\tvalues({Environment.NewLine}");
				sep = null;
				foreach (ColumnDefinition columnDefinition in tableDefinition.ColumnDefinitions)
				{
					if (TSQLHelper.ColumnIsIdentity(columnDefinition)) continue;
					string ident = TSQLHelper.Identifier2Value(columnDefinition.ColumnIdentifier);
					result.Append($"\t\t{sep}{optionAllias}{ident}{Environment.NewLine}");
					if (string.IsNullOrEmpty(sep)) sep = ", ";
				}
				result.Append($"\t){Environment.NewLine}");
			}
			//update
			if (!option_insertonly)
			{
				if (use_intesect)
				{
					result.Append($"when matched and not exists({Environment.NewLine}");
					result.Append($"\tselect ");
					sep = null;
					foreach (ColumnDefinition columnDefinition in tableDefinition.ColumnDefinitions)
					{
						if (TSQLHelper.ColumnIsIdentity(columnDefinition)) continue;
						if (TSQLHelper.ColumnIsPrimaryKey(columnDefinition, tableDefinition)) continue;
						string ident = TSQLHelper.Identifier2Value(columnDefinition.ColumnIdentifier);
						result.Append($"{sep}{optionAlliasDest}{ident}");
						if (String.IsNullOrEmpty(sep)) sep = ", ";
					}
					result.Append($"{Environment.NewLine}");
					result.Append($"\tintersect{Environment.NewLine}");
					result.Append($"\tselect ");
					sep = null;
					foreach (ColumnDefinition columnDefinition in tableDefinition.ColumnDefinitions)
					{
						if (TSQLHelper.ColumnIsIdentity(columnDefinition)) continue;
						if (TSQLHelper.ColumnIsPrimaryKey(columnDefinition, tableDefinition)) continue;
						string ident = TSQLHelper.Identifier2Value(columnDefinition.ColumnIdentifier);
						result.Append($"{sep}{optionAllias}{ident}");
						if (String.IsNullOrEmpty(sep)) sep = ", ";
					}
					result.Append($"{Environment.NewLine}) then{Environment.NewLine}");
				}
				else
				{
					result.Append($"when matched then{Environment.NewLine}");
				}
				result.Append($"\tupdate set{Environment.NewLine}");
				sep = null;
				foreach (ColumnDefinition columnDefinition in tableDefinition.ColumnDefinitions)
				{
					if (TSQLHelper.ColumnIsIdentity(columnDefinition)) continue;
					if (TSQLHelper.ColumnIsPrimaryKey(columnDefinition, tableDefinition)) continue;
					string ident = TSQLHelper.Identifier2Value(columnDefinition.ColumnIdentifier);
					result.Append($"\t\t{sep}{ident} = {optionAllias}{ident}{Environment.NewLine}");
					if (String.IsNullOrEmpty(sep)) sep = ", ";
				}
			}

			//delete
			if (!option_insertonly)
			{
				result.Append($"when not matched by source then{Environment.NewLine}");
				result.Append($"\tdelete{Environment.NewLine}");
			}

            if (use_output)
            {
                result.Append($"output $action");
                sep = ", ";
                foreach (ColumnDefinition columnDefinition in tableDefinition.ColumnDefinitions)
                {
                    string ident = TSQLHelper.Identifier2Value(columnDefinition.ColumnIdentifier);
                    if (!TSQLHelper.ColumnIsPrimaryKey(columnDefinition, tableDefinition, true)) continue;
                    result.Append($"{sep}inserted.{ident}");
                    if (String.IsNullOrEmpty(sep))
                    {
                        sep = ", ";
                    }
                }
                result.Append($"{Environment.NewLine}");

                result.Append($"into @mergeOutput(action");
                sep = ", ";
                foreach (ColumnDefinition columnDefinition in tableDefinition.ColumnDefinitions)
                {
                    string ident = TSQLHelper.Identifier2Value(columnDefinition.ColumnIdentifier);
                    if (!TSQLHelper.ColumnIsPrimaryKey(columnDefinition, tableDefinition, true)) continue;
                    result.Append($"{sep}{ident}");
                    if (String.IsNullOrEmpty(sep))
                    {
                        sep = ", ";
                    }
                }
                result.Append($"){Environment.NewLine}");
            }

            result.Append($";{Environment.NewLine}");
			return result.ToString();
		}
	}
}
