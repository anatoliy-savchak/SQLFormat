using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SQL_Format.Helpers;

namespace SQL_Format
{
	public class SQLTranslatorCopy : SQLTranslator
	{
		public override string GetCaption()
		{
			return "Copy Table";
		}

		public override void SetupOptionsContent(Control Parent, EventHandler changedHandler)
		{
        }

        public override string TranslateExt2(CreateTableStatement createTableStatement, object options, string content, TSqlScript sqlScript)
        {
            SqlBuilder result = new SqlBuilder();
			SqlDomBuilder sqlDomBuilder = new SqlDomBuilder(sqlScript, result);
            SqlDomBuilderMerge sqlDomBuilderMerge = new SqlDomBuilderMerge(result);

            string tableSuffix = "_new";
            result.TableNameFull = TSQLHelper.Identifiers2Value(createTableStatement.SchemaObjectName.Identifiers);
            result.TableName = TSQLHelper.Identifiers2ValueLast(createTableStatement.SchemaObjectName.Identifiers);
            //string tableNameOld = result.TableName + tableSuffix;
            //string tableNameFullOld = result.TableNameFull + tableSuffix;
            string tableNameNew = result.TableName + tableSuffix;
            string tableNameFullNew = result.TableNameFull + tableSuffix;
            {
				result.AppendBegin(SqlBuilder.BEGIN_TRY);
				{
                    //sqlDomBuilderMerge.use_output = true;
                    //sqlDomBuilderMerge.use_intesect = true;

                    //sqlDomBuilderMerge.ProduceMerge(createTableStatement, createTableStatement);
                    //sqlDomBuilderMerge.Test(createTableStatement);
                    /*
					result.AppendIf($"object_id('{tableNameFullOld}') is null").AppendBegin();
					{
						result.AppendLine("-- rename to old table");
                        sqlDomBuilder.ProduceFullTableRenameContent(sqlScript, tableSuffix);

                        result.AppendEnd();
                    }
                    */

                    result.AppendIf($"object_id('{tableNameFullNew}') is null").AppendBegin();
                    {
                        result.AppendLine("-- re-create table");
                        //result.AppendText(content);
                        sqlDomBuilder.ProduceFullTableRenameContent(sqlScript, tableSuffix, content);

                        result.AppendEnd();
                    }
                    
                    sqlDomBuilder.ProduceCopyTable(createTableStatement, "9", targetTableNameFull: tableNameFullNew);


                    result.AppendEnd(SqlBuilder.END_TRY, false);
                }
                result.AppendCatchTypicalRollback();
			}
            return result.ToString();
		}
	}
}
