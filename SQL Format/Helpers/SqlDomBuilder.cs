using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQL_Format.Helpers
{
    public class SqlDomBuilder
    {
        private readonly SqlBuilder _sqlBuilder;

        public SqlDomBuilder(TSqlScript script, SqlBuilder sqlBuilder)
        { 
            _sqlBuilder = sqlBuilder;
        }

        public void ProduceFullTableRenameContent(TSqlScript script, string suffix)
        {
            string objName;
            string newName;
            string objType;
            const string OBJECT = "OBJECT";

            foreach (var batch in script.Batches)
                foreach (var statement in batch.Statements)
                {
                    if (statement is CreateTableStatement createTableStatement)
                    {
                        string tableName = TSQLHelper.Identifiers2Value(createTableStatement.SchemaObjectName.Identifiers);
                        string tableNewName = TSQLHelper.Identifiers2ValueLast(createTableStatement.SchemaObjectName.Identifiers) + suffix;
                        string schemaName = TSQLHelper.Identifier2Value(createTableStatement.SchemaObjectName.Identifiers[0]);
                        objType = OBJECT;
                        _sqlBuilder.AppendSpRename(tableName, tableNewName, objType, "TABLE").NL();

                        foreach (ConstraintDefinition constraint in createTableStatement.Definition.TableConstraints)
                        {
                            //((UniqueConstraintDefinition)constraint)
                            newName = TSQLHelper.Identifier2Value(constraint.ConstraintIdentifier);
                            objName = $"{schemaName}.{newName}";
                            newName += suffix;
                            objType = OBJECT;
                            _sqlBuilder.AppendSpRename(objName, newName, objType, "constraint").NL();
                            //_sqlBuilder.AppendLine($"{TSQLHelper.Identifier2Value(constraint.ConstraintIdentifier)}: {constraint.GetType().FullName}");

                        }
                    }
                    if (statement is CreateIndexStatement createIndexStatement)
                    {
                        newName = TSQLHelper.Identifier2Value(createIndexStatement.Name);
                        objName = $"{TSQLHelper.Identifiers2Value(createIndexStatement.OnName.Identifiers)}.{newName}";
                        newName += suffix;
                        objType = "INDEX";
                        _sqlBuilder.AppendSpRename(objName, newName, objType, "INDEX").NL();
                    }
                }
        }

        public void ProduceTableCreateStatement(string content)
        {
            content = content.Replace("\r\n", "\n");
            var lines = content.Split('\n');
            //StringBuilder sb = new StringBuilder();
            int initialLine = -1;
            int i = -1;
            foreach (var line in lines)
            {
                i++;
                var l = line.ToLowerInvariant();
                if (l.StartsWith("create table"))
                {
                    initialLine = i;
                    _sqlBuilder.AppendLine(line);
                    continue;
                }
                if (initialLine < 0) continue;
                if (l.Trim() == "go")
                    break;
                _sqlBuilder.AppendLine(line);
                if (line.EndsWith(";"))
                    break;
            }
        }

        public void ProduceCopyTable(CreateTableStatement createTableStatement, string varSuffix)
        {
            string sourceTableNameFull = TSQLHelper.Identifiers2Value(createTableStatement.SchemaObjectName.Identifiers);
            string destTableNameFull = TSQLHelper.Identifiers2Value(createTableStatement.SchemaObjectName.Identifiers);

            UniqueConstraintDefinition primaryKey = 
                createTableStatement.Definition.TableConstraints.Where(c => c is UniqueConstraintDefinition uni && uni.IsPrimaryKey).Single() as UniqueConstraintDefinition;

            var lastColumnName = TSQLHelper.Identifiers2Value(primaryKey.Columns.Last().Column.MultiPartIdentifier.Identifiers);
            var lastColumn = createTableStatement.Definition.ColumnDefinitions.Where(c => TSQLHelper.Identifier2Value(c.ColumnIdentifier).ToLowerInvariant() == lastColumnName.ToLowerInvariant()).Single();
            var lastColumnTypeStr = TSQLHelper.Column2TypeStr(lastColumn);

            // last column via table
            string batchtableName = $"#batch{varSuffix}";
            _sqlBuilder.AppendLine($"drop table if exists {batchtableName};")
                .AppendLine($"create table {batchtableName}(")
                .Indent()
                .AppendLine($"{lastColumnName} {lastColumnTypeStr} not null primary key clustered")
                .Unindent()
                .AppendLine(");").NL();

            string lastColumnNameVar = $"@Last{lastColumnName}{varSuffix}";
            _sqlBuilder.AppendLine($"declare {lastColumnNameVar} {lastColumnTypeStr};");

            int batchSize = 5000;
            string batchSizeName = $"@BatchSize{varSuffix}";
            _sqlBuilder.AppendLine($"declare {batchSizeName} int = {batchSize};");

            string affectedInBatchName = $"@AffectedInBatch{varSuffix}";
            _sqlBuilder.AppendLine($"declare {affectedInBatchName} int;");

            _sqlBuilder.NL();

            _sqlBuilder.AppendLine("while 1=1");
            _sqlBuilder.AppendBegin();
            {
                _sqlBuilder.AppendLine($"truncate table {batchtableName};").NL();

                _sqlBuilder.AppendLine($"insert into {batchtableName}({lastColumnName})")
                    .AppendLine($"select top ({batchSizeName}) t.{lastColumnName}")
                    .AppendLine($"from {sourceTableNameFull} t")
                    .Append($"where ")
                    .Append($"({lastColumnNameVar} is null or t.{lastColumnName} > {lastColumnNameVar})", true)
                    .AppendLine("", true)
                    .AppendLine("order by 1 asc;").NL()
                    ;

                _sqlBuilder.AppendLine($"set {affectedInBatchName} = @@rowcount;").NL();

                _sqlBuilder.AppendIf($"{affectedInBatchName} = 0")
                    .Indent()
                    .AppendLine("break;")
                    .Unindent().NL();

                _sqlBuilder.AppendLine($"set {lastColumnNameVar} = (select max(t.{lastColumnName}) from {batchtableName} t);").NL();

                SqlDomBuilderMerge sqlDomBuilderMerge = new SqlDomBuilderMerge(_sqlBuilder);
                sqlDomBuilderMerge.whereLineSource = $"where s.{lastColumnName} in (select b.{lastColumnName} from {batchtableName} b)";
                sqlDomBuilderMerge.ProduceMerge(createTableStatement, createTableStatement);

                _sqlBuilder.AppendEnd();
            }
        }
    }
}
