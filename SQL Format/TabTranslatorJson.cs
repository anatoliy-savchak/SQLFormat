using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace SQL_Format
{
	public class TabTranslatorJson : SQLTranslator
	{
        static Regex csvSplit = new Regex("(?:^|\t)(\"(?:[^\"])*\"|[^\t]*)", RegexOptions.Compiled);
        public override string GetCaption()
		{
			return "Excel paste to Json";
		}

		public override void SetupOptionsContent(Control Parent, EventHandler changedHandler)
		{
			this.AddOptionCheckBox("Produce SQL", "produce_sql", true, Parent, changedHandler);
            this.AddOptionCheckBox("SQL using CTE", "sql_cte", true, Parent, changedHandler);
            this.AddOptionCheckBox("nvarchar(max)", "sql_max", false, Parent, changedHandler);
        }

        public override string TranslateText(string text, object options)
        {
            text = text.Trim(Convert.ToChar(13));
            var tabLines = text.Split(Convert.ToChar(10));
			var columns = new List<ColumnInfo>();
			var columnsInitialized = false;

			JsonArray root = new JsonArray();

			int lineNumber = -1;
			foreach (var line in tabLines)
			{
                lineNumber++;
				if (string.IsNullOrWhiteSpace(line)) continue;

                JsonObject rec = new JsonObject();
				int colIndex = -1;
                foreach (Match match in csvSplit.Matches(line))
				{
					string value = match.Value.TrimStart('\t').TrimEnd('\r');
					colIndex++;

                    if (!columnsInitialized)
					{
						var col = new ColumnInfo()
						{
							Name = value,
						};
						columns.Add(col);
					}
					else
					{
						int maxLen = value.Length;
						if (columns[colIndex].MaxLen < maxLen)
							columns[colIndex].MaxLen = maxLen;

                        rec.Add(columns[colIndex].Name, JsonValue.Create(value));
                    }
                }
				if (columnsInitialized)
					root.Add(rec);
                columnsInitialized = true;
            }

			var opts = new System.Text.Json.JsonSerializerOptions(){ WriteIndented = true};
			opts.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
			string json = root.ToJsonString(opts);

			string result = json;

            var produce_sql = this.GetOptionBool("produce_sql", options);
			if (produce_sql==true)
			{
				var sql_cte = this.GetOptionBool("sql_cte", options);
                var sql_max = this.GetOptionBool("sql_max", options);
                
                string jsonSql = json.Replace("'", "''");

                StringBuilder sb = new StringBuilder();

				if (sql_cte == true)
				{
                    string indent = "\t";
                    sb.AppendLine(";with js as (");
                    sb.AppendLine($"{indent}select json = N'");
                    sb.AppendLine(jsonSql);
                    sb.AppendLine("')");

					sb.AppendLine(", src as (");
					sb.AppendLine($"{indent}select");
                    {
                        string fwComma = ", ";
                        int i = -1;
                        foreach (var col in columns)
                        {
                            i++;
                            fwComma = i > 0 ? ", " : "";

                            sb.AppendLine($"{indent}\t{fwComma}[{col.Name}] = s.[{col.Name}]");
                        }
                    }
                    sb.AppendLine("from js");
                    sb.AppendLine("cross apply openjson(js.json) with (");
                    {
                        int i = -1;
                        foreach (var col in columns)
                        {
                            i++;
                            string fwComma = i > 0 ? ", " : "";
                            string maxlen = sql_max == true ? "max" : col.MaxLen.ToString();
                            sb.AppendLine($"{indent}{fwComma}[{col.Name}] nvarchar({maxlen}) '$.\"{col.Name}\"'");
                        }
                    }
                    sb.AppendLine($"{indent}) s");
                    sb.AppendLine(")");
                    sb.AppendLine("select");
                    {
                        string fwComma = ", ";
                        int i = -1;
                        foreach (var col in columns)
                        {
                            i++;
                            fwComma = i > 0 ? ", " : "";

                            sb.AppendLine($"{indent}{fwComma}[{col.Name}] = s.[{col.Name}]");
                        }
                    }
                    sb.AppendLine("from src s");
                }
                else
				{
					sb.AppendLine("declare @j nvarchar(max) = N'");
					sb.AppendLine(jsonSql);
					sb.AppendLine("';");

					sb.AppendLine();

					sb.AppendLine("select");
					string indent = "\t";
					string fwComma = ", ";
					{
						int i = -1;
						foreach (var col in columns)
						{
							i++;
							fwComma = i > 0 ? ", " : "";

							sb.AppendLine($"{indent}{fwComma}t.[{col.Name}]");
						}
					}
					sb.AppendLine("from openjson(@j) with (");
					{
						int i = -1;
						foreach (var col in columns)
						{
							i++;
							fwComma = i > 0 ? ", " : "";
                            string maxlen = sql_max == true ? "max" : col.MaxLen.ToString();
                            sb.AppendLine($"{indent}{fwComma}[{col.Name}] nvarchar({maxlen}) '$.\"{col.Name}\"'");
						}
					}
					sb.AppendLine(") t");
				}
                result = sb.ToString();
			}


            return result;
        }

		private class ColumnInfo
		{
			internal string Name;
			internal int MaxLen = 0;
		}
    }
}
