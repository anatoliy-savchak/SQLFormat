using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQL_Format
{
	public static class TSQLHelper
	{
		public static string Identifier2Value(Identifier identifier)
		{
			if (identifier == null) return null;
			/*
			if (identifier.QuoteType == QuoteType.SquareBracket)
			{
				return $"[{identifier.Value}]";
			}
			*/
			return identifier.Value;
		}

		public static string Identifiers2Value(IList<Identifier> identifiers)
		{
			if ((identifiers == null) || (identifiers.Count == 0)) return null;
			string result = Identifier2Value(identifiers[0]);
			for(int i = 1; i < identifiers.Count; i++)
				result = $"{result}.{Identifier2Value(identifiers[i])}";
			return result;
		}

		public static string Identifiers2ValueLast(IList<Identifier> identifiers)
		{
			if ((identifiers == null) || (identifiers.Count == 0)) return null;
			return Identifier2Value(identifiers[identifiers.Count - 1]);
		}

		public static string Column2TypeStr(ColumnDefinition columnDefinition)
		{
			if (columnDefinition.DataType == null) return null;
			string result = columnDefinition.DataType.Name.Identifiers[0].Value.ToLowerInvariant();
			//+		Name	{Microsoft.SqlServer.TransactSql.ScriptDom.SchemaObjectName}	Microsoft.SqlServer.TransactSql.ScriptDom.SchemaObjectName
			if (columnDefinition.DataType is ParameterizedDataTypeReference)
			{
				if (((ParameterizedDataTypeReference)columnDefinition.DataType).Parameters.Count > 0)
				{
					result = $"{result}({((ParameterizedDataTypeReference)columnDefinition.DataType).Parameters[0].Value})";
				}
			}

			return result;
		}

		public static bool ColumnIsDefault(ColumnDefinition columnDefinition)
		{
			if (columnDefinition.IdentityOptions != null)
			{
				return true;
			}
			if (columnDefinition.DefaultConstraint != null)
			{
				return true;
			}

			return false;
		}

		public static bool ColumnIsIdentity(ColumnDefinition columnDefinition)
		{
			if (columnDefinition.IdentityOptions != null)
			{
				return true;
			}

			return false;
		}

		public static bool ColumnIsPrimaryKey(ColumnDefinition columnDefinition, TableDefinition definition, bool AllowUnique = false)
		{
			if ((columnDefinition.Index != null) && (columnDefinition.Index.IndexType != null) && (columnDefinition.Index.IndexType.IndexTypeKind == IndexTypeKind.Clustered))
			{
				return true;
			}

            if ((AllowUnique) && (columnDefinition.Constraints.Count > 0))
            {
                foreach(var c in columnDefinition.Constraints)
                {
                    if (c is Microsoft.SqlServer.TransactSql.ScriptDom.UniqueConstraintDefinition)
                        return true;
                }
            }

			if ((definition.TableConstraints != null) && (definition.TableConstraints.Count > 0))
			{
				string ident = TSQLHelper.Identifier2Value(columnDefinition.ColumnIdentifier);
				foreach (var c in definition.TableConstraints)
				{
					if (!(c is UniqueConstraintDefinition)) continue;
					if (((UniqueConstraintDefinition)c).IsPrimaryKey)
					{

						foreach (var co in ((UniqueConstraintDefinition)c).Columns)
						{
							string s = Identifiers2ValueLast(co.Column.MultiPartIdentifier.Identifiers);
							if (s.ToLowerInvariant() == ident.ToLowerInvariant()) return true;
						}
					}
				}
			}

			return false;
		}
	}
}
