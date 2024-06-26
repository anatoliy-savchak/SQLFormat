﻿using Microsoft.SqlServer.TransactSql.ScriptDom;
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
        public string VarMsgName;

        public SqlDomBuilder(TSqlScript script, SqlBuilder sqlBuilder)
        { 
            _sqlBuilder = sqlBuilder;
        }

        public void ProduceFullTableRenameScript(TSqlScript script, string suffix, bool reverse = false)
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
                        string tableName = TSQLHelper.Identifiers2Value(createTableStatement.SchemaObjectName.Identifiers) + (reverse ? suffix : "");
                        string tableNewName = TSQLHelper.Identifiers2ValueLast(createTableStatement.SchemaObjectName.Identifiers) + (!reverse ? suffix : "");
                        string schemaName = TSQLHelper.Identifier2Value(createTableStatement.SchemaObjectName.Identifiers[0]);
                        objType = OBJECT;
                        _sqlBuilder.AppendSpRename(tableName, tableNewName, objType, "TABLE").NL();

                        foreach (ConstraintDefinition constraint in createTableStatement.Definition.TableConstraints)
                        {
                            //((UniqueConstraintDefinition)constraint)
                            var conName = TSQLHelper.Identifier2Value(constraint.ConstraintIdentifier);
                            objName = !reverse ? $"{schemaName}.{conName}" : $"{schemaName}.{conName + suffix}";
                            newName = reverse ? conName : conName + suffix;
                            objType = OBJECT;
                            _sqlBuilder.AppendSpRename(objName, newName, objType, "constraint").NL();
                        }

                        foreach (var colDef in createTableStatement.Definition.ColumnDefinitions)
                        {
                            foreach (var constraint in colDef.Constraints)
                            {
                                if (constraint.ConstraintIdentifier == null) continue;
                                var conName = TSQLHelper.Identifier2Value(constraint.ConstraintIdentifier);
                                objName = !reverse ? $"{schemaName}.{conName}" : $"{schemaName}.{conName + suffix}";
                                newName = reverse ? conName : conName + suffix;
                                objType = OBJECT;
                                _sqlBuilder.AppendSpRename(objName, newName, objType, "constraint").NL();
                            }

                            {
                                var constraint = colDef.DefaultConstraint;
                                if (constraint == null || constraint.ConstraintIdentifier == null) continue;
                                var conName = TSQLHelper.Identifier2Value(constraint.ConstraintIdentifier);
                                objName = !reverse ? $"{schemaName}.{conName}" : $"{schemaName}.{conName + suffix}";
                                newName = reverse ? conName : conName + suffix;
                                objType = OBJECT;
                                _sqlBuilder.AppendSpRename(objName, newName, objType, "constraint").NL();
                            }

                        }

                    }
                    if (statement is CreateIndexStatement createIndexStatement)
                    {
                        var indexName = TSQLHelper.Identifier2Value(createIndexStatement.Name);
                        var tableNameFull = TSQLHelper.Identifiers2Value(createIndexStatement.OnName.Identifiers);
                        // we already renamed table
                        objName = $"{tableNameFull + (!reverse ? suffix : "")}.{indexName}" + (reverse ? suffix : ""); ;
                        newName = indexName + (!reverse ? suffix : "");
                        objType = "INDEX";
                        _sqlBuilder.AppendSpRename(objName, newName, objType, "INDEX").NL();
                    }
                }
        }

        public void ProduceFullTableRenameContent(TSqlScript script, string suffix, string content)
        {

            foreach (var batch in script.Batches)
                foreach (var statement in batch.Statements)
                {
                    if (statement is CreateTableStatement createTableStatement)
                    {
                        string tableName = TSQLHelper.Identifiers2ValueLast(createTableStatement.SchemaObjectName.Identifiers);
                        string tableNewName = tableName + suffix;
                        content = content.Replace($"[{tableName}]", $"[{tableNewName}]");
                        foreach (var constraint in createTableStatement.Definition.TableConstraints)
                        {
                            var cname = TSQLHelper.Identifier2Value(constraint.ConstraintIdentifier);
                            var cnameNew = cname + suffix;
                            content = content.Replace($"[{cname}]", $"[{cnameNew}]");
                        }
                        foreach (var colDef in createTableStatement.Definition.ColumnDefinitions)
                        {
                            foreach (var constraint in colDef.Constraints)
                            {
                                if (constraint.ConstraintIdentifier == null) continue;
                                var cname = TSQLHelper.Identifier2Value(constraint.ConstraintIdentifier);
                                var cnameNew = cname + suffix;
                                content = content.Replace($"[{cname}]", $"[{cnameNew}]");
                            }
                            
                            {
                                var constraint = colDef.DefaultConstraint;
                                if (constraint == null || constraint.ConstraintIdentifier == null) continue;
                                var cname = TSQLHelper.Identifier2Value(constraint.ConstraintIdentifier);
                                var cnameNew = cname + suffix;
                                content = content.Replace($"[{cname}]", $"[{cnameNew}]");
                            }

                        }
                    }
                    if (statement is CreateIndexStatement createIndexStatement)
                    {
                        var indexName = TSQLHelper.Identifier2Value(createIndexStatement.Name);
                        var indexNameNew = indexName + suffix; ;
                        content = content.Replace($"[{indexName}]", $"[{indexNameNew}]");
                    }
                }
            _sqlBuilder.AppendText(content);
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

        private struct ColInfo {
            public int Index;
            public string ColumnName;
            public ColumnDefinition ColumnDefinition;
            public string ColumnTypeStr;
            public string VarNameLastValue;
        }

        public void ProduceCopyTable(CreateTableStatement createTableStatement, string varSuffix, string targetTableNameFull = null, int omitLastKeyCount = 0)
        {
            string sourceTableNameFull = TSQLHelper.Identifiers2Value(createTableStatement.SchemaObjectName.Identifiers);
            string destTableNameFull = targetTableNameFull != null ? targetTableNameFull : TSQLHelper.Identifiers2Value(createTableStatement.SchemaObjectName.Identifiers);

            UniqueConstraintDefinition clusteredKey = createTableStatement.Definition.TableConstraints.Where(c => c is UniqueConstraintDefinition uni && uni.Clustered == true).SingleOrDefault() as UniqueConstraintDefinition;
            if (clusteredKey is null)
            {
                _sqlBuilder.AppendLine("Error: No clustered index!");
                return;
            }
            ColInfo lastColumn;
            var firstColumns = new List<ColInfo>();
            {
                ColInfo? lastColumnN = null;
                for (int i = 0; i < clusteredKey.Columns.Count; i++)
                {
                    var colName = TSQLHelper.Identifiers2Value(clusteredKey.Columns[i].Column.MultiPartIdentifier.Identifiers);
                    ColumnDefinition column = createTableStatement.Definition.ColumnDefinitions.Where(c => TSQLHelper.Identifier2Value(c.ColumnIdentifier).ToLowerInvariant() == colName.ToLowerInvariant()).Single();
                    var colInfo = new ColInfo();
                    colInfo.Index = i;
                    colInfo.ColumnName = colName;
                    colInfo.ColumnDefinition = column;
                    colInfo.ColumnTypeStr = TSQLHelper.Column2TypeStr(column);
                    colInfo.VarNameLastValue = $"@Last{colName}{varSuffix}";
                    if (i == clusteredKey.Columns.Count - 1 - omitLastKeyCount)
                    {
                        lastColumnN = colInfo;
                        break;
                    }
                    firstColumns.Add(colInfo);
                }
                if (lastColumnN == null)
                {
                    _sqlBuilder.AppendLine("Error: No clustered last column!");
                    return;
                }
                lastColumn = lastColumnN.Value;
            }

            int batchSize = 5000;
            string batchSizeName = $"@BatchSize{varSuffix}";
            _sqlBuilder.AppendLine($"declare {batchSizeName} int = {batchSize};");

            string affectedInBatchName = $"@AffectedInBatch{varSuffix}";
            _sqlBuilder.AppendLine($"declare {affectedInBatchName} int;");

            string batchNumName = $"@BatchNum{varSuffix}";
            _sqlBuilder.AppendLine($"declare {batchNumName} int = 0;");

            string totalCountName = $"@TotalCount{varSuffix}";
            _sqlBuilder.AppendLine($"declare {totalCountName} int = (select count(*) from {sourceTableNameFull} with (nolock));");

            string expectedBatchCountName = $"@ExpectedBatchCount{varSuffix}";
            _sqlBuilder.AppendLine($"declare {expectedBatchCountName} int = {totalCountName} / {batchSizeName} + 1;");

            Print($"concat('Total count:', {totalCountName}, ', batch count: ', {expectedBatchCountName}, ', size: ', {batchSizeName}, ' in {sourceTableNameFull}.')", false);

            // last column via table
            string batchtableName = $"#batch{varSuffix}";
            _sqlBuilder.AppendLine($"drop table if exists {batchtableName};")
                .AppendLine($"create table {batchtableName}(")
                .Indent()
                .AppendLine($"{lastColumn.ColumnName} {lastColumn.ColumnTypeStr} not null primary key clustered")
                .Unindent()
                .AppendLine(");").NL();

            foreach (var info in firstColumns)
            {
                _sqlBuilder.AppendLine($"declare {info.VarNameLastValue} {info.ColumnTypeStr};");
            }

            _sqlBuilder.AppendLine($"declare {lastColumn.VarNameLastValue} {lastColumn.ColumnTypeStr};");


            _sqlBuilder.NL();

            for(int i = 0; i < firstColumns.Count; i++)
            {
                var info = firstColumns[i];
                _sqlBuilder.AppendLine($"while 1=1 -- {info.ColumnName}").AppendBegin();

                _sqlBuilder.AppendLine($"if {info.VarNameLastValue} is null").AppendBegin();
                {
                    _sqlBuilder.AppendLine($"set {info.VarNameLastValue} = (").Indent()
                        .AppendLine($"select top (1) t.{info.ColumnName}")
                        .AppendLine($"from {sourceTableNameFull} t")
                        .Append($"where ");
                    for (int j = i - 1; j > -1; j--)
                    {
                        _sqlBuilder.Append($"({firstColumns[j].VarNameLastValue} = t.{firstColumns[j].ColumnName}) and ", true);
                    }
                    _sqlBuilder.Append($"(1=1)", true)
                        .AppendLine("", true)
                        .AppendLine("order by 1 asc").Unindent().AppendLine(");").NL()
                        ;
                    _sqlBuilder.AppendEnd() ;
                }
                _sqlBuilder.AppendLine("else").AppendBegin();
                {

                    _sqlBuilder.AppendLine($"set {info.VarNameLastValue} = (").Indent()
                        .AppendLine($"select top (1) t.{info.ColumnName}")
                        .AppendLine($"from {sourceTableNameFull} t")
                        .Append($"where ");
                    for (int j = i - 1; j > -1; j--)
                    {
                        _sqlBuilder.Append($"({firstColumns[j].VarNameLastValue} = t.{firstColumns[j].ColumnName}) and ", true);
                    }
                    _sqlBuilder.Append($"(t.{info.ColumnName} > {info.VarNameLastValue})", true)
                        .AppendLine("", true)
                        .AppendLine("order by 1 asc").Unindent().AppendLine(");").NL()
                        ;
                    _sqlBuilder.AppendEnd();
                }

                _sqlBuilder.AppendLine($"if {info.VarNameLastValue} is null").AppendBegin().AppendLine("break;").AppendEnd().NL();


                Print($"concat('Processing {info.ColumnName}:', {info.VarNameLastValue})", false);
                if (i < firstColumns.Count - 1)
                {
                    _sqlBuilder.AppendLine($"set {firstColumns[i+1].VarNameLastValue} = null;").NL();
                }

            }
            _sqlBuilder.AppendLine($"set {lastColumn.VarNameLastValue} = null;").NL();
            _sqlBuilder.AppendLine("while 1=1");
            _sqlBuilder.AppendBegin();
            {
                _sqlBuilder.AppendLine($"truncate table {batchtableName};").NL();

                string distinct = omitLastKeyCount > 0 ? "distinct " : "";
                _sqlBuilder.AppendLine($"insert into {batchtableName}({lastColumn.ColumnName})")
                    .AppendLine($"select top ({batchSizeName}) t.{lastColumn.ColumnName}")
                    .AppendLine($"from (").Indent()
                    .AppendLine($"select {distinct}t.{lastColumn.ColumnName}")
                    .AppendLine($"from {sourceTableNameFull} t")
                    .Append($"where ");
                foreach(var col in firstColumns)
                {
                    _sqlBuilder.Append($"({col.VarNameLastValue} = t.{col.ColumnName}) and ", true);
                }

                _sqlBuilder.Append($"({lastColumn.VarNameLastValue} is null or t.{lastColumn.ColumnName} > {lastColumn.VarNameLastValue})", true)
                    .AppendLine("", true)
                    .Unindent().AppendLine(") t")
                    .AppendLine("order by 1 asc;").NL()
                    ;

                _sqlBuilder.AppendLine($"set {affectedInBatchName} = @@rowcount;").NL();

                _sqlBuilder.AppendIf($"{affectedInBatchName} = 0")
                    .Indent()
                    .AppendLine("break;")
                    .Unindent().NL();

                _sqlBuilder.AppendLine($"set {batchNumName} += 1;").NL();
                Print($"concat('Merge batch: ', {batchNumName}, ' / ', {batchSizeName})", false);
                _sqlBuilder.AppendLine($"set {lastColumn.VarNameLastValue} = (select max(t.{lastColumn.ColumnName}) from {batchtableName} t);").NL();

                SqlDomBuilderMerge sqlDomBuilderMerge = new SqlDomBuilderMerge(_sqlBuilder);
                string whereLineSource = "where ";
                string whereLineDest = "where ";
                foreach (var col in firstColumns)
                {
                    whereLineSource += $"({col.VarNameLastValue} = s.{col.ColumnName}) and ";
                    whereLineDest += $"({col.VarNameLastValue} = t.{col.ColumnName}) and ";
                }
                whereLineSource += $"s.{lastColumn.ColumnName} in (select b.{lastColumn.ColumnName} from {batchtableName} b)";
                whereLineDest += $"t.{lastColumn.ColumnName} in (select b.{lastColumn.ColumnName} from {batchtableName} b)";
                sqlDomBuilderMerge.whereLineSource = whereLineSource;
                sqlDomBuilderMerge.whereLineDest = whereLineDest;
                sqlDomBuilderMerge.ProduceMerge(createTableStatement, createTableStatement, targetTableNameFull: destTableNameFull);

                _sqlBuilder.AppendEnd();
            }
            foreach (var info in firstColumns.OrderByDescending(x=>x.Index))
            {
                _sqlBuilder.AppendEnd($"end -- {info.ColumnName}");
            }

        }

        private void PrintErr(string msg, string varname, bool addQuotes = true)
        {
            if (addQuotes) msg = $"'{msg}'";
            _sqlBuilder.AppendLine($"set {varname} = {msg}; raiserror({varname}, 10, 1) with nowait;");
        }

        public SqlBuilder Print(string msg, bool addQuotes = true)
        {
            if (VarMsgName != null)
                PrintErr(msg, this.VarMsgName, addQuotes);
            return _sqlBuilder;
        }

        public void ProduceCopytableVerify(CreateTableStatement createTableStatement, string varSuffix, string sourcetTableNameFull = null, string targetTableNameFull = null)
        {
            string sourceTableNameFull = sourcetTableNameFull ?? TSQLHelper.Identifiers2Value(createTableStatement.SchemaObjectName.Identifiers);
            string destTableNameFull = targetTableNameFull ?? TSQLHelper.Identifiers2Value(createTableStatement.SchemaObjectName.Identifiers);

            UniqueConstraintDefinition clusteredKey = createTableStatement.Definition.TableConstraints.Where(c => c is UniqueConstraintDefinition uni && uni.Clustered == true).SingleOrDefault() as UniqueConstraintDefinition;
            if (clusteredKey is null)
            {
                _sqlBuilder.AppendLine("Error: No clustered index!");
                return;
            }
            var columns = new List<ColInfo>();
            {
                for (int i = 0; i < clusteredKey.Columns.Count; i++)
                {
                    var colName = TSQLHelper.Identifiers2Value(clusteredKey.Columns[i].Column.MultiPartIdentifier.Identifiers);
                    ColumnDefinition column = createTableStatement.Definition.ColumnDefinitions.Where(c => TSQLHelper.Identifier2Value(c.ColumnIdentifier).ToLowerInvariant() == colName.ToLowerInvariant()).Single();
                    var colInfo = new ColInfo();
                    colInfo.Index = i;
                    colInfo.ColumnName = colName;
                    columns.Add(colInfo);
                }
            }

            Print($"Comparing {sourceTableNameFull} with {destTableNameFull}...");

            _sqlBuilder
                .AppendLine("if exists(")
                .Indent()
                .AppendLine($"select * from {sourceTableNameFull} s")
                .AppendLine($"full outer join {destTableNameFull} t on (")
                .Indent()
                .AppendLines(columns.Select(c => $"s.{c.ColumnName} = t.{c.ColumnName}"), " and")
                .Unindent()
                .AppendLine(")")
                .AppendLine($"where s.{columns.First().ColumnName} is null or t.{columns.First().ColumnName} is null")
                .Unindent()
                .AppendLine(")")
                .AppendBegin()
                .AppendLine($";throw 51000, 'Differs {sourceTableNameFull} vs {destTableNameFull}!', 1;")
                .AppendEnd(addSpaceafterwards: false);
            _sqlBuilder.AppendLine("else").Indent();
            Print($"Success comparing {sourceTableNameFull} with {destTableNameFull}");
            _sqlBuilder.Unindent();
        }

        public void ProduceCopyTableSingle(CreateTableStatement createTableStatement, string varSuffix, string targetTableNameFull = null)
        {
            string sourceTableNameFull = TSQLHelper.Identifiers2Value(createTableStatement.SchemaObjectName.Identifiers);
            string destTableNameFull = targetTableNameFull != null ? targetTableNameFull : TSQLHelper.Identifiers2Value(createTableStatement.SchemaObjectName.Identifiers);

            UniqueConstraintDefinition clusteredKey = createTableStatement.Definition.TableConstraints.Where(c => c is UniqueConstraintDefinition uni && uni.Clustered == true).SingleOrDefault() as UniqueConstraintDefinition;
            if (clusteredKey is null)
            {
                _sqlBuilder.AppendLine("Error: No clustered index!");
                return;
            }
            ColInfo keyColumn = new ColInfo();
            {
                var colName = TSQLHelper.Identifiers2Value(clusteredKey.Columns[0].Column.MultiPartIdentifier.Identifiers);
                ColumnDefinition column = createTableStatement.Definition.ColumnDefinitions.Where(c => TSQLHelper.Identifier2Value(c.ColumnIdentifier).ToLowerInvariant() == colName.ToLowerInvariant()).Single();
                keyColumn.Index = 0;
                keyColumn.ColumnName = colName;
                keyColumn.ColumnDefinition = column;
                keyColumn.ColumnTypeStr = TSQLHelper.Column2TypeStr(column);
                keyColumn.VarNameLastValue = $"@Key{varSuffix}";
            }

            string AffectedIterationName = $"@AffectedIteration{varSuffix}";
            _sqlBuilder.AppendLine($"declare {AffectedIterationName} int;");

            string numName = $"@i{varSuffix}";
            _sqlBuilder.AppendLine($"declare {numName} int = 0;");

            string totalCountName = $"@TotalCount{varSuffix}";
            _sqlBuilder.AppendLine($"declare {totalCountName} int = (select count(*) from {sourceTableNameFull} with (nolock));").NL();


            // last column via table
            string keysTableName = $"#keys{varSuffix}";
            _sqlBuilder.AppendLine($"drop table if exists {keysTableName};")
                .AppendLine($"create table {keysTableName}(")
                .Indent()
                .AppendLine($"{keyColumn.ColumnName} {keyColumn.ColumnTypeStr} not null primary key clustered")
                .Unindent()
                .AppendLine(");").NL();


            _sqlBuilder.AppendLine($"declare {keyColumn.VarNameLastValue} {keyColumn.ColumnTypeStr};");

            _sqlBuilder.NL();
            _sqlBuilder.AppendLine($"insert into {keysTableName}({keyColumn.ColumnName})");
            _sqlBuilder.AppendLine($"select distinct t.{keyColumn.ColumnName}");
            _sqlBuilder.AppendLine($"from {sourceTableNameFull} t with (nolock);").NL();

            string loopCountName = $"@LoopCount{varSuffix}";
            _sqlBuilder.AppendLine($"declare {loopCountName} int = @@rowcount;");

            Print($"concat('Total count:', {totalCountName}, ', loop count: ', {loopCountName}, ' in {sourceTableNameFull}.')", false);
            _sqlBuilder.NL();

            _sqlBuilder.AppendLine($"while 1=1").AppendBegin();
            {
                _sqlBuilder.AppendLine($"set {numName} += 1;").NL();

                _sqlBuilder.AppendLine($"set {keyColumn.VarNameLastValue} = (").Indent()
                    .AppendLine($"select top (1) t.{keyColumn.ColumnName}")
                    .AppendLine($"from {keysTableName} t")
                    .AppendLine($"where {keyColumn.VarNameLastValue} is null or t.{keyColumn.ColumnName} > {keyColumn.VarNameLastValue}")
                    .AppendLine($"order by 1 asc")
                    .Unindent().AppendLine(");").NL();

                _sqlBuilder.AppendLine($"if {keyColumn.VarNameLastValue} is null").AppendBegin().AppendLine("break;").AppendEnd().NL();


                Print($"concat('Processing ', {numName},' / ', {loopCountName}, ', {keyColumn.ColumnName}: ', {keyColumn.VarNameLastValue}, ' ...')", false);

                _sqlBuilder.AppendLine($"insert into {destTableNameFull}(")
                    .Indent()
                    .AppendLines(createTableStatement.Definition.ColumnDefinitions.Select(c => TSQLHelper.Identifier2Value(c.ColumnIdentifier)), ",")
                    .Unindent()
                    .AppendLine(")")
                    .AppendLine($"select ")
                    .Indent()
                    .AppendLines(createTableStatement.Definition.ColumnDefinitions.Select(c => $"t.{TSQLHelper.Identifier2Value(c.ColumnIdentifier)}"), ",")
                    .Unindent()
                    .AppendLine($"from {sourceTableNameFull} t")
                    .AppendLine($"where t.{keyColumn.ColumnName} = {keyColumn.VarNameLastValue};").NL();

            }
            _sqlBuilder.AppendEnd($"end");
        }

    }
}
