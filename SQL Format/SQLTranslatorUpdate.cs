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
	public class SQLTranslatorUpdate : SQLTranslator
	{
		public override string GetCaption()
		{
			return "Update";
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
				label.Text = "Dest Alias: ";
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
			string optionAllias = null;
			if (options is Control)
			{
				var r = ((Control)options).Controls.Find("option_alias_src", true);
				if (r.Length > 0)
				{
					optionAllias = (r[0] as TextBox).Text;
				}
			}
			string optionAlliasDest = null;
			if (options is Control)
			{
				var r = ((Control)options).Controls.Find("option_alias_dest", true);
				if (r.Length > 0)
				{
					optionAlliasDest = (r[0] as TextBox).Text;
				}
			}

			if (!String.IsNullOrEmpty(optionAllias)) optionAllias = optionAllias + '.';
			if (!String.IsNullOrEmpty(optionAlliasDest)) optionAlliasDest = optionAlliasDest + '.';

			StringBuilder result = new StringBuilder();
			foreach (ColumnDefinition columnDefinition in tableDefinition.ColumnDefinitions)
			{
				string ident = TSQLHelper.Identifier2Value(columnDefinition.ColumnIdentifier);
				result.Append($"{optionAlliasDest}{ident} = {optionAllias}{ident},{Environment.NewLine}");
			}

			return result.ToString();
		}
	}
}
