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

			string optionAllias = optionAllias0;
			string optionAlliasDest = optionAlliasDest0;
			if (!String.IsNullOrEmpty(optionAllias)) optionAllias = optionAllias + '.';
			if (!String.IsNullOrEmpty(optionAlliasDest)) optionAlliasDest = optionAlliasDest + '.';

			string firstColumnIdent = null;

			StringBuilder result = new StringBuilder();
			string sep = null;
			// with src as
			{
				result.Append($";with src as ({Environment.NewLine}");
				result.Append($"\tselect{Environment.NewLine}");
				sep = null;
				foreach (ColumnDefinition columnDefinition in tableDefinition.ColumnDefinitions)
				{
					string ident = TSQLHelper.Identifier2Value(columnDefinition.ColumnIdentifier);
					if (String.IsNullOrEmpty(firstColumnIdent)) firstColumnIdent = ident;
					result.Append($"\t\t{sep}{ident} = {optionAllias}{ident}{Environment.NewLine}");
					if (String.IsNullOrEmpty(sep)) sep = ", ";
				}
				result.Append($"\tfrom {optionAllias0}{Environment.NewLine}");
				result.Append($"){Environment.NewLine}");
			}
			// , targ as
			{
				result.Append($", targ as ({Environment.NewLine}");
				result.Append($"\tselect{Environment.NewLine}");
				sep = null;
				foreach (ColumnDefinition columnDefinition in tableDefinition.ColumnDefinitions)
				{
					string ident = TSQLHelper.Identifier2Value(columnDefinition.ColumnIdentifier);
					result.Append($"\t\t{sep}{optionAlliasDest}{ident}{Environment.NewLine}");
					if (String.IsNullOrEmpty(sep)) sep = ", ";
				}
				result.Append($"\tfrom {optionAlliasDest0}{Environment.NewLine}");
				result.Append($"){Environment.NewLine}");
			}
			//merge
			{
				result.Append($"merge targ as {optionAlliasDest0}{Environment.NewLine}");
				result.Append($"using src as {optionAllias0}{Environment.NewLine}");
				result.Append($"on ({optionAlliasDest}{firstColumnIdent} = {optionAllias}{firstColumnIdent}){Environment.NewLine}");
			}
			//insert
			{
				result.Append($"when not matched by target then{Environment.NewLine}");
				result.Append($"\tinsert({Environment.NewLine}");
				sep = null;
				foreach (ColumnDefinition columnDefinition in tableDefinition.ColumnDefinitions)
				{
					string ident = TSQLHelper.Identifier2Value(columnDefinition.ColumnIdentifier);
					result.Append($"\t\t{sep}{ident}{Environment.NewLine}");
					if (String.IsNullOrEmpty(sep)) sep = ", ";
				}
				result.Append($"\t){Environment.NewLine}");
				result.Append($"\tvalues({Environment.NewLine}");
				sep = null;
				foreach (ColumnDefinition columnDefinition in tableDefinition.ColumnDefinitions)
				{
					string ident = TSQLHelper.Identifier2Value(columnDefinition.ColumnIdentifier);
					result.Append($"\t\t{sep}{optionAllias}{ident}{Environment.NewLine}");
					if (string.IsNullOrEmpty(sep)) sep = ", ";
				}
				result.Append($"\t){Environment.NewLine}");
			}
			//update
			{
				result.Append($"when matched then{Environment.NewLine}");
				result.Append($"\tupdate set{Environment.NewLine}");
				sep = null;
				foreach (ColumnDefinition columnDefinition in tableDefinition.ColumnDefinitions)
				{
					string ident = TSQLHelper.Identifier2Value(columnDefinition.ColumnIdentifier);
					result.Append($"\t\t{sep}{ident} = {optionAllias}{ident}{Environment.NewLine}");
					if (String.IsNullOrEmpty(sep)) sep = ", ";
				}
			}

			//delete
			{
				result.Append($"when not matched by source then{Environment.NewLine}");
				result.Append($"\tdelete{Environment.NewLine}");
			}

			result.Append($";{Environment.NewLine}");
			return result.ToString();
		}
	}
}
